using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class EnemyManager : MonoBehaviour
{
	[Header("Game Objects")]
	public TilemapRegistry Tilemaps;
	public TileBase EnemyTile;

	private List<Vector3Int> _validSpawnPositions;

	[Header("Enemy Logic")]
	public TileBase GroundEnemyTile;
	public EnemyPathfinding GroundEnemyPathfinding;

	private List<Vector3Int> _pathfindPositions;

	[Header("Debug")]
	public GameObject DebugParent;
	public GameObject TextPrefab;

	private void Start()
	{
		_validSpawnPositions = new List<Vector3Int>();
		_pathfindPositions = new List<Vector3Int>();

		foreach (Vector3Int position in Tilemaps.OuterEdge.cellBounds.allPositionsWithin)
		{
			if (!Tilemaps.OuterEdge.HasTile(position)) continue;
			
			_validSpawnPositions.Add(position);
			_pathfindPositions.Add(position);
		}

		foreach (Vector3Int position in Tilemaps.Ground.cellBounds.allPositionsWithin)
		{
			if (!Tilemaps.Ground.HasTile(position)) continue;
			
			_pathfindPositions.Add(position);
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
			if (Tilemaps.Enemies.HasTile(position)) continue;

			Tilemaps.Enemies.SetTile(position, EnemyTile);

			return;
		}

		Debug.LogWarning("Failed to spawn enemy! Is the outer edge saturated?");
	}

	public void DebugRunPathfinding()
	{
		if (DebugParent.transform.childCount > 0)
		{
			foreach (Transform child in DebugParent.transform)
			{
				Destroy(child.gameObject);
			}
		}
		else
		{
			Camera mainCamera = Camera.main;
			
			Dictionary<Vector3Int, float> attractionMap =
				GroundEnemyPathfinding.MakeAttractionMap(Tilemaps.Buildings, _pathfindPositions);

			foreach (KeyValuePair<Vector3Int, float> pair in attractionMap)
			{
				GameObject textInstance = Instantiate(TextPrefab, DebugParent.transform);
				textInstance.transform.position =
					mainCamera.WorldToScreenPoint(Tilemaps.Buildings.GetCellCenterWorld(pair.Key));
				textInstance.GetComponent<Text>().text = pair.Value.ToString(CultureInfo.InvariantCulture);
			}
		}
	}
}