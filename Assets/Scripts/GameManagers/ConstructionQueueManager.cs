﻿using System;
using System.Collections;
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

	[Header("Events")]
	public GameEvent buildingEnqueuedEvent;
	public GameEvent updateUIEvent;
	public GameEvent executeEvent;

	[Header("Rendering")]
	[FormerlySerializedAs("OutlineTile")]
	[FormerlySerializedAs("HighlightFrameTile")]
	public TileBase HighlightTile;
	public TextMeshProUGUI ConstructionProgressText;
	public TextMeshProUGUI ConstructionPowerText;
	public float BuildActionInterval;

	private readonly List<Tuple<Vector3Int, TileBase, GameObject>> _buildingQueue =
		new List<Tuple<Vector3Int, TileBase, GameObject>>();
	private int _selectedIndex = -1;

	public int QueueLength => _buildingQueue.Count;

	private void Awake()
	{
		updateUIEvent.AddListener(Display);
		executeEvent.AddListener(ExecuteBuildOrder);
	}

	private void OnEnable()
	{
		foreach (Tuple<Vector3Int, TileBase, GameObject> queueItem in _buildingQueue)
		{
			queueItem.Item3.GetComponent<ConstructionQueueItemPanel>().SetButtonInteractable(true);
		}
	}

	public void QueueConstruction(Vector3Int tilePosition, TileBase selectedTile)
	{
		if (Tilemaps.Enemies.HasTile(tilePosition))
		{
			GameManager.Instance.FlytextManager.SpawnFlytextWorldPosition(
				Tilemaps.ConstructionPlanner.GetCellCenterWorld(tilePosition),
				"Occupied by enemy!",
				0, 0, 2
			);
			return;
		}

		CancelConstructionAtPosition(tilePosition);

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

		buildingEnqueuedEvent.Raise();
		updateUIEvent.Raise();
	}

	public void CancelConstructionAtPosition(Vector3Int tilePosition)
	{
		if (!Tilemaps.ConstructionPlanner.HasTile(tilePosition)) return;

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

	public void CancelBuildOrder(int index)
	{
		if (_selectedIndex == index)
		{
			GameManager.Instance.EnterSelectionMode();
		}

		(Vector3Int tilePosition, TileBase selectedTile, GameObject queueItem) = _buildingQueue[index];
		_buildingQueue.RemoveAt(index);
		Destroy(queueItem);
		Tilemaps.ConstructionPlanner.SetTile(tilePosition, null);

		for (; index < _buildingQueue.Count; index++)
		{
			_buildingQueue[index].Item3.GetComponent<ConstructionQueueItemPanel>().SetQueueIndex(index);
		}

		updateUIEvent.Raise();
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

		if (_selectedIndex == from)
		{
			SelectIndex(to);
		}

		updateUIEvent.Raise();
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
			Vector3Int position = _buildingQueue[_selectedIndex].Item1;

			Tilemaps.Highlights.SetTile(position, HighlightTile);
			_buildingQueue[_selectedIndex].Item3.GetComponent<ConstructionQueueItemPanel>().SelfButton.Select();
			GameManager.Instance.DisableOtherManagers(this);
			GameManager.Instance.SetStatusText(
				$"Previewing construction order #{_selectedIndex + 1}\n"
				+ $"at {(Vector2Int) position}"
			);
		}
	}

	public void ExecuteBuildOrder()
	{
		EndTurnManager.actions.Enqueue(ExecuteBuildOrderCoroutine());
	}

	private IEnumerator ExecuteBuildOrderCoroutine()
	{
		if (_buildingQueue.Count <= 0) yield break;
		int buildUnits = GameManager.Instance.ResourceManager.PowerProduced;

		while (_buildingQueue.Count > 0 && buildUnits > 0)
		{
			(Vector3Int tilePosition, TileBase selectedTile, GameObject queueItem) = _buildingQueue[0];
			var buildCost = Tilemaps.ConstructionPlanner
				.GetInstantiatedObject(tilePosition)
				.GetComponent<BuildCost>();
			var queueItemProgress = queueItem.GetComponent<ConstructionQueueItemProgress>();

			int buildDeficit = buildCost.RequiredProgress - queueItemProgress.CurrentProgress;
			queueItemProgress.CurrentProgress += buildUnits;
			if (queueItemProgress.CurrentProgress < buildCost.RequiredProgress)
			{
				GameManager.Instance.FlytextManager.SpawnFlytextWorldPosition(
					Tilemaps.ConstructionPlanner.GetCellCenterWorld(tilePosition),
					$"+{buildUnits} progress\n"
					+ $"({queueItemProgress.CurrentProgress}/{buildCost.RequiredProgress})",
					0.2f, 0.2f, 0.5f
				);
				yield return new WaitForSeconds(BuildActionInterval);

				break;
			}

			buildUnits = queueItemProgress.CurrentProgress - buildCost.RequiredProgress;
			_buildingQueue.RemoveAt(0);
			Destroy(queueItem);
			Tilemaps.Buildings.SetTile(tilePosition, selectedTile == BulldozeTile ? null : selectedTile);
			Tilemaps.ConstructionPlanner.SetTile(tilePosition, null);

			GameManager.Instance.FlytextManager.SpawnFlytextWorldPosition(
				Tilemaps.ConstructionPlanner.GetCellCenterWorld(tilePosition),
				$"+{buildDeficit} progress\nFinished!",
				0.2f, 0.2f, 0.5f
			);

			for (var index = 0; index < QueueLength; index++)
			{
				_buildingQueue[index].Item3.GetComponent<ConstructionQueueItemPanel>().SetQueueIndex(index);
			}

			updateUIEvent.Raise();

			yield return new WaitForSeconds(BuildActionInterval);
		}
	}

	public void Display()
	{
		if (_buildingQueue.Count > 0)
		{
			(Vector3Int tilePosition, TileBase _, GameObject queueItem) = _buildingQueue[0];
			var buildCost = Tilemaps.ConstructionPlanner.GetInstantiatedObject(tilePosition).GetComponent<BuildCost>();
			var queueItemProgress = queueItem.GetComponent<ConstructionQueueItemProgress>();
			ConstructionProgressText.text =
				$"Progress:\n{queueItemProgress.CurrentProgress} / {buildCost.RequiredProgress}";
		}
		else
		{
			ConstructionProgressText.text = "Progress:\nIdle";
		}

		var gridTransform = ConstructionQueueGrid.GetComponent<RectTransform>();
		gridTransform.offsetMax = new Vector2(
			_buildingQueue.Count * (QueueItemPrefab.GetComponent<ConstructionQueueItemPanel>().Spacing
			                        + QueueItemPrefab.GetComponent<RectTransform>().rect.width),
			0
		);
		ConstructionPowerText.text = $"Build Speed:\n+{GameManager.Instance.ResourceManager.PowerProduced} / turn";
	}

	private void OnDisable()
	{
		foreach (Tuple<Vector3Int, TileBase, GameObject> queueItem in _buildingQueue)
		{
			queueItem.Item3.GetComponent<ConstructionQueueItemPanel>().SetButtonInteractable(false);
		}
	}

	private void OnDestroy()
	{
		updateUIEvent.RemoveListener(Display);
		executeEvent.RemoveListener(ExecuteBuildOrder);
	}
}