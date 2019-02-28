using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndTurnManager : MonoBehaviour
{
	public List<GameEvent> phases;
	
	[Header("Rendering")]
	public GameEvent updateUIEvent;
	public Button endTurnButton;

	public static Queue<IEnumerator> actions = new Queue<IEnumerator>();

	private void OnEnable()
	{
		endTurnButton.interactable = true;
	}

	public void ExecutePhases()
	{
		GameManager.Instance.StartCoroutine(ExecutePhaseCoroutines());
	}

	private IEnumerator ExecutePhaseCoroutines()
	{
		GameManager.Instance.DisableOtherManagers(null);
		
		foreach (GameEvent phase in phases)
		{
			phase.Raise();

			while (actions.Count > 0)
			{
				yield return StartCoroutine(actions.Dequeue());
			}
		}
		
		GameManager.Instance.EnterSelectionMode();

		updateUIEvent.Raise();
	}

	private void OnDisable()
	{
		endTurnButton.interactable = false;
	}
}