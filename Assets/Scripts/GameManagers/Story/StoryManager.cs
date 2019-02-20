using System.Collections.Generic;

public class StoryManager : GameEventListener
{
	public MessageEvent messagePanelEvent;
	public IntReference turnCount;
	public List<StoryElement> storyElements = new List<StoryElement>();

	public void DisplayStory()
	{
		foreach (StoryElement storyElement in storyElements)
		{
			if (storyElement.IsConditionMet())
			{
				messagePanelEvent.Display(storyElement.message);
			}
		}
	}
}