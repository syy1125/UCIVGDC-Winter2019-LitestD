using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowMessages : MonoBehaviour
{
    public MessageInterface messageInterface;
    public GameEvent gameEvent;
    public List<Message> messages = new List<Message>();

    private int continuesLeft;

    private void Awake()
    {
        foreach (Message message in messages)
        {
            message.ResetTimesShown();
        }

        messageInterface.ContinueMade += EnableContinue;
        continuesLeft = 0;
    }

    public void EnableContinue()
    { 
        if (messages.Contains(messageInterface.CurrentMessage()))
        {
            messageInterface.ContinueMade -= EnableContinue;
            gameEvent.AddListener(Continue);
        }
    }

    public void EnqueueAllMessages()
    {
        foreach (Message message in messages)
        {
            messageInterface.Enqueue(message);
        }
    }

    public void Continue()
    {
        messageInterface.Continue();

        continuesLeft--;
        if (continuesLeft <= 0)
        {
            gameEvent.RemoveListener(Continue);
        }
    }

    public void Continue(int numberOfTimes)
    {
        for (int i = 0; i < numberOfTimes; i++)
        {
            messageInterface.Continue();
        }
    }

    public void Clear()
    {
        messageInterface.Clear();
    }
}
