using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

public class BuildOnTile : MonoBehaviour
{
	public Tilemap GroundMap;
	public Tilemap BuildingMap;
	public GameEvent SelectBuildTileEvent;
	public GameEvent UpdateUIEvent;

	private Camera _mainCamera;
	private static Plane _zPlane = new Plane(Vector3.forward, 0);

	public TileBase SelectedTile { get; private set; }
	public TileBase BulldozeTile;

	private List<Tuple<Vector3Int, TileBase>> _buildingQueue;

	private void Start()
	{
		_mainCamera = Camera.main;
		_buildingQueue = new List<Tuple<Vector3Int, TileBase>>();
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

		_buildingQueue.Add(
			new Tuple<Vector3Int, TileBase>(tilePosition, SelectedTile == BulldozeTile ? null : SelectedTile)
		);
		Debug.Log($"Building queue has {_buildingQueue.Count} members.");
	}

	public void SelectBuildTile(TileBase tile)
	{
		SelectedTile = tile;
		SelectBuildTileEvent.Raise();
	}

	public void ExecuteBuildOrder()
	{
		if (_buildingQueue.Count <= 0) return;

		(Vector3Int tilePosition, TileBase selectedTile) = _buildingQueue[0];
		_buildingQueue.RemoveAt(0);
		BuildingMap.SetTile(tilePosition, selectedTile);
		UpdateUIEvent.Raise();
		Debug.Log($"Building queue has {_buildingQueue.Count} members.");
	}
}