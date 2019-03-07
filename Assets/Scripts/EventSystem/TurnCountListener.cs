using UnityEngine;

public class TurnCountListener : MonoBehaviour
{
    public GameEvent gameEvent;
	public IntReference turnCountRef;

	private void Awake()
	{
        gameEvent.AddListener(IncrementTurnCount);
		LoadTurnCount();
	}

	private void LoadTurnCount()
	{
		turnCountRef.value = 1;
		// Could later add logic to load in the turn count from a save file
	}

	public void IncrementTurnCount()
	{
		turnCountRef.value++;
	}
}