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
	public TileBase BulldozeTile;
	public GameObject ButtonGrid;

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
		if (!Input.GetMouseButtonDown(0) || ReferenceEquals(SelectedTile, null)) return;

		Ray mouseRay = _mainCamera.ScreenPointToRay(Input.mousePosition);
		if (!_zPlane.Raycast(mouseRay, out float distance))
		{
			Debug.LogError("Failed to raycast onto Z plane.");
			return;
		}

		Vector3Int tilePosition = Tilemaps.Ground.WorldToCell(mouseRay.GetPoint(distance));
		if (!Tilemaps.Ground.HasTile(tilePosition)) return;

		GameManager.Instance.ConstructionQueueManager.QueueConstruction(tilePosition, SelectedTile);
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

		if (tile != null)
		{
			GameManager.Instance.DisableOtherManagers(this);
			GameManager.Instance.SetStatusText(
				tile == BulldozeTile
					? "Planning bulldoze jobs"
					: $"Planning {tile.name} building construction"
			);
		}

		SelectBuildTileEvent.Raise();
	}
}