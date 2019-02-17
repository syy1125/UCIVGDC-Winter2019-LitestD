using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResourceManager : MonoBehaviour
{
	[Header("References")]
	public IntReference HousingCapacity;
	public IntReference GeneratorCapacity;
	public IntReference FarmCapacity;
	[Space]
	public IntReference Population;
	public IntReference GeneratorWorkerCount;
	public IntReference FarmWorkerCount;

	[HideInInspector]
	public HashSet<PowerConsumer> PowerConsumers;

	[Header("Balancing")]
	public int PowerPerTechnician = 3;
	public int FoodPerFarmer = 3;


	[Header("Overview Rendering")]
	public GameEvent UpdateUIEvent;
	public TextMeshProUGUI HousingDisplay;
	public TextMeshProUGUI FreeWorkforceDisplay;
	public TextMeshProUGUI GeneratorDisplay;
	public TextMeshProUGUI FarmDisplay;

	public int IdlePopulation
	{
		get { return Population - GeneratorWorkerCount - FarmWorkerCount;  }
	}

	private void Awake()
	{
		PowerConsumers = new HashSet<PowerConsumer>();
	}

	public void ExecuteFinalActions()
	{
		Population.value = Mathf.Clamp(Population + 1, 0, HousingCapacity);
		UpdateUIEvent.Raise();
	}

	public void Display()
	{
		int idlePopulation = IdlePopulation;

		HousingDisplay.text =
			$"Housing {Population} / {HousingCapacity} ({HousingCapacity - Population} free)";
		FreeWorkforceDisplay.text =
			$"Idle: {idlePopulation}";
		GeneratorDisplay.text =
			$"Generator: {GeneratorWorkerCount} / {GeneratorCapacity} (+{PowerPerTechnician * GeneratorWorkerCount} power)";
		FarmDisplay.text =
			$"Farm: {FarmWorkerCount} / {FarmCapacity.value} (+{FoodPerFarmer * FarmWorkerCount} food)";
	}

	private void OnDestroy()
	{
		Population.value = 0;
		GeneratorWorkerCount.value = 0;
		FarmWorkerCount.value = 0;
	}
}