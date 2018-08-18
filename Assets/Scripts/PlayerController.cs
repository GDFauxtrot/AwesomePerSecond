using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    [Header("Movement")]
    public float playerSpeed;
    public float airControlMultiplier;
    public float jumpForce;

    Vector2 mouseInput, moveInput;
    Vector2 mouseRotation;

    [Header("Camera")]
    public Vector2 cameraDistanceMinMax;
    public float cameraDistanceStart;
    public float cameraSensitivity;

    public GameObject cameraTarget;
    public float startCameraYRotation;
    public float cameraDistanceScrollAmount;

    float cameraDistance;
    Camera camera;

    [Header("Misc")]
    public Rigidbody rb;
    public SphereCollider sphere;
    public bool drawDebugLines;
    bool grounded;

    Collider[] overlapCols; // using nonalloc

    void Awake() {
        rb.maxAngularVelocity = Mathf.Infinity; // BOI WE SPINNIN NOW

        // Initial camera business
        camera = Camera.main;
        cameraDistance = cameraDistanceStart;
        mouseRotation = new Vector2(0, startCameraYRotation);
        RotateCameraToTarget();

        overlapCols = new Collider[8]; // Or however many we need
    }

    void Update() {
        // Gather inputs
        mouseInput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
        moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        mouseRotation += mouseInput * cameraSensitivity;
        bool jumpPressed = Input.GetButtonDown("Jump");

        #region CAMERA

        // Rotating by mouse delta every frame is wonky? - reset rotation and apply full mouse rotation at once
        cameraTarget.transform.eulerAngles = Vector3.zero;

        // Lock rotation at major up/down (so you aren't looking upside-down behind you)
        mouseRotation.y = Mathf.Clamp(mouseRotation.y, -90, 90);
        cameraTarget.transform.Rotate(-mouseRotation.y, mouseRotation.x, 0);

        // Scroll in/out w/ scroll wheel
        cameraDistance += -Input.GetAxisRaw("Mouse ScrollWheel") * cameraDistanceScrollAmount;
        cameraDistance = Mathf.Clamp(cameraDistance, cameraDistanceMinMax.x, cameraDistanceMinMax.y);

        #endregion
        #region MOVEMENT

        // Gather camera forward and right (ignoring up/down look)
        Vector3 forceForward = new Vector3(camera.transform.forward.x, 0, camera.transform.forward.z);
        Vector3 forceRight = new Vector3(camera.transform.right.x, 0, camera.transform.right.z);

        if (drawDebugLines) {
            Debug.DrawRay(transform.position, forceForward, Color.red);
            Debug.DrawRay(transform.position, forceRight, Color.blue);
        }

        // Apply rotation force to player
        rb.AddTorque(
            forceForward * -moveInput.x * playerSpeed +
            forceRight * moveInput.y * playerSpeed);

        // Air control
        if (!grounded) {
            rb.AddForce(
                forceForward * moveInput.y * playerSpeed * airControlMultiplier +
                forceRight * moveInput.x * playerSpeed * airControlMultiplier);
        }
        // Jump impulse
        if (jumpPressed && grounded) {
            grounded = false;
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
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

            if (drawDebugLines) {
                Debug.DrawRay(colPoint, colNormal, Color.magenta);
            }

            // This amount can be changed later (89.9 degree slopes will techically count as "grounded" in this state)
            if (colNormal.y > 0)
                grounded = true;
        }

        #endregion
    }

    void LateUpdate() {
        RotateCameraToTarget();
    }

    /// <summary>
    /// Sets camera position and the rotation to the target, and then moves backwards the desired distance.
    /// </summary>
    private void RotateCameraToTarget() {
        camera.transform.position = cameraTarget.transform.position;
        camera.transform.rotation = cameraTarget.transform.rotation;
        camera.transform.position -= camera.transform.forward * cameraDistance;
    }
}
