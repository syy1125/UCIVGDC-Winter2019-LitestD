[System.Serializable]
public class Message
{
    public string title;
    public string text;

    public Message(string title, string text)
    {
        this.title = title;
        this.text = text;
    }
}
