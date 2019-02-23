using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class EnemyManager : MonoBehaviour
{
	[Header("Game Objects")]
	public TilemapRegistry Tilemaps;
	public IntReference TurnCountRef;

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

			Tilemaps.Enemies.SetTile(position, GroundEnemyTile);

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

	public void ExecuteEnemySpawns()
	{
		if (TurnCountRef < 5) return;
		
		for (int i = 0; i < 2; i++)
		{
			SpawnRandomEnemy();
		}
	}
	
	public void ExecuteGroundEnemyActions()
	{
		Dictionary<Vector3Int, float> attractionMap =
			GroundEnemyPathfinding.MakeAttractionMap(Tilemaps.Buildings, _pathfindPositions);

		// It is necessary to first store all the children so that moving children in the loop
		// doesn't cause re-instantiated enemies to get another action.
		var children = new Queue<Transform>();
		foreach (Transform child in Tilemaps.Enemies.transform)
		{
			children.Enqueue(child);
		}

		foreach (Transform child in children)
		{
			Vector3Int tilePosition = Tilemaps.Enemies.WorldToCell(child.position);
			TileBase enemyTile = Tilemaps.Enemies.GetTile(tilePosition);

			float maxAttraction = float.MinValue;
			Vector3Int bestTarget = tilePosition;
			foreach (Vector3Int direction in new[] {Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right})
			{
				Vector3Int target = tilePosition + direction;
				if (Tilemaps.Enemies.HasTile(target)) continue;
				if (!attractionMap.ContainsKey(target)) continue;

				float tileAttraction = attractionMap[target];
				if (tileAttraction <= maxAttraction) continue;

				maxAttraction = tileAttraction;
				bestTarget = target;
			}

			if (Tilemaps.Buildings.HasTile(bestTarget))
			{
				int attackStrength = Tilemaps.Enemies
					.GetInstantiatedObject(tilePosition)
					.GetComponent<EnemyAttack>()
					.AttackStrength;
				Tilemaps.Buildings
					.GetInstantiatedObject(bestTarget)
					.GetComponent<HealthPool>()
					.Damage(attackStrength);
			}
			else
			{
				Tilemaps.Enemies.SetTile(tilePosition, null);
				Tilemaps.Enemies.SetTile(bestTarget, enemyTile);
			}
		}
	}
}