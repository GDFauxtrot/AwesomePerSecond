using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RBStartAsleep : MonoBehaviour {

    void Awake() {
        GetComponent<Rigidbody>().Sleep();
    }
}
