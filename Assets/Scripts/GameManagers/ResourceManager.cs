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
	
	public void Display()
	{
		HousingDisplay.text =
			$"Housing {Population} / {HousingCapacity} ({HousingCapacity - Population} free)";
		FreeWorkforceDisplay.text =
			$"Unassigned: {Population - GeneratorWorkerCount - FarmWorkerCount}";
		GeneratorDisplay.text =
			$"Generator: {GeneratorWorkerCount} / {GeneratorCapacity} (+{PowerPerTechnician * GeneratorWorkerCount} power)";
		FarmDisplay.text =
			$"Farm: {FarmWorkerCount} / {FarmCapacity.value} (+{FoodPerFarmer * FarmWorkerCount} food)";

		AssignGeneratorButton.interactable = GeneratorWorkerCount < GeneratorCapacity;
		UnassignGeneratorButton.interactable = GeneratorWorkerCount > 0;
		AssignFarmButton.interactable = FarmWorkerCount < FarmCapacity;
		UnassignFarmButton.interactable = FarmWorkerCount > 0;
	}
}