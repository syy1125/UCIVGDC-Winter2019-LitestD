using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
	[Header("Capacity References")]
	public IntReference HousingCapacity;
	public IntReference GeneratorCapacity;
	public IntReference FarmCapacity;

	[Header("Balancing")]
	public int PowerPerTechnician = 3;
	public int FoodPerFarmer = 3;

	[HideInInspector]
	public int Population;
	[HideInInspector]
	public int GeneratorWorkerCount;
	[HideInInspector]
	public int FarmWorkerCount;

}
