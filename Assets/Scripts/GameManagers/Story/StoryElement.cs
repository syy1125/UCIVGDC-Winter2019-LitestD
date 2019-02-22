using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class StoryElement : ScriptableObject
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
	
	[Header("Limits")]
	public int maxActivations = 1; // Set to negative to always activate
	[HideInInspector]
	public int currentActivations;

    [Header("Messages")]
	public List<Message> messages = new List<Message>();

	public bool IsConditionMet()
	{
		if (maxActivations >= 0 && currentActivations >= maxActivations) return false;
		
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