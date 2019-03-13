using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using Random = UnityEngine.Random;

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
	public int FoodRequiredPerPopulation = 5;

	private int _growthProgress;

	[Header("Overview Rendering")]
	public GameEvent UpdateUIEvent;
	public TextMeshProUGUI HousingDisplay;
	public TextMeshProUGUI FreeWorkforceDisplay;
	[FormerlySerializedAs("GeneratorDisplay")]
	public TextMeshProUGUI PowerDisplay;
	[FormerlySerializedAs("FarmDisplay")]
	public TextMeshProUGUI FoodDisplay;
	public Button OpenOverlayButton;

	[Header("Popup Rendering")]
	public GameObject OverlayRoot;
	public Animator PopupAnimator;
	public string HideAnimationName;
	public TextMeshProUGUI PopulationDetail;
	public TextMeshProUGUI GrowthDetail;
	public TextMeshProUGUI GeneratorDetail;
	public TextMeshProUGUI FarmDetail;
	[FormerlySerializedAs("UnassignedDetail")]
	public TextMeshProUGUI IdleDetail;
	public Button AssignGeneratorButton;
	public Button UnassignGeneratorButton;
	public Button AssignFarmButton;
	public Button UnassignFarmButton;

	private bool _overlayVisible;
	private Coroutine _hideOverlayCoroutine;

	[Header("Housing")]
	public Tilemap BuildingMap;
	public Sprite[] NormalPortraits;
	private Stack<Sprite> _extraPopulation = new Stack<Sprite>();

	public int IdlePopulation => Population - GeneratorWorkerCount - FarmWorkerCount;
	public int PowerProduced => GeneratorWorkerCount * PowerPerTechnician;
	public int FoodProduced => FarmWorkerCount * FoodPerFarmer;
	public int FoodConsumed => Population;

	private static readonly int VisibleKey = Animator.StringToHash("visible");

	private void OnEnable()
	{
		OpenOverlayButton.interactable = true;
	}

	public void InitializeState()
	{
		HousePopulation(GeneratePortrait());
		HousePopulation(GeneratePortrait());
		HousePopulation(GeneratePortrait());

		AssignFarm();

		AssignGenerator();
		AssignGenerator();
	}

	public void ExecuteFinalActions()
	{
		EndTurnManager.actions.Enqueue(ResolvePopulationChange());
		UpdateUIEvent.Raise();
	}

	private IEnumerator ResolvePopulationChange()
	{
		if (Population >= HousingCapacity)
		{
			CapPopulation();
			yield break;
		}

		_growthProgress += FoodProduced - FoodConsumed;

		var growthMap = new Dictionary<Vector3Int, int>();

		while (_growthProgress >= FoodRequiredPerPopulation)
		{
			_growthProgress -= FoodRequiredPerPopulation;
			GameObject target = HousePopulation(GeneratePortrait());

			if (target == null) continue;

			Vector3Int buildingPosition =
				BuildingMap.WorldToCell(target.transform.position);
			growthMap[buildingPosition] =
				growthMap.TryGetValue(buildingPosition, out int original) ? original + 1 : 1;
		}

		foreach (KeyValuePair<Vector3Int, int> entry in growthMap)
		{
			GameManager.Instance.FlytextManager.SpawnFlytextWorldPosition(
				BuildingMap.GetCellCenterWorld(entry.Key),
				$"+{entry.Value} population"
			);
		}

		while (_growthProgress <= -FoodRequiredPerPopulation)
		{
			_growthProgress += FoodRequiredPerPopulation;
			RemovePopulation();
		}

		if (Population >= HousingCapacity)
		{
			CapPopulation();
		}
	}

	private void CapPopulation()
	{
		_growthProgress = 0;
		Population.value = HousingCapacity;
		_extraPopulation.Clear();
		PreventPopulationOverdraft();
	}

	public void PreventPopulationOverdraft()
	{
		while (IdlePopulation < 0)
		{
			if (FarmWorkerCount <= 0)
			{
				GeneratorWorkerCount.value += IdlePopulation;
			}
			else if (GeneratorWorkerCount <= 0)
			{
				FarmWorkerCount.value += IdlePopulation;
			}
			else if (Random.value < 0.5f)
			{
				GeneratorWorkerCount.value -= 1;
			}
			else
			{
				FarmWorkerCount.value -= 1;
			}
		}
	}

	public GameObject HousePopulation(Sprite portrait)
	{
		PopulationProvider targetHousing = FindBestHousing(
			candidate => candidate.ResidentCount < candidate.Capacity,
			(candidate, current) => candidate.ConstructionOrder < current.ConstructionOrder
		);

		if (ReferenceEquals(targetHousing, null))
		{
			_extraPopulation.Push(portrait);
			Population.value++;
			return null;
		}

		targetHousing.Portraits.Push(portrait);

		Population.value++;

		return targetHousing.gameObject;
	}

	public void RemovePopulation()
	{
		PopulationProvider targetHousing = FindBestHousing(
			candidate => candidate.ResidentCount > 0,
			(candidate, current) => candidate.ConstructionOrder > current.ConstructionOrder
		);

		if (ReferenceEquals(targetHousing, null))
		{
			Debug.LogWarning("Can't find population to kill! Are you sure there are still people alive?");
			return;
		}

		targetHousing.Portraits.Pop();
		Population.value -= 1;
	}

	private PopulationProvider FindBestHousing(
		Predicate<PopulationProvider> isViable,
		Func<PopulationProvider, PopulationProvider, bool> isBetter
	)
	{
		PopulationProvider current = null;

		foreach (Vector3Int position in BuildingMap.cellBounds.allPositionsWithin)
		{
			GameObject tileLogic = BuildingMap.GetInstantiatedObject(position);
			if (tileLogic == null || !tileLogic.activeSelf) continue;

			var candidate = tileLogic.GetComponent<PopulationProvider>();
			if (
				candidate != null
				&& isViable(candidate)
				&& (ReferenceEquals(current, null) || isBetter(candidate, current))
			)
			{
				current = candidate;
			}
		}

		return current;
	}

	private Sprite GeneratePortrait()
	{
		return NormalPortraits[Random.Range(0, NormalPortraits.Length)];
	}

	public void SetOverlayVisible(bool visible)
	{
		if (visible == _overlayVisible) return;
		_overlayVisible = visible;

		if (visible)
		{
			OverlayRoot.SetActive(true);
			PopupAnimator.SetBool(VisibleKey, true);
			if (_hideOverlayCoroutine != null)
			{
				GameManager.Instance.StopCoroutine(_hideOverlayCoroutine);
			}

			GameManager.Instance.DisableOtherManagers(this);
		}
		else
		{
			PopupAnimator.SetBool(VisibleKey, false);
			_hideOverlayCoroutine = GameManager.Instance.StartCoroutine(DelayedHideOverlay());
		}
	}

	private IEnumerator DelayedHideOverlay()
	{
		Debug.Log(PopupAnimator.GetNextAnimatorStateInfo(0).IsName(HideAnimationName));
		yield return new WaitWhile(
			() => PopupAnimator.GetCurrentAnimatorStateInfo(0).IsName(HideAnimationName)
		);
		OverlayRoot.SetActive(false);
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
		HousingDisplay.text = $"Housing {Population} / {HousingCapacity}";
		HousingDisplay.color = Population > HousingCapacity ? Color.red : Color.white;
		FreeWorkforceDisplay.text = $"Idle: {IdlePopulation}";
		PowerDisplay.text = $"+{PowerProduced} / turn";
		string farmText = $"{FoodProduced - FoodConsumed} / turn";
		if (FoodProduced >= FoodConsumed)
		{
			FoodDisplay.text = "+" + farmText;
			FoodDisplay.color = Color.white;
		}
		else
		{
			FoodDisplay.text = farmText;
			FoodDisplay.color = Color.red;
		}

		PopulationDetail.text = $"Population {Population} / {HousingCapacity} Housing\n"
		                        + (Population > HousingCapacity
			                        ? $"<color=red>({HousingCapacity - Population} space available)</color>"
			                        : $"({HousingCapacity - Population} space available)");
		GeneratorDetail.text = $"Gen. worker {GeneratorWorkerCount} / {GeneratorCapacity} Capacity\n"
		                       + $"(Effect: +{PowerProduced} build speed)";
		if (Population >= HousingCapacity)
		{
			GrowthDetail.text = "Population at capacity";
		}
		else if (FoodProduced < FoodConsumed)
		{
			GrowthDetail.text = "Population: <color=red>Declining</color>\n"
			                    + $"({-_growthProgress} / {FoodRequiredPerPopulation} progress, +{FoodConsumed - FoodProduced} / turn)";
		}
		else if (FoodProduced > FoodConsumed)
		{
			GrowthDetail.text = "Population: <color=green>Growing</color>\n"
			                    + $"({_growthProgress} / {FoodRequiredPerPopulation} progress, +{FoodProduced - FoodConsumed} / turn)";
		}
		else
		{
			GrowthDetail.text = "Population: Stable\n"
			                    + $"(Growth stockpile: {_growthProgress})";
		}

		FarmDetail.text = $"Farm worker {FarmWorkerCount} / {FarmCapacity} Capacity\n"
		                  + $"(Effect: +{FoodProduced} food produced)\n"
		                  + $"Population consumes {FoodConsumed} food\n"
		                  + (FoodConsumed > FoodProduced
			                  ? $"<color=red>(Net: {FoodProduced - FoodConsumed} / turn)</color>"
			                  : $"(Net: +{FoodProduced - FoodConsumed} / turn)");
		IdleDetail.text = $"Idle workers: {IdlePopulation}";

		AssignGeneratorButton.interactable = GeneratorWorkerCount < GeneratorCapacity && IdlePopulation > 0;
		UnassignGeneratorButton.interactable = GeneratorWorkerCount > 0;
		AssignFarmButton.interactable = FarmWorkerCount < FarmCapacity && IdlePopulation > 0;
		UnassignFarmButton.interactable = FarmWorkerCount > 0;
	}

	private void OnDisable()
	{
		OpenOverlayButton.interactable = false;
	}

	private void OnDestroy()
	{
		Population.value = 0;
		GeneratorWorkerCount.value = 0;
		FarmWorkerCount.value = 0;
	}
}