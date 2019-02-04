using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerConsumer : MonoBehaviour
{
	public int PowerDraw;

	private void OnEnable()
	{
		GameManager.Instance.GetComponent<ResourceManager>().PowerConsumers.Add(this);
	}

	private void OnDisable()
	{
		GameManager.Instance.GetComponent<ResourceManager>().PowerConsumers.Remove(this);
	}
}