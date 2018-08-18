using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explode : MonoBehaviour {

	// Use this for initialization
	public Vector3 startSize;
	public Vector3 endSize;

	public Renderer rend;
    void Start() {
        rend = GetComponent<Renderer>();
        rend.material.shader = Shader.Find("FX/CONCUSIVE");
    }
	// Update is called once per frame
	void Update () {
		transform.localScale = Vector3.Lerp(startSize,endSize,Mathf.Sin(Time.time)*5);
		rend.material.SetFloat("_noiseIntensity",Mathf.Lerp(20,0,Mathf.Sin(Time.time)*5));
	}
}
