using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemyManager : MonoBehaviour
{
	public Tilemap OuterEdgeMap;
	public Tilemap EnemyMap;
	public TileBase EnemyTile;

	private List<Vector3Int> _validSpawnPositions;

	private void Start()
	{
		_validSpawnPositions = new List<Vector3Int>();

		foreach (Vector3Int position in OuterEdgeMap.cellBounds.allPositionsWithin)
		{
			if (OuterEdgeMap.HasTile(position))
			{
				_validSpawnPositions.Add(position);
			}
		}
	}

	public void SpawnRandomEnemy()
	{
		List<Vector3Int> attemptOrder = new List<Vector3Int>(_validSpawnPositions);
		for (int currentIndex = 0; currentIndex < attemptOrder.Count; currentIndex++)
		{
			int targetIndex = Random.Range(currentIndex, attemptOrder.Count);
			Vector3Int temp = attemptOrder[targetIndex];
			attemptOrder[targetIndex] = attemptOrder[currentIndex];
			attemptOrder[currentIndex] = temp;
		}

		foreach (Vector3Int position in attemptOrder)
		{
			if (EnemyMap.HasTile(position)) continue;

			EnemyMap.SetTile(position, EnemyTile);
			
			return;
		}
		
		Debug.LogWarning("Failed to spawn enemy! Is the outer edge saturated?");
	}

	public void DebugRunPathfinding()
	{
		foreach (Transform child in EnemyMap.transform)
		{
			Debug.Log(child.GetComponent<EnemyLogic>().CalculateMove());
		}
	}
}