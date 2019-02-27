using System.Collections.Generic;

public class StoryManager : GameEventListener
{
	public MessageEvent messagePanelEvent;
	public IntReference turnCount;
	public List<MessageElement> storyElements = new List<MessageElement>();

    private void Awake()
    {
        foreach (MessageElement storyElement in storyElements)
        {
            storyElement.currentActivations = 0;
        }
    }

    public void DisplayStory()
	{
		foreach (MessageElement storyElement in storyElements)
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