using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class TileSelectionManager : MonoBehaviour
{
	public TilemapRegistry Tilemaps;
	public Vector3Int? Selection { get; private set; }
	[FormerlySerializedAs("ClickAdaptor")]
	public PointerEventAdaptor GroundClickAdaptor;

	[Header("Tilemap Effects")]
	public TileBase HighlightTile;
	public Color SelectedColor;
	public Color DisabledColor;

	[Header("Rendering")]
	public GameEvent UpdateUIEvent;
	[FormerlySerializedAs("BuildingNameText")]
	public TextMeshProUGUI NameText;
	[FormerlySerializedAs("BuildingFlavourText")]
	public TextMeshProUGUI FlavourText;
	public TextMeshProUGUI ToggleButtonText;
	public GameObject ToggleButton;
	public GameObject ExplosiveShotButton;

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

		Vector3Int tilePosition = Tilemaps.Ground.WorldToCell(eventData.pointerCurrentRaycast.worldPosition);

		if (!Tilemaps.Ground.HasTile(tilePosition)) return;

		SetSelection(tilePosition);
	}

	public void SetSelection(Vector3Int? target)
	{
		if (Selection != null)
		{
			Tilemaps.Ground.SetColor(Selection.Value, Color.white);
			Tilemaps.Highlights.ClearAllTiles();
		}

		Selection = target;

		if (Selection != null)
		{
			Tilemaps.Ground.SetTileFlags(Selection.Value, TileFlags.None);
			Tilemaps.Ground.SetColor(Selection.Value, SelectedColor);

			DisplayHighlights();
			GameManager.Instance.DisableOtherManagers(this);
			GameManager.Instance.SetStatusText($"Viewing building at {(Vector2Int) Selection.Value}");
		}

		UpdateUIEvent.Raise();
	}

	private void DisplayHighlights()
	{
		System.Diagnostics.Debug.Assert(Selection != null, nameof(Selection) + " != null");
		GameObject tileLogic = Tilemaps.Buildings.GetInstantiatedObject(Selection.Value);

		if (!tileLogic) return;

		var turret = tileLogic.GetComponent<TurretAttack>();
		if (turret == null) return;
		
		foreach (Vector3Int position in turret.GetPositionsInRange())
		{
			Tilemaps.Highlights.SetTile(position, HighlightTile);
		}
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

			if (Tilemaps.Buildings.HasTile(selectedTile))
			{
				DisplayBuildingInfo(selectedTile);
			}
			else if (Tilemaps.Enemies.HasTile(selectedTile))
			{
				DisplayEnemyInfo(selectedTile);
			}
			else
			{
				DisplayGroundInfo();
			}
		}
	}

	private void DisplayBuildingInfo(Vector3Int selectedTile)
	{
		GameObject buildingLogic = Tilemaps.Buildings.GetInstantiatedObject(selectedTile);
		var description = buildingLogic.GetComponent<BuildingDescription>();
		var health = buildingLogic.GetComponent<HealthPool>();

		NameText.text = description.Name;
		FlavourText.text = $"{description.FlavourText}\n"
		                   + $"\n"
		                   + $"Health: {health.Health} / {health.MaxHealth}";

		ToggleButton.SetActive(true);
		ToggleButtonText.text = Tilemaps.Buildings.GetInstantiatedObject(selectedTile).activeSelf
			? "Turn Off"
			: "Turn On";

		var turret = buildingLogic.GetComponent<TurretAttack>();
		if (turret == null)
		{
			ExplosiveShotButton.SetActive(false);
		}
		else
		{
			ExplosiveShotButton.SetActive(true);
		}
	}

	private void DisplayEnemyInfo(Vector3Int selectedTile)
	{
		GameObject enemyLogic = Tilemaps.Enemies.GetInstantiatedObject(selectedTile);
		var attack = enemyLogic.GetComponent<EnemyAttack>();
		var health = enemyLogic.GetComponent<HealthPool>();

		NameText.text = "Enemy";
		FlavourText.text = $"Health: {health.Health} / {health.MaxHealth}\n"
		                   + $"Attack strength: {attack.AttackStrength}";
		ToggleButton.SetActive(false);
	}

	private void DisplayGroundInfo()
	{
		NameText.text = GroundName;
		FlavourText.text = GroundFlavourText;
		ToggleButton.SetActive(false);
	}

	public void PlanExplosiveShot()
	{
		
	}
	
	public void ToggleBuilding()
	{
		Debug.Assert(Selection != null, nameof(Selection) + " != null");

		GameObject buildingLogic = Tilemaps.Buildings.GetInstantiatedObject(Selection.Value);
		buildingLogic.SetActive(!buildingLogic.activeSelf);

		Tilemaps.Buildings.SetTileFlags(Selection.Value, TileFlags.None);
		Tilemaps.Buildings.SetColor(Selection.Value, buildingLogic.activeSelf ? Color.white : DisabledColor);

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