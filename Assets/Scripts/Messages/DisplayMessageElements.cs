using System.Collections.Generic;
using UnityEngine;

public class DisplayMessageElements : MonoBehaviour
{
	public MessageEvent messagePanelEvent;
	public List<MessageElement> messageElements = new List<MessageElement>();

    private void Awake()
    {
        foreach (MessageElement storyElement in messageElements)
        {
            storyElement.currentActivations = 0;
        }
    }

    public void DisplayStory()
	{
		foreach (MessageElement storyElement in messageElements)
		{
			if (storyElement.IsConditionMet())
			{
				storyElement.currentActivations++;
				foreach (Message message in storyElement.messages)
				{
					messagePanelEvent.Display(message);
				}
			}
		}
	}
}