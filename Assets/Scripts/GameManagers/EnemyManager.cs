using System.Collections;
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

	[Header("Effects and Timing")]
	public float AttackAftermathInterval;
	public float MovementAftermathInterval;

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
				EndTurnManager.actions.Enqueue(AttackBuildingCoroutine(tilePosition, bestTarget));
			}
			else
			{
				EndTurnManager.actions.Enqueue(MoveEnemyCoroutine(tilePosition, bestTarget));
			}
		}
	}

	private IEnumerator AttackBuildingCoroutine(Vector3Int enemyPosition, Vector3Int targetPosition)
	{
		int attackStrength = Tilemaps.Enemies
			.GetInstantiatedObject(enemyPosition)
			.GetComponent<EnemyAttack>()
			.AttackStrength;
		Tilemaps.Buildings
			.GetInstantiatedObject(targetPosition)
			.GetComponent<HealthPool>()
			.Damage(attackStrength);

		yield return new WaitForSeconds(AttackAftermathInterval);
	}

	private IEnumerator MoveEnemyCoroutine(Vector3Int from, Vector3Int to)
	{
		TileBase enemyTile = Tilemaps.Enemies.GetTile(from);
		Tilemaps.Enemies.SetTile(to, enemyTile);

		GameObject oldEnemy = Tilemaps.Enemies.GetInstantiatedObject(from);
		GameObject newEnemy = Tilemaps.Enemies.GetInstantiatedObject(to);
		newEnemy.GetComponent<EnemyAttack>().AttackStrength =
			oldEnemy.GetComponent<EnemyAttack>().AttackStrength;
		var oldEnemyHealth = oldEnemy.GetComponent<HealthPool>();
		if (oldEnemyHealth.Health < oldEnemyHealth.MaxHealth)
		{
			newEnemy.GetComponent<HealthPool>().Damage(oldEnemyHealth.MaxHealth - oldEnemyHealth.Health);
		}

		Tilemaps.Enemies.SetTile(from, null);

		yield return new WaitForSeconds(MovementAftermathInterval);
	}
}