using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class PlanConstructionManager : MonoBehaviour
{
	public TilemapRegistry Tilemaps;
	public GameEvent SelectBuildTileEvent;

	private Camera _mainCamera;
	private static Plane _zPlane = new Plane(Vector3.forward, 0);

	public TileBase SelectedTile { get; private set; }
	public GameObject ButtonGrid;

	[Header("Special Cases")]
	public TileBase BulldozeTile;
	public TileBase TurretTile;
	public TileBase HighlightTile;
	private Vector3Int? _lastHoverTile;

	private void Start()
	{
		_mainCamera = Camera.main;
	}

	private void OnEnable()
	{
		foreach (Transform button in ButtonGrid.transform)
		{
			button.GetComponent<Button>().interactable = true;
		}
	}

	private void Update()
	{
		if (ReferenceEquals(SelectedTile, null)) return;

		if (!GetHoverPosition(out Vector3Int tilePosition))
		{
			if (_lastHoverTile == null) return;
			Tilemaps.ConstructionPreview.ClearAllTiles();
			Tilemaps.Highlights.ClearAllTiles();
			_lastHoverTile = null;
			return;
		}

		if (tilePosition != _lastHoverTile)
		{
			if (_lastHoverTile != null)
			{
				Tilemaps.ConstructionPreview.SetTile(_lastHoverTile.Value, null);
			}

			Tilemaps.ConstructionPreview.SetTile(tilePosition, SelectedTile);

			if (SelectedTile == TurretTile)
			{
				Tilemaps.Highlights.ClearAllTiles();

				foreach (Vector3Int target in ((Tile) TurretTile)
					.gameObject
					.GetComponent<TurretAttack>()
					.GetPositionsInRange(
						tilePosition,
						target => Tilemaps.Ground.HasTile(target) || Tilemaps.OuterEdge.HasTile(target)
					)
				)
				{
					Tilemaps.Highlights.SetTile(target, HighlightTile);
				}
			}

			_lastHoverTile = tilePosition;
		}

		if (Input.GetMouseButtonDown(0))
		{
			GameManager.Instance.ConstructionQueueManager.QueueConstruction(tilePosition, SelectedTile);
		}
	}

	private bool GetHoverPosition(out Vector3Int tilePosition)
	{
		Ray mouseRay = _mainCamera.ScreenPointToRay(Input.mousePosition);
		if (!_zPlane.Raycast(mouseRay, out float distance))
		{
			Debug.LogError("Failed to raycast onto Z plane.");
			tilePosition = Vector3Int.zero;
			return false;
		}

		tilePosition = Tilemaps.Ground.WorldToCell(mouseRay.GetPoint(distance));
		return Tilemaps.Ground.HasTile(tilePosition);
	}

	private void OnDisable()
	{
		foreach (Transform button in ButtonGrid.transform)
		{
			button.GetComponent<Button>().interactable = false;
		}
	}

	public void SelectBuildTile(TileBase tile)
	{
		SelectedTile = tile;

		if (!ReferenceEquals(SelectedTile, null))
		{
			GameManager.Instance.DisableOtherManagers(this);
			GameManager.Instance.SetStatusText(
				tile == BulldozeTile
					? "Planning bulldoze jobs"
					: $"Planning {((Tile) SelectedTile).gameObject.GetComponent<BuildingDescription>().Name.ToLower()} construction"
			);
		}

		if (_lastHoverTile != null)
		{
			Tilemaps.ConstructionPreview.ClearAllTiles();
			Tilemaps.Highlights.ClearAllTiles();
		}

		SelectBuildTileEvent.Raise();
	}
}