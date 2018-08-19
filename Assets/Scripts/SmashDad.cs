using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmashDad : MonoBehaviour {

    public GameObject player;
    bool smashed = false;

    void Awake() {

    }

    void Start () {
        
    }
    
    // Update is called once per frame
    void Update () {
        
    }

    public void NotifySmashed() {
        if (!smashed) {
            smashed = true;
            player.GetComponent<PlayerController>().smashCount++;
        }
    }
}
