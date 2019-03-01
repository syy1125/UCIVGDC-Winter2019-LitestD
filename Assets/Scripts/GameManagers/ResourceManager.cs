using System.Collections.Generic;
using System.Linq;
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

	[Header("Balancing")]
	public int PowerPerTechnician = 3;
	public int FoodPerFarmer = 3;


	[Header("Overview Rendering")]
	public GameEvent UpdateUIEvent;
	public TextMeshProUGUI HousingDisplay;
	public TextMeshProUGUI FreeWorkforceDisplay;
	public TextMeshProUGUI GeneratorDisplay;
	public TextMeshProUGUI FarmDisplay;

	[Header("Buttons")]
	public Button AssignGeneratorButton;
	public Button UnassignGeneratorButton;
	public Button AssignFarmButton;
	public Button UnassignFarmButton;

	public Button EndTurnButton;

	public int IdlePopulation => Population - GeneratorWorkerCount - FarmWorkerCount;
	public int PowerProduced => GeneratorWorkerCount * PowerPerTechnician;
	public int FoodProduced => FarmWorkerCount * FoodPerFarmer;
	public int FoodConsumed => Population;

	public void ExecuteFinalActions()
	{
		Population.value = Mathf.Clamp(Population + 1, 0, HousingCapacity);
		UpdateUIEvent.Raise();
	}

	public void AssignGenerator()
	{
		GeneratorWorkerCount.value += 1;
		UpdateUIEvent.Raise();
	}

	public void UnassignGenerator()
	{
		GeneratorWorkerCount.value -= 1;
		UpdateUIEvent.Raise();
	}

	public void AssignFarm()
	{
		FarmWorkerCount.value += 1;
		UpdateUIEvent.Raise();
	}

	public void UnassignFarm()
	{
		FarmWorkerCount.value -= 1;
		UpdateUIEvent.Raise();
	}

	public void Display()
	{
		HousingDisplay.text = $"Housing {Population} / {HousingCapacity} ({HousingCapacity - Population} free)";
		FreeWorkforceDisplay.text = $"Idle: {IdlePopulation}";
		GeneratorDisplay.text = $"Generator: {GeneratorWorkerCount} / {GeneratorCapacity}";
		FarmDisplay.text = $"Farm: {FarmWorkerCount} / {FarmCapacity.value}";

		AssignGeneratorButton.interactable = GeneratorWorkerCount < GeneratorCapacity && IdlePopulation > 0;
		UnassignGeneratorButton.interactable = GeneratorWorkerCount > 0;
		AssignFarmButton.interactable = FarmWorkerCount < FarmCapacity && IdlePopulation > 0;
		UnassignFarmButton.interactable = FarmWorkerCount > 0;
	}
	
	private void OnDestroy()
	{
		Population.value = 0;
		GeneratorWorkerCount.value = 0;
		FarmWorkerCount.value = 0;
	}
}