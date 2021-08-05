
// UI class for the messages that show in the activity log
using TMPro;
using UnityEngine;

public class ActivityLogMessage : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI messageText;

    public void SetMessageText(string message) => messageText.text = message;
}
