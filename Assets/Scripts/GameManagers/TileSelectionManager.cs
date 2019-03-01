using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using Debug = System.Diagnostics.Debug;

public class TileSelectionManager : MonoBehaviour
{
	public Tilemap GroundMap;
	public Tilemap BuildingMap;
	public Vector3Int? Selection { get; private set; }
	[FormerlySerializedAs("ClickAdaptor")]
	public PointerEventAdaptor GroundClickAdaptor;

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

	private void OnEnable()
	{
		GroundClickAdaptor.enabled = true;
	}

	public void OnGroundClick(PointerEventData eventData)
	{
        if (eventData.button != PointerEventData.InputButton.Left) return; // Only want left click to select tiles

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
		Debug.Assert(Selection != null, nameof(Selection) + " != null");

		GameObject buildingLogic = BuildingMap.GetInstantiatedObject(Selection.Value);
		buildingLogic.SetActive(!buildingLogic.activeSelf);

		BuildingMap.SetTileFlags(Selection.Value, TileFlags.None);
		BuildingMap.SetColor(Selection.Value, buildingLogic.activeSelf ? Color.white : DisabledColor);

		UpdateUIEvent.Raise();
	}

	private void OnDisable()
	{
        if (GroundClickAdaptor != null)
        {
            GroundClickAdaptor.enabled = false;
        }
	}
}