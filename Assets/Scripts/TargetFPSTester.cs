using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TargetFPSTester : MonoBehaviour {

    public Slider fpsSlider;
    public Text fpsText;
    void Awake() {
        
    }
    
    void Update() {
        Application.targetFrameRate = (int) fpsSlider.value;
        fpsText.text = fpsSlider.value.ToString();
    }
}
