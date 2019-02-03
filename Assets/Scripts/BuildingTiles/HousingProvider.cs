using UnityEngine;

public class HousingProvider : MonoBehaviour
{
	public IntReference HousingCapacity;
	public int Capacity;

	private void OnEnable()
	{
		HousingCapacity.value += Capacity;
	}

	private void OnDisable()
	{
		HousingCapacity.value -= Capacity;
	}
}
