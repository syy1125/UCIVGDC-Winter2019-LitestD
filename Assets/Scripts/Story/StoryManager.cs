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
        Popup firstPopup = FindLinesToPopup(1);
        EnqueueLines(firstPopup);
    }

    public void DisplayStory()
    {
        EnqueueLines(FindLinesToPopup(turnCount));
    }

    private Popup FindLinesToPopup(int turn)
    {
        foreach (Popup popup in story.popups)
        {
            if (popup.turn == turn)
            {
                return popup;
            }
        }
        return null;
    }

    private void EnqueueLines(Popup popup)
    {
        if (popup != null)
        {
            foreach(Line line in popup.lines)
            {
                displayPopup.EnqueueMessage(line.text);
            }
        }
    }
}
