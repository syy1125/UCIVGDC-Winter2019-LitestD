using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Messages/MessageElement")]
public class MessageElement : ScriptableObject
{
    public enum Condition
    {
        Equals,
        NotEquals,
        LessThan,
        LessThanOrEqual,
        GreaterThan,
        GreaterThanOrEqual
    }

    [Header("Condition")]
    public IntReference intReference;
    public Condition condition;
    public int value;

    [Header("Messages")]
    public List<Message> messages = new List<Message>();

    public bool IsConditionMet()
    {
        switch (condition)
        {
            case Condition.Equals:
                return intReference == value;
            case Condition.NotEquals:
                return intReference != value;
            case Condition.LessThan:
                return intReference < value;
            case Condition.LessThanOrEqual:
                return intReference <= value;
            case Condition.GreaterThan:
                return intReference > value;
            case Condition.GreaterThanOrEqual:
                return intReference >= value;
        }

        return false;
    }
}
