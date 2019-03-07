using System;
using UnityEngine;

[CreateAssetMenu]
public class GameEvent : ScriptableObject
{
	public bool Debounce;
	
    private Action FirstRound;
    private Action SecondRound;

	public void Raise()
	{
        FirstRound?.Invoke();
        SecondRound?.Invoke();
	}

	public void AddListener(Action listener)
	{
        FirstRound += listener;
	}

    public void AddListener(Action listener, bool secondRound)
    {
        if (secondRound)
            SecondRound += listener;
        else
            FirstRound += listener;
    }

	public void RemoveListener(Action listener)
	{
        FirstRound -= listener;
	}

    public void RemoveListener(Action listener, bool secondRound)
    {
        if (secondRound)
            SecondRound -= listener;
        else
            FirstRound += listener;
    }
}