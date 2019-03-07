using UnityEngine;

public enum Position
{
    Default,
    Center,
    TopLeft,
    TopRight,
    BottomRight,
    BottomLeft
}

[System.Serializable]
public class Message
{
    [Header("Basic Options")]
    public string title = "";
    [TextArea(3, 6)] public string text = "";
    public int maxTimesShown = 1;

    [Header("Special Options")]
    public Position position = Position.Default;
    public bool showContinueButton = true;
    public bool disableUI = true;

    private int timesShown = 0;

    public Message(string title, string text, int maxTimesShown, Position position, bool showContinueButton, bool disableUI)
    {
        this.title = title;
        this.text = text;
        this.maxTimesShown = maxTimesShown;
        this.position = position;
        this.showContinueButton = showContinueButton;
        this.disableUI = disableUI;
    }

    public bool CanShow()
    {
        return timesShown < maxTimesShown || maxTimesShown < 0;
    }

    public void ResetTimesShown()
    {
        timesShown = 0;
    }

    public void Shown()
    {
        timesShown++;
    }
}
