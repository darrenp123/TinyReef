using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AssignUIFunctions : MonoBehaviour
{
    UISoundManager soundManager;
    [SerializeField]
    Button button;
    [SerializeField]
    Slider slider;

    private void Awake() {
        soundManager = FindObjectOfType<UISoundManager>();
        button.onClick.AddListener(soundManager.Mute);
        soundManager.AssignSlider(slider);
        slider.onValueChanged.AddListener(delegate { soundManager.VolumeControl(slider.value); });
    }
}
