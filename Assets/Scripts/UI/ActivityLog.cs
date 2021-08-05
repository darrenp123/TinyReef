
// UI class for the activity log
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Every message as a type, this is important when we want to select a specific type of message to see
public enum MessageType
{
    DEATH,
    BIRTH,
    DEFAULT,
}

public class ActivityLog : MonoBehaviour
{
    // references to all the necessary UI elements
    [SerializeField] private Button deathsButon;
    [SerializeField] private Button birthsButon;
    [SerializeField] private Button clearMessagesButon;
    [SerializeField] private VerticalLayoutGroup messagesConteiner;
    [SerializeField] private ActivityLogMessage messagePrefab;

    // a list of all the string messages is saved along with their typing
    private Dictionary<MessageType, List<string>> _messages;
    // a list of all created UI messages elements saved for reuse. UI messages do not save their previous message,
    // they are changed as needed
    private List<ActivityLogMessage> messageObjPool;

    private void Awake()
    {
        _messages = new Dictionary<MessageType, List<string>>();
        foreach (MessageType messageType in Enum.GetValues(typeof(MessageType)))
        {
            _messages.Add(messageType, new List<string>());
        }

        messageObjPool = new List<ActivityLogMessage>();

        deathsButon.onClick.AddListener(ShowDeathMessages);
        birthsButon.onClick.AddListener(ShowBirthMessages);
        clearMessagesButon.onClick.AddListener(ClearAllMessages);
    }

    // method used by other classes to send a message to the activity log;
    public void SendMessage(MessageType messageType, string message)
    {
        ShowMessage(message);
        _messages[messageType].Add(message);
    }

    private void ShowDeathMessages()
    {
        ClearAllMessages();
        ChosseMessagesToShow(MessageType.DEATH);
    }

    private void ShowBirthMessages()
    {
        ClearAllMessages();
        ChosseMessagesToShow(MessageType.BIRTH);
    }

    // shows the messages from the chosen type
    private void ChosseMessagesToShow(MessageType messageType)
    {
        foreach (string message in _messages[messageType])
        {
            ShowMessage(message);
        }
    }

    // reuses UI message elements if available, other wise creates a new UI message element
    private void ShowMessage(string message)
    {
        ActivityLogMessage availableMessageOgj = null;
        foreach (ActivityLogMessage item in messageObjPool)
        {
            if (item.gameObject.activeSelf == false)
            {
                availableMessageOgj = item;
                break;
            }
        }

        if (availableMessageOgj)
        {
            availableMessageOgj.gameObject.SetActive(true);
        }
        else
        {
            availableMessageOgj = Instantiate(messagePrefab, messagesConteiner.transform);
            messageObjPool.Add(availableMessageOgj);
        }

        availableMessageOgj.SetMessageText(message);
    }

    private void ClearAllMessages()
    {
        foreach (ActivityLogMessage item in messageObjPool)
        {
            item.gameObject.SetActive(false);
        }
    }
}
