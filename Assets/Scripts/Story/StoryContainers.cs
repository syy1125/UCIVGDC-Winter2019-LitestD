using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Line
{
    public string title;
    public string text;
}

[System.Serializable]
public class Popup
{
    public int turn;
    public List<Line> lines;
}

[System.Serializable]
public class Story
{
    public List<Popup> popups;

    public static Story CreateFromJson(string jsonString)
    {
        return JsonUtility.FromJson<Story>(jsonString);
    }
}
