using System.Collections.Generic;

public class EndTurnListener : GameEventListener
{
	public GameEvent updateUIEvent;
	public List<GameEvent> phases;

	public void ExecutePhases()
	{
		foreach (GameEvent phase in phases)
		{
			phase.Raise();
		}

		updateUIEvent.Raise();
	}
}