using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum MessageType
{
    DEATH,
    BIRTH,
    DEFAULT,
}

public class ActivityLog : MonoBehaviour
{
    [SerializeField] private Button deathsButon;
    [SerializeField] private Button birthsButon;
    [SerializeField] private Button clearMessagesButon;
    [SerializeField] private VerticalLayoutGroup messagesConteiner;
    [SerializeField] private ActivityLogMessage messagePrefab;

    private Dictionary<MessageType, List<string>> _messages;
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

    private void ChosseMessagesToShow(MessageType messageType)
    {
        foreach (string message in _messages[messageType])
        {
            ShowMessage(message);
        }
    }

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
