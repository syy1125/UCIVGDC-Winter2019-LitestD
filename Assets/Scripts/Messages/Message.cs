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
	[TextArea]
	public string text;
    public Position position;

	public Message(string title, string text)
	{
		this.title = title;
		this.text = text;
	}
}