using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerConsumer : MonoBehaviour
{
	public IntReference PowerConsumptionRef;
	public int PowerDraw;

	private void OnEnable()
	{
		PowerConsumptionRef.value += PowerDraw;
	}

	private void OnDisable()
	{
		PowerConsumptionRef.value -= PowerDraw;
	}
}