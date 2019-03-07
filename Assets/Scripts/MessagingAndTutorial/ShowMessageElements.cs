using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowMessageElements : MonoBehaviour
{
    public MessageInterface messageInterface;
    public GameEvent gameEvent;
    public List<MessageElement> messageElements = new List<MessageElement>();

    private void Awake()
    {
        gameEvent.AddListener(Show);

        foreach (MessageElement messageElement in messageElements)
        {
            foreach (Message message in messageElement.messages)
            {
                message.ResetTimesShown();
            }
        }
    }

    public void Show()
    {
        foreach (MessageElement messageElement in messageElements)
        {
            if (messageElement.IsConditionMet())
            {
                foreach (Message message in messageElement.messages)
                {
                    messageInterface.Enqueue(message);
                }
            }
        }
    }
}
