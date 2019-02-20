using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class TileSelectionManager : MonoBehaviour
{
	public Tilemap GroundMap;
	public Tilemap BuildingMap;
	public Vector3Int? Selection { get; private set; }

	[Header("Rendering")]
	public GameEvent UpdateUIEvent;
	public TextMeshProUGUI BuildingNameText;
	public TextMeshProUGUI BuildingFlavourText;
	public TextMeshProUGUI ToggleButtonText;
	public string GroundName;
	[TextArea]
	public string GroundFlavourText;

	[Space]
	public GameObject AssignJobPanel;
	public TextMeshProUGUI JobNameText;
	public GameObject PortraitGrid;
	public GameObject PortraitPrefab;
	public Sprite[] NormalPortraits;
	public Button AssignButton;
	public Button UnassignButton;

	private readonly Stack<Sprite> _workerPortraits = new Stack<Sprite>();

	private static Plane _zPlane = new Plane(Vector3.forward, 0);
	private Camera _mainCamera;

	private void Start()
	{
		_mainCamera = Camera.main;
	}

	private void Update()
	{
		if (!Input.GetMouseButtonDown(0)) return;

		Ray mouseRay = _mainCamera.ScreenPointToRay(Input.mousePosition);
		if (!_zPlane.Raycast(mouseRay, out float distance))
		{
			Debug.LogError("Failed to raycast onto Z plane.");
			return;
		}

		Vector3Int tilePosition = GroundMap.WorldToCell(mouseRay.GetPoint(distance));
		if (!GroundMap.HasTile(tilePosition)) return;

		SetSelection(tilePosition);
	}

	public void SetSelection(Vector3Int? target)
	{
		if (Selection != null)
		{
			GroundMap.SetColor(Selection.Value, Color.white);
			GroundMap.SetTileFlags(Selection.Value, TileFlags.LockColor);
			BuildingMap.SetColor(Selection.Value, Color.white);
			BuildingMap.SetTileFlags(Selection.Value, TileFlags.LockColor);
		}

		Selection = target;

		if (Selection != null)
		{
			GroundMap.SetTileFlags(Selection.Value, TileFlags.None);
			GroundMap.SetColor(Selection.Value, Color.cyan);
			BuildingMap.SetTileFlags(Selection.Value, TileFlags.None);
			BuildingMap.SetColor(Selection.Value, Color.cyan);
		}

		GameManager.Instance.DisableOtherManagers(this);
		UpdateUIEvent.Raise();
	}

	public void Display()
	{
		if (Selection == null)
		{
			GetComponent<Image>().enabled = false;
			foreach (Transform child in transform)
			{
				child.gameObject.SetActive(false);
			}
		}
		else
		{
			Vector3Int selectedTile = Selection.Value;

			GetComponent<Image>().enabled = true;
			foreach (Transform child in transform)
			{
				child.gameObject.SetActive(true);
			}

			ToggleButtonText.text = BuildingMap.GetInstantiatedObject(selectedTile).activeSelf
				? "Turn Off"
				: "Turn On";

			DisplayBuildingInfo(selectedTile);
		}
	}

	private void DisplayBuildingInfo(Vector3Int selectedTile)
	{
		if (BuildingMap.HasTile(selectedTile))
		{
			var description =
				BuildingMap.GetInstantiatedObject(selectedTile).GetComponent<BuildingDescription>();

			BuildingNameText.text = description.Name;
			BuildingFlavourText.text = description.FlavourText;

			var provider = BuildingMap.GetInstantiatedObject(selectedTile).GetComponent<WorkerProvider>();

			if (provider == null)
			{
				AssignJobPanel.SetActive(false);
			}
			else
			{
				AssignJobPanel.SetActive(true);
				JobNameText.text = provider.JobName;

				AssignButton.interactable = provider.AssignedCount < provider.Capacity
				                            && GameManager.Instance.ResourceManager.IdlePopulation > 0;
				UnassignButton.interactable = provider.AssignedCount > 0;

				foreach (Transform child in PortraitGrid.transform)
				{
					Destroy(child.gameObject);
				}

				foreach (Sprite portraitSprite in provider.WorkerPortraits)
				{
					GameObject portrait = Instantiate(PortraitPrefab, PortraitGrid.transform);
					portrait.transform.GetChild(0).GetComponent<Image>().sprite = portraitSprite;
				}

				for (int index = provider.AssignedCount; index < provider.Capacity; index++)
				{
					Instantiate(PortraitPrefab, PortraitGrid.transform);
				}
			}
		}
		else
		{
			BuildingNameText.text = GroundName;
			BuildingFlavourText.text = GroundFlavourText;

			AssignJobPanel.SetActive(false);
		}
	}

	public void AssignWorker()
	{
		Debug.Assert(Selection != null, nameof(Selection) + " != null");
		var provider = BuildingMap.GetInstantiatedObject(Selection.Value).GetComponent<WorkerProvider>();

		if (_workerPortraits.Count <= 0)
		{
			_workerPortraits.Push(NormalPortraits[Random.Range(0, NormalPortraits.Length)]);
		}

		provider.AssignWorker(_workerPortraits.Pop());

		UpdateUIEvent.Raise();
	}

	public void UnassignWorker()
	{
		System.Diagnostics.Debug.Assert(Selection != null, nameof(Selection) + " != null");
		var provider = BuildingMap.GetInstantiatedObject(Selection.Value).GetComponent<WorkerProvider>();

		_workerPortraits.Push(provider.UnassignWorker());

		UpdateUIEvent.Raise();
	}

	public void ToggleBuilding()
	{
		System.Diagnostics.Debug.Assert(Selection != null, nameof(Selection) + " != null");
		GameObject buildingLogic = BuildingMap.GetInstantiatedObject(Selection.Value);
		buildingLogic.SetActive(!buildingLogic.activeSelf);
		UpdateUIEvent.Raise();
	}
}