using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioSlider : MonoBehaviour
{
    UISoundManager soundManager;
    Slider slider;

    private void Awake() {
        soundManager = FindObjectOfType<UISoundManager>();
        slider = GetComponent<Slider>();
        soundManager.AssignSlider(slider);
        slider.onValueChanged.AddListener(delegate { soundManager.VolumeControl(slider.value); });
    }
}
