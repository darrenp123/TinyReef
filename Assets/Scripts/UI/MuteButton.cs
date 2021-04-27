using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MuteButton : MonoBehaviour
{
    UISoundManager soundManager;
    Button button;

    private void Awake() {
        soundManager = FindObjectOfType<UISoundManager>();
        button = GetComponent<Button>();
        button.onClick.AddListener(soundManager.Mute);
    }
}
