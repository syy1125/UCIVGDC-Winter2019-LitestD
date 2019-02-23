using System.Collections;
using System.Collections.Generic;

public class EndTurnListener : GameEventListener
{
	public GameEvent updateUIEvent;
	public List<GameEvent> phases;

	public static Queue<IEnumerator> actions = new Queue<IEnumerator>();

	public void ExecutePhases()
	{
		StartCoroutine(ExecutePhaseCoroutines());
	}

	private IEnumerator ExecutePhaseCoroutines()
	{
		foreach (GameEvent phase in phases)
		{
			phase.Raise();

			while (actions.Count > 0)
			{
				yield return StartCoroutine(actions.Dequeue());
			}
		}

		updateUIEvent.Raise();
	}
}