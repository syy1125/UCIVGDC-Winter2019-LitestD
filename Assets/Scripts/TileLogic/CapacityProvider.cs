using UnityEngine;

public class CapacityProvider : MonoBehaviour
{
	public IntReference CapacityRef;
	public int CapacityDelta;

	private void OnEnable()
	{
		CapacityRef.value += CapacityDelta;
	}

	private void OnDisable()
	{
		CapacityRef.value -= CapacityDelta;
	}
}