using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ActivityLogMessage : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI messageText;

    public void SetMessageText(string message) => messageText.text = message;
}
