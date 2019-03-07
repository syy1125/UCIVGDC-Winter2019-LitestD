using UnityEngine;
using UnityEngine.Events;

public class GameEventListener : MonoBehaviour
{
	public GameEvent gameEvent;
	public UnityEvent response;

	private void OnEnable()
	{
		gameEvent.AddListener(OnEventRaised);
	}

	private void OnDisable()
	{
		gameEvent.RemoveListener(OnEventRaised);
	}

	public void OnEventRaised()
	{
		response.Invoke();
	}
}