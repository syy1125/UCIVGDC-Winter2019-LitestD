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
	public string title;
	[TextArea(3, 6)]
	public string text;
    public Position position;
    public bool disableUI = true;

	public Message(string title, string text)
	{
		this.title = title;
		this.text = text;
	}
}