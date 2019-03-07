using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Conditional Elements/Story Element")]
public class StoryElement : ScriptableObject
{
    public Condition condition;

    [Header("Messages")]
    public List<Message> messages = new List<Message>();
}
