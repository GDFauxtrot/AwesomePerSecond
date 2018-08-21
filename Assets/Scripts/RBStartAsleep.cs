using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RBStartAsleep : MonoBehaviour {

    GameObject dad;
    Rigidbody rb;
    bool toldDad = false;

    void Awake() {
        rb = GetComponent<Rigidbody>();
        if(SceneManager.GetActiveScene().name != "MainMenu")
            rb.Sleep();

        dad = gameObject.transform.parent.gameObject;
    }

    void Update() {
        if (!toldDad && !rb.IsSleeping()) {
            toldDad = true;
            dad.GetComponent<SmashDad>().NotifySmashed(rb.velocity.magnitude);
        }
    }
}
