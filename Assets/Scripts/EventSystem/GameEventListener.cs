using UnityEngine;
using UnityEngine.Events;

public class GameEventListener : MonoBehaviour
{
	public GameEvent gameEvent;
	public UnityEvent response;

	private bool shouldInvoke;
	
	private void OnEnable()
	{
		gameEvent.AddListener(OnEventRaised);
	}

	private void OnDisable()
	{
		gameEvent.RemoveListener(OnEventRaised);
	}

	private void OnEventRaised()
	{
		if (gameEvent.Debounce)
		{
			shouldInvoke = true;
		}
		else
		{
			response.Invoke();
		}
	}
	private void LateUpdate()
	{
		if (!shouldInvoke) return;

		shouldInvoke = false;
		response.Invoke();
	}
}