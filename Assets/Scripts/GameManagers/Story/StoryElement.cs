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

	public IntReference intReference;
	public Condition condition;
	public int value;
	public Message message;

	// TODO: Add an option to limit the number of times a message can be shown.
	//      This is probably most important for the LessThans and GreaterThans.
	//      Could have a checkbox for ShowOnlyOnce.

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