using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WorkerProvider : MonoBehaviour
{
	public IntReference PopulationRef;
	public IntReference WorkerCountRef;

	public string JobName;

	public int AssignedCount => WorkerPortraits.Count;
	private int _lastAssignedCount;

	public int Capacity { get; private set; }
	public Stack<Sprite> WorkerPortraits { get; private set; }

	private void OnEnable()
	{
		if (_lastAssignedCount <= 0) return;
		
		TileSelectionManager selectionManager = GameManager.Instance.TileSelectionManager;

		Vector3Int? originalSelection = selectionManager.Selection;
		selectionManager.SetSelection(GetComponentInParent<Tilemap>().WorldToCell(transform.position));
		while (AssignedCount < _lastAssignedCount)
		{
			selectionManager.AssignWorker();
		}

		_lastAssignedCount = 0;
		selectionManager.SetSelection(originalSelection);
	}

	private void Start()
	{
		Capacity = GetComponent<CapacityProvider>().CapacityDelta;
		WorkerPortraits = new Stack<Sprite>();
	}

	public void AssignWorker(Sprite portrait)
	{
		WorkerCountRef.value += 1;
		WorkerPortraits.Push(portrait);
	}

	public Sprite UnassignWorker()
	{
		WorkerCountRef.value -= 1;
		return WorkerPortraits.Pop();
	}

	public void KillWorkers()
	{
		WorkerCountRef.value -= AssignedCount;
		PopulationRef.value -= AssignedCount;
		WorkerPortraits.Clear();
	}

	private void OnDisable()
	{
		if (AssignedCount <= 0) return;
		
		_lastAssignedCount = AssignedCount;
		TileSelectionManager selectionManager = GameManager.Instance.TileSelectionManager;

		Vector3Int? originalSelection = selectionManager.Selection;
		selectionManager.SetSelection(GetComponentInParent<Tilemap	>().WorldToCell(transform.position));
		while (AssignedCount > 0)
		{
			selectionManager.UnassignWorker();
		}
		selectionManager.SetSelection(originalSelection);
	}
}