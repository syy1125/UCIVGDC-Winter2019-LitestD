using System.Collections.Generic;

public class StoryManager : GameEventListener
{
	public MessageEvent messagePanelEvent;
	public IntReference turnCount;
	public List<StoryElement> storyElements = new List<StoryElement>();

    private void Awake()
    {
        foreach (StoryElement storyElement in storyElements)
        {
            storyElement.currentActivations = 0;
        }
    }

    public void DisplayStory()
	{
		foreach (StoryElement storyElement in storyElements)
		{
			if (storyElement.IsConditionMet())
			{
                print("Hello");
				storyElement.currentActivations++;
				foreach (Message message in storyElement.messages)
				{
					messagePanelEvent.Display(message);
				}
			}
		}
	}
}