using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explode : MonoBehaviour {

	// Use this for initialization
	public Vector3 endSize;
	
	public float lifetime;
	private float killTime;
	public float intensity;

	public Renderer rend;
    void Start() {
        rend = GetComponent<Renderer>();
        rend.material.shader = Shader.Find("FX/CONCUSIVE");
		killTime=lifetime;
    }
	// Update is called once per frame
	void Update () {
		transform.localScale = Vector3.Lerp(endSize,new Vector3(1f,1f,1f), killTime/lifetime);
		rend.material.SetFloat("_noiseIntensity",Mathf.Lerp(0,intensity, killTime/lifetime));
		killTime-=Time.deltaTime;
		if(killTime<=0)
		{
			Destroy(gameObject);
		}
	}
}
