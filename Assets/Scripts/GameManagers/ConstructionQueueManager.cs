using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

public class ConstructionQueueManager : MonoBehaviour
{
	public TilemapRegistry Tilemaps;

	[Header("Prefabs")]
	public GameObject ConstructionQueueGrid;
	public GameObject QueueItemPrefab;
	public TileBase BulldozeTile;

	[Header("Rendering")]
	public GameEvent UpdateUIEvent;
	[FormerlySerializedAs("OutlineTile")]
	[FormerlySerializedAs("HighlightFrameTile")]
	public TileBase HighlightTile;
	public TextMeshProUGUI ConstructionProgressText;
	public TextMeshProUGUI ConstructionPowerText;

	private readonly List<Tuple<Vector3Int, TileBase, GameObject>> _buildingQueue =
		new List<Tuple<Vector3Int, TileBase, GameObject>>();
	private int _selectedIndex = -1;
	private int _currentProgress;

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
		GameObject tileLogic = Tilemaps.ConstructionPlanner.GetInstantiatedObject(tilePosition);
		if (tileLogic)
		{
			tileLogic.SetActive(false);
		}

		GameObject queueItem = Instantiate(QueueItemPrefab, ConstructionQueueGrid.transform);
		var panelTransform = queueItem.GetComponent<RectTransform>();
		var panelController = queueItem.GetComponent<ConstructionQueueItemPanel>();

		var move = new Vector2(panelTransform.rect.width / 2, 0);
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
		_currentProgress += GameManager.Instance.ResourceManager.PowerProduced;

		while (true)
		{
			(Vector3Int tilePosition, TileBase selectedTile, GameObject queueItem) = _buildingQueue[0];
			int buildingCost = Tilemaps.ConstructionPlanner
				.GetInstantiatedObject(tilePosition)
				.GetComponent<BuildCost>().Cost;

			if (_currentProgress < buildingCost) break;
			_currentProgress -= buildingCost;

			_buildingQueue.RemoveAt(0);
			Destroy(queueItem);
			Tilemaps.Buildings.SetTile(tilePosition, selectedTile == BulldozeTile ? null : selectedTile);
			Tilemaps.ConstructionPlanner.SetTile(tilePosition, null);
		}

		for (var index = 0; index < QueueLength; index++)
		{
			_buildingQueue[index].Item3.GetComponent<ConstructionQueueItemPanel>().SetQueueIndex(index);
		}

		Display();
	}

	public void Display()
	{
		if (_buildingQueue.Count > 0)
		{
			(Vector3Int tilePosition, TileBase selectedTile, GameObject queueItem) = _buildingQueue[0];
			ConstructionProgressText.text =
				$"Progress:\n{_currentProgress} / {Tilemaps.ConstructionPlanner.GetInstantiatedObject(tilePosition).GetComponent<BuildCost>().Cost}";
		}
		else
		{
			ConstructionProgressText.text = "Progress:\nIdle";
		}

		ConstructionPowerText.text = $"Build Speed:\n+{GameManager.Instance.ResourceManager.PowerProduced} / turn";
	}

	private void OnDisable()
	{
		foreach (Tuple<Vector3Int, TileBase, GameObject> queueItem in _buildingQueue)
		{
			queueItem.Item3.GetComponent<ConstructionQueueItemPanel>().SetButtonInteractable(false);
		}
	}
}