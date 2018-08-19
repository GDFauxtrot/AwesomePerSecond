using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UILoading  : MonoBehaviour {

	public GameObject intro;
	public GameObject credits;
	public void loadLevel(string level)
	{
		SceneManager.LoadScene(level); 
	}

	public void loadCredits()
	{
		intro.SetActive(false);
		credits.SetActive(true);
	}

	public void loadintro()
	{
		intro.SetActive(true);
		credits.SetActive(false);
	}

	public void exit()
	{
		  Application.Quit();
	}
}
