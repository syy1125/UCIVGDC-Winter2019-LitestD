﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowStoryElements : MonoBehaviour
{
    public MessageInterface messageInterface;
    public GameEvent showStoryEvent;
    public GameEvent showTutorialEvent;
    public List<StoryElement> messageElements = new List<StoryElement>();

    private void Awake()
    {
        showStoryEvent.AddListener(Show, true);
        messageInterface.PanelHidden += ShowTutorial;
        // Make sure to show story even if there aren't any story elements

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
        bool messagesShown = false;
        foreach (StoryElement messageElement in messageElements)
        {
            if (messageElement.condition.IsMet())
            {
                foreach (Message message in messageElement.messages)
                {
                    messageInterface.Enqueue(message);
                    messagesShown = true;
                }
            }
        }

        if (!messagesShown)
        {
            ShowTutorial();
        }
    }

    public void ShowTutorial()
    {
        showTutorialEvent.Raise();
    }
}
