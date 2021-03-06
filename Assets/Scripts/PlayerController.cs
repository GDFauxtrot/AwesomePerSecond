﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour {

    [Header("Movement")]
    public float playerSpeed;
    public float airControlMultiplier;
    public float jumpForce;

    Vector2 mouseInput, moveInput;
    Vector2 mouseRotation;

    bool grounded;

    [Header("Camera")]
    public Vector2 cameraDistanceMinMax;
    public float cameraDistanceStart;
    public float cameraSensitivity;

    public GameObject cameraTarget;
    public float startCameraYRotation;
    public float cameraDistanceScrollAmount;

    float cameraDistance;
    Camera camera;

    [Header("Ground Pound")]

    public float groundPoundSpeed;
    public float groundPoundForce;
    public float groundPoundAirLift;
    public float groundPoundRadius;
    public GameObject groundPoundEffectPrefab;
    bool groundPounding;

    [Header("Audio")]
    public AudioClip ballRollingClip;
    public List<AudioClip> clips;
    public float rollVolume;
    public float rollVolumeScale;

    AudioSource rollSource;

    [Header("Misc")]
    public Rigidbody rb;
    public SphereCollider sphere;

    public bool drawDebug;

    Collider[] overlapCols; // using nonalloc

    private Renderer rend;
    private float previousYvelocity;
    public AnimationCurve squash;

    public int smashCount;

    void Awake() {
        rb.maxAngularVelocity = Mathf.Infinity; // BOI WE SPINNIN NOW

        // Initial camera business
        camera = Camera.main;
        cameraDistance = cameraDistanceStart;
        mouseRotation = new Vector2(0, startCameraYRotation);
        RotateCameraToTarget();

        overlapCols = new Collider[8]; // Or however many we need

        rend = GetComponent<Renderer>();
        rend.material.shader = Shader.Find("Custom/VertexAnim");

        rollSource = gameObject.AddComponent<AudioSource>();
        rollSource.clip = ballRollingClip;
        rollSource.volume = rollVolume;
        rollSource.loop = true;
        rollSource.Play();
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();

        // Gather inputs
        mouseInput = new Vector2(
            Input.GetAxisRaw("Mouse X") + Input.GetAxisRaw("Joy X"),
            -Input.GetAxisRaw("Mouse Y") + Input.GetAxisRaw("Joy Y"));
        moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        mouseRotation += mouseInput * cameraSensitivity;
        bool jumpPressed = Input.GetButtonDown("Jump");
        bool groundPoundPressed = Input.GetButtonDown("Ground Pound");

        #region CAMERA

        // Rotating by mouse delta every frame is wonky? - reset rotation and apply full mouse rotation at once
        cameraTarget.transform.eulerAngles = Vector3.zero;

        // Lock rotation at major up/down (so you aren't looking upside-down behind you)
        mouseRotation.y = Mathf.Clamp(mouseRotation.y, -90, 90);
        cameraTarget.transform.Rotate(mouseRotation.y, mouseRotation.x, 0);

        // Scroll in/out w/ scroll wheel
        cameraDistance += -Input.GetAxisRaw("Mouse ScrollWheel") * cameraDistanceScrollAmount;
        cameraDistance = Mathf.Clamp(cameraDistance, cameraDistanceMinMax.x, cameraDistanceMinMax.y);

        #endregion
        #region MOVEMENT

        // Gather camera forward and right (ignoring up/down look)
        Vector3 forceForward = new Vector3(camera.transform.forward.x, 0, camera.transform.forward.z);
        Vector3 forceRight = new Vector3(camera.transform.right.x, 0, camera.transform.right.z);

        if (drawDebug) {
            Debug.DrawRay(transform.position, forceForward, Color.blue);
            Debug.DrawRay(transform.position, forceRight, Color.red);
        }

        // Apply rotation force to player
        rb.AddTorque(
            forceForward * -moveInput.x * playerSpeed * (60*Time.deltaTime) +
            forceRight * moveInput.y * playerSpeed * (60*Time.deltaTime));

        // Air control
        if (!grounded) {
            rb.AddForce(
                forceForward * moveInput.y * playerSpeed * airControlMultiplier * (60*Time.deltaTime) +
                forceRight * moveInput.x * playerSpeed * airControlMultiplier * (60*Time.deltaTime));
        }
        
        // Jump impulse
        if (jumpPressed && grounded) {
            grounded = false;
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        // Activate ground pound motion
        if (groundPoundPressed && !grounded && !groundPounding) {
            if (-rb.velocity.y < groundPoundSpeed)
                rb.velocity = Vector3.down * groundPoundSpeed;
            else
                rb.velocity = Vector3.down * -rb.velocity.y;
            groundPounding = true;
        }

        #endregion
        #region COLLISION STUFF

        // Query around us to see if we're grounded or not (need the 0.001f added too, corner collisions not counted otherwise)
        int overlapCount = Physics.OverlapSphereNonAlloc(transform.position, sphere.radius + 0.001f, overlapCols, ~(1 << LayerMask.NameToLayer("Player")));

        grounded = false; // until proven otherwise

        for (int i = 0; i < overlapCount; ++i) {
            // This won't give us a collision point and normal... so let's just make our own, why not
            Vector3 colPoint = overlapCols[i].ClosestPoint(transform.position);
            Vector3 colNormal = (transform.position - colPoint).normalized;

            if (drawDebug) {
                Debug.DrawRay(colPoint, colNormal, Color.magenta);
            }

            // This amount can be changed later (89.9 degree slopes will techically count as "grounded" in this state)
            if (colNormal.y > 0)
                grounded = true;
        }

        // Ground pound impact
        if (grounded && groundPounding) {
            groundPounding = false;

            GameObject effect = Instantiate(groundPoundEffectPrefab, transform.position, Quaternion.identity);
            effect.GetComponent<GroundPoundEffect>().PlayEffect();

            Collider[] poundedObjects = Physics.OverlapSphere(transform.position, groundPoundRadius, 1 << LayerMask.NameToLayer("GroundPoundable"));
            foreach (Collider obj in poundedObjects) {

                // Make sure the object isn't obscured by another one (floor/wall/ceiling/etc)
                bool objectVisible = true;
                Vector3 ray = obj.transform.position - transform.position;
                RaycastHit[] hits = Physics.RaycastAll(transform.position, ray.normalized, ray.magnitude);
                foreach (RaycastHit hit in hits) {
                    if (hit.collider.gameObject.layer != LayerMask.NameToLayer("GroundPoundable"))
                        objectVisible = false;
                }

                if (objectVisible) {
                    Rigidbody objRB = obj.gameObject.GetComponent<Rigidbody>();
                    if (objRB) {
                        float v = Mathf.Max(-rb.velocity.y, groundPoundSpeed); // sometimes it no fast
                        objRB.AddExplosionForce(v * groundPoundForce, transform.position, groundPoundRadius, groundPoundAirLift);
                    } else {
                        Debug.LogWarning("Ground poundable object '" + obj.name + "' has no rigidbody!");
                    }
                }
            }
        }

        #endregion
        #region BALL ROLLING AUDIO THINGO

        float ourSpeed = rb.velocity.magnitude;
        float ballVol = Mathf.Lerp(0, rollVolume, ourSpeed/rollVolumeScale);
        rollSource.volume = ballVol;
        rollSource.pitch = ourSpeed/rollVolumeScale * 1.5f;

        #endregion
        #region Material editing

        // // finding terminal velocity
        // Vector3 addedForce = forceForward * -moveInput.x * playerSpeed * (60*Time.deltaTime) +
        //                     forceRight * moveInput.y * playerSpeed * (60*Time.deltaTime);
        // float topVelocity = ((addedForce.magnitude / rb.drag) - 
        //                         Time.fixedDeltaTime * addedForce.magnitude) / 
        //                         rb.mass;

        rend.material.SetFloat("_disc",(rb.velocity.magnitude/25));

        Vector3 clampedVelocity= new Vector3 (clampNeg(rb.velocity.x),
                                            clampNeg(rb.velocity.y),
                                            clampNeg(rb.velocity.z));
        rend.material.SetVector("_Speed",(rb.velocity));

        if(previousYvelocity<=0 && rb.velocity.y>=0)
            StartCoroutine(AnimateSquash(.5f));
        #endregion
    }

    private float clampNeg(float velocity)
    {
        return Mathf.Clamp(velocity,-10,10);
    }
    IEnumerator AnimateSquash(float duration)
    {
         float journey = 0f;
        while (journey <= duration)
        {
            journey = journey + Time.deltaTime;
            float percent = Mathf.Clamp01(journey / duration);
            float curvePercent = squash.Evaluate(percent);
            rend.material.SetFloat("_Squashing",curvePercent);
            yield return null;

        }
    }   
    void LateUpdate() {
        RotateCameraToTarget();
        previousYvelocity = rb.velocity.y;
    }

    void OnDrawGizmos() {
        // Ground pound radius
        if (drawDebug) {
            Gizmos.color = new Color(1f, 0.5f, 0f, 1f);
            Gizmos.DrawWireSphere(transform.position, groundPoundRadius);
        }
    }

    void OnTriggerEnter(Collider col) {
        if (col.gameObject.layer == LayerMask.NameToLayer("TriggerEnd"))
            SceneManager.LoadScene("MainMenu");
    }

    /// <summary>
    /// Sets camera position and the rotation to the target, and then moves backwards the desired distance.
    /// </summary>
    private void RotateCameraToTarget() {
        camera.transform.position = cameraTarget.transform.position;
        camera.transform.rotation = cameraTarget.transform.rotation;

        RaycastHit hit;
        
        if (Physics.Raycast(cameraTarget.transform.position, -camera.transform.forward, out hit, cameraDistance)) {
            camera.transform.position = hit.point + (camera.transform.forward * 0.1f);
        } else {
            camera.transform.position -= camera.transform.forward * cameraDistance;
        }
        
    }
}
