using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Message
{
    public string title;
    public string text;
}

[System.Serializable]
public class MessageSequence
{
    public int turn;
    public List<Message> messages;
}

[System.Serializable]
public class Story
{
    public List<MessageSequence> allMessageSequences;

    public static Story CreateFromJson(string jsonString)
    {
        return JsonUtility.FromJson<Story>(jsonString);
    }
}
