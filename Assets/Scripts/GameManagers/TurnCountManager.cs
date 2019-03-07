using UnityEngine;

public class TurnCountManager : MonoBehaviour
{
    public GameEvent inrementTurnCountEvent;
	public IntReference turnCountRef;

	private void Awake()
	{
        inrementTurnCountEvent.AddListener(IncrementTurnCount);
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