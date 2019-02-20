using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ConstructionQueueManager : MonoBehaviour
{
	public TilemapRegistry Tilemaps;

	public GameObject ConstructionQueueGrid;
	public GameObject QueueItemPrefab;
	public TileBase BulldozeTile;

	public GameEvent UpdateUIEvent;

	private readonly List<Tuple<Vector3Int, TileBase, GameObject>> _buildingQueue =
		new List<Tuple<Vector3Int, TileBase, GameObject>>();

	public int QueueLength => _buildingQueue.Count;

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
		Tilemaps.ConstructionPlanner.GetInstantiatedObject(tilePosition).SetActive(false);

		GameObject queueItem = Instantiate(QueueItemPrefab, ConstructionQueueGrid.transform);
		var panelTransform = queueItem.GetComponent<RectTransform>();
		var move = new Vector2(panelTransform.rect.width, 0);
		panelTransform.offsetMin += move / 2;
		panelTransform.offsetMax += move / 2;
		var panelController = queueItem.GetComponent<ConstructionQueueItemPanel>();
		panelController.BuildingSprite = ((Tile) selectedTile).sprite;
		panelController.SetQueueIndex(_buildingQueue.Count);

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
}