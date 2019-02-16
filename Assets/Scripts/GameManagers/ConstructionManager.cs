using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class ConstructionManager : MonoBehaviour
{
	public TilemapRegistry Tilemaps;
	public GameEvent SelectBuildTileEvent;

	private Camera _mainCamera;
	private static Plane _zPlane = new Plane(Vector3.forward, 0);

	public TileBase SelectedTile { get; private set; }
	public GameObject ButtonGrid;
	public TileBase BulldozeTile;

	[Header("Rendering")]
	public GameEvent UpdateUIEvent;
	public GameObject ConstructionQueueGrid;
	public GameObject QueueItemPrefab;

	private List<Tuple<Vector3Int, TileBase, GameObject>> _buildingQueue;

	private void Start()
	{
		_mainCamera = Camera.main;
		_buildingQueue = new List<Tuple<Vector3Int, TileBase, GameObject>>();
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

		Tilemaps.ConstructionPlanner.SetTile(tilePosition, SelectedTile);

		GameObject queueItem = Instantiate(QueueItemPrefab, ConstructionQueueGrid.transform);
		var panelTransform = queueItem.GetComponent<RectTransform>();
		Vector2 move = new Vector2(panelTransform.rect.width, 0);
		panelTransform.offsetMin += move / 2;
		panelTransform.offsetMax += move / 2;
		var panelController = queueItem.GetComponent<ConstructionQueueItemPanel>();
		panelController.BuildingSprite = ((Tile) SelectedTile).sprite;
		panelController.SetQueueIndex(_buildingQueue.Count);

		_buildingQueue.Add(
			new Tuple<Vector3Int, TileBase, GameObject>(
				tilePosition, SelectedTile, queueItem
			)
		);
		UpdateUIEvent.Raise();
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
		GameManager.Instance.DisableOtherManagers(this);
		SelectBuildTileEvent.Raise();
	}

	public void CancelBuildOrder(int index)
	{
		(Vector3Int tilePosition, TileBase selectedTile, GameObject queueItem) = _buildingQueue[index];
		_buildingQueue.RemoveAt(index);
		Destroy(queueItem);
		Tilemaps.ConstructionPlanner.SetTile(tilePosition, null);
		for (var i = 0; i < _buildingQueue.Count; i++)
		{
			_buildingQueue[i].Item3.GetComponent<ConstructionQueueItemPanel>().SetQueueIndex(i);
		}
	}

	public void ExecuteBuildOrder()
	{
		if (_buildingQueue.Count <= 0) return;

		(Vector3Int tilePosition, TileBase selectedTile, GameObject queueItem) = _buildingQueue[0];
		_buildingQueue.RemoveAt(0);
		Destroy(queueItem);
		Tilemaps.Buildings.SetTile(tilePosition, selectedTile == BulldozeTile ? null : selectedTile);
		Tilemaps.ConstructionPlanner.SetTile(tilePosition, null);
	}
}