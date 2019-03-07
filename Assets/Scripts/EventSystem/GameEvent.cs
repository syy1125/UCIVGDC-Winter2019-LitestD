using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Events/GameEvent")]
public class GameEvent : ScriptableObject
{
    private Action OnEventRaised;

	public void Raise()
	{
        OnEventRaised?.Invoke();
	}

	public void AddListener(Action listener)
	{
        OnEventRaised += listener;
	}

	public void RemoveListener(Action listener)
	{
        OnEventRaised -= listener;
	}
}