using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

public class ConstructionQueueManager : MonoBehaviour
{
	public TilemapRegistry Tilemaps;

	public GameObject ConstructionQueueGrid;
	public GameObject QueueItemPrefab;
	public TileBase BulldozeTile;

	public GameEvent UpdateUIEvent;
	[FormerlySerializedAs("OutlineTile")]
	[FormerlySerializedAs("HighlightFrameTile")]
	public TileBase HighlightTile;

	private readonly List<Tuple<Vector3Int, TileBase, GameObject>> _buildingQueue =
		new List<Tuple<Vector3Int, TileBase, GameObject>>();
	private int _selectedIndex = -1;

	public int QueueLength => _buildingQueue.Count;

	private void OnEnable()
	{
		foreach (Tuple<Vector3Int, TileBase, GameObject> queueItem in _buildingQueue)
		{
			queueItem.Item3.GetComponent<ConstructionQueueItemPanel>().SetButtonInteractable(true);
		}
	}

	public void QueueConstruction(Vector3Int tilePosition, TileBase selectedTile)
	{
		if (Tilemaps.ConstructionPlanner.HasTile(tilePosition))
		{
			for (var i = 0; i < QueueLength;)
			{
				if (_buildingQueue[i].Item1 == tilePosition)
				{
					CancelBuildOrder(i);
				}
				else
				{
					i++;
				}
			}
		}

		Tilemaps.ConstructionPlanner.SetTile(tilePosition, selectedTile);
		GameObject tileLogic = Tilemaps.ConstructionPlanner.GetInstantiatedObject(tilePosition)
		if (tileLogic)
		{
			tileLogic.SetActive(false);
		}

		GameObject queueItem = Instantiate(QueueItemPrefab, ConstructionQueueGrid.transform);
		var panelTransform = queueItem.GetComponent<RectTransform>();
		var panelController = queueItem.GetComponent<ConstructionQueueItemPanel>();

		var move = new Vector2(panelTransform.rect.width / 2 + panelController.Spacing, 0);
		panelTransform.offsetMin += move;
		panelTransform.offsetMax += move;
		panelController.BuildingSprite = ((Tile) selectedTile).sprite;
		panelController.SetQueueIndex(_buildingQueue.Count);
		if (!enabled) panelController.SetButtonInteractable(false);

		_buildingQueue.Add(
			new Tuple<Vector3Int, TileBase, GameObject>(
				tilePosition, selectedTile, queueItem
			)
		);
		UpdateUIEvent.Raise();
	}

	public void CancelBuildOrder(int index)
	{
		(Vector3Int tilePosition, TileBase selectedTile, GameObject queueItem) = _buildingQueue[index];
		_buildingQueue.RemoveAt(index);
		Destroy(queueItem);
		Tilemaps.ConstructionPlanner.SetTile(tilePosition, null);

		for (; index < _buildingQueue.Count; index++)
		{
			_buildingQueue[index].Item3.GetComponent<ConstructionQueueItemPanel>().SetQueueIndex(index);
		}
	}


	public void MoveQueueItem(int from, int to)
	{
		Tuple<Vector3Int, TileBase, GameObject> queueItem = _buildingQueue[from];
		_buildingQueue.RemoveAt(from);
		_buildingQueue.Insert(to, queueItem);

		for (var index = 0; index < QueueLength; index++)
		{
			_buildingQueue[index].Item3.GetComponent<ConstructionQueueItemPanel>().SetQueueIndex(index);
		}
	}

	public void SelectIndex(int index)
	{
		if (_selectedIndex >= 0)
		{
			Tilemaps.Highlights.SetTile(_buildingQueue[_selectedIndex].Item1, null);
		}

		_selectedIndex = index;

		if (_selectedIndex >= 0)
		{
			Tilemaps.Highlights.SetTile(_buildingQueue[_selectedIndex].Item1, HighlightTile);
			_buildingQueue[_selectedIndex].Item3.GetComponent<ConstructionQueueItemPanel>().SelfButton.Select();
			GameManager.Instance.DisableOtherManagers(this);
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

		for (int index = 0; index < QueueLength; index++)
		{
			_buildingQueue[index].Item3.GetComponent<ConstructionQueueItemPanel>().SetQueueIndex(index);
		}
	}

	private void OnDisable()
	{
		foreach (Tuple<Vector3Int, TileBase, GameObject> queueItem in _buildingQueue)
		{
			queueItem.Item3.GetComponent<ConstructionQueueItemPanel>().SetButtonInteractable(false);
		}
	}
}