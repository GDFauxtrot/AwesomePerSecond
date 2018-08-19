using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fling : MonoBehaviour {

 	public List<GameObject> objList;
	public GameObject location;
	// Use this for initialization
	void Start () {
		
		StartCoroutine(Flinger());
	}
	
	// Update is called once per frame
	IEnumerator Flinger()
	{
		while(true){
			var obj = Instantiate(objList[Random.Range(0,objList.Count)],transform);
		
			yield return new WaitForSeconds(3);
			Destroy(obj);
		}
	}
}
