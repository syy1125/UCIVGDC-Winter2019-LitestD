using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
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
	public Button OpenOverlayButton;

	[Header("Popup Rendering")]
	public GameObject OverlayRoot;
	public Animator PopupAnimator;
	public string HideAnimationName;
	public TextMeshProUGUI PopulationDetail;
	public TextMeshProUGUI GeneratorDetail;
	public TextMeshProUGUI FarmDetail;
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

	public Button EndTurnButton;


	public int IdlePopulation => Population - GeneratorWorkerCount - FarmWorkerCount;
	public int PowerProduced => GeneratorWorkerCount * PowerPerTechnician;
	public int FoodProduced => FarmWorkerCount * FoodPerFarmer;
	public int FoodConsumed => Population;

	private static readonly int VisibleKey = Animator.StringToHash("Visible");

	private void OnEnable()
	{
		OpenOverlayButton.interactable = true;
	}

	public void InitializeState()
	{
		Population.value = 1;
	}

	public void ExecuteFinalActions()
	{
		ResolvePopulationChange();
		UpdateUIEvent.Raise();
	}

	private void ResolvePopulationChange()
	{
		if (Population < HousingCapacity)
		{
			Population.value += 1;
			HousePopulation(GeneratePortrait());
		}
		else if (Population > HousingCapacity)
		{
			Population.value = HousingCapacity;
			_extraPopulation.Clear();
			PreventPopulationOverdraft();
		}
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

	public void HousePopulation(Sprite portrait)
	{
		PopulationProvider targetHousing = null;

		foreach (Vector3Int position in BuildingMap.cellBounds.allPositionsWithin)
		{
			GameObject tileLogic = BuildingMap.GetInstantiatedObject(position);
			if (tileLogic == null || !tileLogic.activeSelf) continue;

			var populationProvider = tileLogic.GetComponent<PopulationProvider>();
			if (
				populationProvider != null
				&& populationProvider.ResidentCount < populationProvider.Capacity
				&& (ReferenceEquals(targetHousing, null)
				    || populationProvider.ConstructionOrder < targetHousing.ConstructionOrder)
			)
			{
				targetHousing = populationProvider;
			}
		}

		if (ReferenceEquals(targetHousing, null))
		{
			_extraPopulation.Push(portrait);
		}
		else
		{
			targetHousing.Portraits.Push(portrait);
		}
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
		HousingDisplay.text = $"Housing {Population} / {HousingCapacity} ({HousingCapacity - Population} available)";
		HousingDisplay.color = Population > HousingCapacity ? Color.red : Color.white;
		FreeWorkforceDisplay.text = $"Idle: {IdlePopulation}";
		GeneratorDisplay.text = $"Generator: {GeneratorWorkerCount} / {GeneratorCapacity}";
		FarmDisplay.text = $"Farm: {FarmWorkerCount} / {FarmCapacity.value}";

		PopulationDetail.text = $"Population {Population} / {HousingCapacity} Housing\n"
		                        + (Population > HousingCapacity
			                        ? $"<color=red>({HousingCapacity - Population} space available)</color>"
			                        : $"({HousingCapacity - Population} space available)");
		GeneratorDetail.text = $"Gen. worker {GeneratorWorkerCount} / {GeneratorCapacity} Capacity\n"
		                       + $"(Effect: +{PowerProduced} build speed)";
		FarmDetail.text = $"Farm worker {FarmWorkerCount} / {FarmCapacity} Capacity\n"
		                  + $"(Effect: +{FoodProduced} food produced)\n"
		                  + $"Population consumes {FoodConsumed} food\n"
		                  + (FoodConsumed > FoodProduced
			                  ? $"<color=red>(Net: {FoodProduced - FoodConsumed} / turn)</color>"
			                  : $"(Net: +{FoodProduced - FoodConsumed} / turn)");

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