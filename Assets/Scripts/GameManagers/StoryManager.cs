using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoryManager : GameEventListener
{
    public MessageEvent messagePanelEvent;
    public IntReference turnCount;

    private void Start()
    {
        messagePanelEvent.Display(new Message("Aaron", "Is the best!"));
    }

    public void DisplayStory()
    {
        switch (turnCount)
        {
            case 2:
                messagePanelEvent.Display(new Message("Bob", "does not sob"));
                break;
        }
    }
}
