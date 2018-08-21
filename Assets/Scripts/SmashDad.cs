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

    public void NotifySmashed(float aForce) {
        if (!smashed) {
            Debug.Log(aForce);
            smashed = true;
            player.GetComponent<PlayerController>().smashCount++;
            List<AudioClip> clips = player.GetComponent<PlayerController>().clips;
            AudioSource.PlayClipAtPoint(clips[Random.Range(0, clips.Count)], transform.position);
        }
    }
}
