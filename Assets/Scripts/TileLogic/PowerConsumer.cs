using UnityEngine;

public class PowerConsumer : MonoBehaviour
{
	public int PowerDraw;

	private void OnEnable()
	{
		GameManager.Instance.ResourceManager.PowerConsumers.Add(this);
	}

	private void OnDisable()
	{
		GameManager.Instance.ResourceManager.PowerConsumers.Remove(this);
	}
}