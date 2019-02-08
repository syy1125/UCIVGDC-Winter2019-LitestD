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


	[Header("Rendering")]
	public GameEvent UpdateUIEvent;
	public TextMeshProUGUI HousingDisplay;
	public TextMeshProUGUI FreeWorkforceDisplay;
	public TextMeshProUGUI GeneratorDisplay;
	public TextMeshProUGUI FarmDisplay;
	public Button AssignGeneratorButton;
	public Button UnassignGeneratorButton;
	public Button AssignFarmButton;
	public Button UnassignFarmButton;

	private void Awake()
	{
		PowerConsumers = new HashSet<PowerConsumer>();
	}

	public void DebugAddPop()
	{
		Population.value = Mathf.Clamp(Population + 1, 0, HousingCapacity);
		UpdateUIEvent.Raise();
	}

	public void ChangeGeneratorWorkforce(int delta)
	{
		GeneratorWorkerCount.value = Mathf.Clamp(GeneratorWorkerCount + delta, 0, GeneratorCapacity);
		UpdateUIEvent.Raise();
	}

	public void ChangeFarmWorkforce(int delta)
	{
		FarmWorkerCount.value = Mathf.Clamp(FarmWorkerCount + delta, 0, FarmCapacity);
		UpdateUIEvent.Raise();
	}

	public void Display()
	{
		int unassignedPopulation = Population - GeneratorWorkerCount - FarmWorkerCount;

		HousingDisplay.text =
			$"Housing {Population} / {HousingCapacity} ({HousingCapacity - Population} free)";
		FreeWorkforceDisplay.text =
			$"Unassigned: {unassignedPopulation}";
		GeneratorDisplay.text =
			$"Generator: {GeneratorWorkerCount} / {GeneratorCapacity} (+{PowerPerTechnician * GeneratorWorkerCount} power)";
		FarmDisplay.text =
			$"Farm: {FarmWorkerCount} / {FarmCapacity.value} (+{FoodPerFarmer * FarmWorkerCount} food)";

		AssignGeneratorButton.interactable = GeneratorWorkerCount < GeneratorCapacity && unassignedPopulation > 0;
		UnassignGeneratorButton.interactable = GeneratorWorkerCount > 0;
		AssignFarmButton.interactable = FarmWorkerCount < FarmCapacity && unassignedPopulation > 0;
		UnassignFarmButton.interactable = FarmWorkerCount > 0;
	}

	private void OnDestroy()
	{
		Population.value = 0;
		GeneratorWorkerCount.value = 0;
		FarmWorkerCount.value = 0;
	}
}