using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowMessageElements : MonoBehaviour
{
    public MessageInterface messageInterface;
    public GameEvent gameEvent;
    public List<StoryElement> messageElements = new List<StoryElement>();

    private void Awake()
    {
        gameEvent.AddListener(Show);

        foreach (StoryElement messageElement in messageElements)
        {
            foreach (Message message in messageElement.messages)
            {
                message.ResetTimesShown();
            }
        }
    }

    public void Show()
    {
        foreach (StoryElement messageElement in messageElements)
        {
            if (messageElement.condition.IsConditionMet())
            {
                foreach (Message message in messageElement.messages)
                {
                    messageInterface.Enqueue(message);
                }
            }
        }
    }
}
