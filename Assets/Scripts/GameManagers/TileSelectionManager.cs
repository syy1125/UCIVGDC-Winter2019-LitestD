using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Experimental.PlayerLoop;
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
	public TextMeshProUGUI ExplosiveShotText;

	public bool IsPlanningExplosiveShot { get; private set; }
	private Vector3Int? _lastHoverTile;

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

		if (IsPlanningExplosiveShot)
		{
			FinishPlanningExplosiveShot(tilePosition);
		}
		else
		{
			SetSelection(tilePosition);
		}
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

			GameManager.Instance.DisableOtherManagers(this);
			GameManager.Instance.SetStatusText($"Viewing building at {(Vector2Int) Selection.Value}");
		}

		UpdateUIEvent.Raise();
	}

	private void Update()
	{
		if (!IsPlanningExplosiveShot) return;

		if (!GameManager.Instance.PlanConstructionManager.GetHoverPosition(
			out Vector3Int tilePosition,
			target => Tilemaps.Ground.HasTile(target) || Tilemaps.OuterEdge.HasTile(target)
		))
		{
			if (_lastHoverTile == null) return;
			Tilemaps.Highlights.ClearAllTiles();
			_lastHoverTile = null;
			return;
		}

		if (tilePosition != _lastHoverTile)
		{
			_lastHoverTile = tilePosition;

			Tilemaps.Highlights.ClearAllTiles();

			DisplayExplosiveShotArea(tilePosition);
		}
	}

	private void DisplayExplosiveShotArea(Vector3Int tilePosition)
	{
		for (int dx = -1; dx <= 1; dx++)
		{
			for (int dy = -1; dy <= 1; dy++)
			{
				Tilemaps.Highlights.SetTile(tilePosition + new Vector3Int(dx, dy, 0), HighlightTile);
			}
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
			DisplayHighlights();

			ExplosiveShotButton.SetActive(true);
			if (!TurretAttack.ExplosiveShotPlanned)
			{
				ExplosiveShotButton.GetComponent<Button>().interactable = true;
				ExplosiveShotText.text = "Explosive Shot";
			}
			else if (turret.WillFireExplosiveShot)
			{
				ExplosiveShotButton.GetComponent<Button>().interactable = true;
				ExplosiveShotText.text = "Cancel Shot";
			}
			else
			{
				ExplosiveShotButton.GetComponent<Button>().interactable = false;
				ExplosiveShotText.text = "Shot Planned Elsewhere";
			}
		}
	}

	private void DisplayHighlights()
	{
		System.Diagnostics.Debug.Assert(Selection != null, nameof(Selection) + " != null");
		GameObject tileLogic = Tilemaps.Buildings.GetInstantiatedObject(Selection.Value);

		if (!tileLogic) return;

		var turret = tileLogic.GetComponent<TurretAttack>();
		if (turret == null) return;

		if (IsPlanningExplosiveShot) return;

		if (turret.WillFireExplosiveShot)
		{
			DisplayExplosiveShotArea(TurretAttack.ExplosiveShotTarget);
		}
		else
		{
			foreach (Vector3Int position in turret.GetPositionsInRange())
			{
				Tilemaps.Highlights.SetTile(position, HighlightTile);
			}
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
		ExplosiveShotButton.SetActive(false);
	}

	private void DisplayGroundInfo()
	{
		NameText.text = GroundName;
		FlavourText.text = GroundFlavourText;
		ToggleButton.SetActive(false);
		ExplosiveShotButton.SetActive(false);
	}

	public void OnExplosiveShotClick()
	{
		System.Diagnostics.Debug.Assert(Selection != null, nameof(Selection) + " != null");
		var turret = Tilemaps.Buildings.GetInstantiatedObject(Selection.Value).GetComponent<TurretAttack>();

		if (turret.WillFireExplosiveShot)
		{
			CancelExplosiveShot(turret);
		}
		else
		{
			StartPlanningExplosiveShot();
		}
	}

	private static void CancelExplosiveShot(TurretAttack turret)
	{
		turret.WillFireExplosiveShot = false;
		TurretAttack.ExplosiveShotPlanned = false;
	}

	private void StartPlanningExplosiveShot()
	{
		Tilemaps.Highlights.ClearAllTiles();
		IsPlanningExplosiveShot = true;
		ExplosiveShotButton.GetComponent<Button>().Select();
	}

	private void FinishPlanningExplosiveShot(Vector3Int target)
	{
		System.Diagnostics.Debug.Assert(Selection != null, nameof(Selection) + " != null");
		var turret = Tilemaps.Buildings.GetInstantiatedObject(Selection.Value).GetComponent<TurretAttack>();

		EventSystem.current.SetSelectedGameObject(null);
		turret.WillFireExplosiveShot = true;
		TurretAttack.ExplosiveShotPlanned = true;
		TurretAttack.ExplosiveShotTarget = target;
		IsPlanningExplosiveShot = false;

		UpdateUIEvent.Raise();
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

		IsPlanningExplosiveShot = false;
	}
}