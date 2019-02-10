using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class StoryManager : GameEventListener
{
    public DisplayPopup displayPopup;
    public IntReference turnCount;

    private string storyFileName = "story.json";
    private Story story;

    void Awake()
    {
        LoadStory();
    }

    private void LoadStory()
    {
        string storyPath = Path.Combine(Application.streamingAssetsPath, storyFileName);
        if (File.Exists(storyPath))
        {
            string storyAsJson = File.ReadAllText(storyPath);
            story = Story.CreateFromJson(storyAsJson);
        }
    }

    private void Start()
    {
        // It would probably be better if an UpdateUIEvent was just called at the beginning of the game.
        // There could potentially be a problem where the first turn is enqueued twice.
        MessageSequence firstMessageSequence = FindMessagesToShow(1);
        EnqueueMessages(firstMessageSequence);
    }

    public void DisplayStory()
    {
        EnqueueMessages(FindMessagesToShow(turnCount));
    }

    private MessageSequence FindMessagesToShow(int turn)
    {
        foreach (MessageSequence messageSequence in story.allMessageSequences)
        {
            if (messageSequence.turn == turn)
            {
                return messageSequence;
            }
        }
        return null;
    }

    private void EnqueueMessages(MessageSequence messageSequence)
    {
        if (messageSequence != null)
        {
            foreach(Message message in messageSequence.messages)
            {
                displayPopup.EnqueueMessage(message);
            }
        }
    }
}
