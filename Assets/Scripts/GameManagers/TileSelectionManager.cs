using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class TileSelectionManager : MonoBehaviour
{
	public Tilemap GroundMap;
	public Tilemap BuildingMap;
	public Vector3Int? Selection { get; private set; }

	[Header("Tilemap Effect Colors")]
	public Color SelectedColor;
	public Color DisabledColor;

	[Header("Rendering")]
	public GameEvent UpdateUIEvent;
	public TextMeshProUGUI BuildingNameText;
	public TextMeshProUGUI BuildingFlavourText;
	public TextMeshProUGUI ToggleButtonText;
	public GameObject ToggleButton;

	[Space]
	public string GroundName;
	[TextArea]
	public string GroundFlavourText;

	private readonly Stack<Sprite> _workerPortraits = new Stack<Sprite>();

	private static Plane _zPlane = new Plane(Vector3.forward, 0);
	private Camera _mainCamera;

	private void Start()
	{
		_mainCamera = Camera.main;
	}

	public void OnGroundClick(PointerEventData eventData)
	{
		Vector3Int tilePosition = GroundMap.WorldToCell(eventData.pointerCurrentRaycast.worldPosition);

		if (!GroundMap.HasTile(tilePosition)) return;

		SetSelection(tilePosition);
	}

	public void SetSelection(Vector3Int? target)
	{
		if (Selection != null)
		{
			GroundMap.SetColor(Selection.Value, Color.white);
//			BuildingMap.SetColor(Selection.Value, Color.white);
		}

		Selection = target;

		if (Selection != null)
		{
			GroundMap.SetTileFlags(Selection.Value, TileFlags.None);
			GroundMap.SetColor(Selection.Value, SelectedColor);
//			BuildingMap.SetTileFlags(Selection.Value, TileFlags.None);
//			BuildingMap.SetColor(Selection.Value, SelectedColor);
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

			ToggleButton.SetActive(true);
			ToggleButtonText.text = BuildingMap.GetInstantiatedObject(selectedTile).activeSelf
				? "Turn Off"
				: "Turn On";
		}
		else
		{
			BuildingNameText.text = GroundName;
			BuildingFlavourText.text = GroundFlavourText;
			ToggleButton.SetActive(false);
		}
	}

	public void ToggleBuilding()
	{
		System.Diagnostics.Debug.Assert(Selection != null, nameof(Selection) + " != null");

		GameObject buildingLogic = BuildingMap.GetInstantiatedObject(Selection.Value);
		buildingLogic.SetActive(!buildingLogic.activeSelf);

		BuildingMap.SetTileFlags(Selection.Value, TileFlags.None);
		BuildingMap.SetColor(Selection.Value, buildingLogic.activeSelf ? Color.white : DisabledColor);

		UpdateUIEvent.Raise();
	}
}