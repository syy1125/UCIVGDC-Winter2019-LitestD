﻿using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
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
	public float SpawnInterval;
	public float AttackAftermathInterval;
	public float MovementAftermathInterval;
	public AnimationCurve SpeedupFactorCurve;
	public int SpeedupCurveMin;
	public int SpeedupCurveMax;
	//public int turnOfFirstAttack = 5;
	public EnemySpawning enemySpawning;

	private float _speedupFactor;

	private bool _hasEnemySpawned;

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
		EndTurnManager.actions.Enqueue(EnemySpawnsCoroutine());
	}

	private IEnumerator EnemySpawnsCoroutine()
	{
		int enemiesToSpawn = enemySpawning.HowManyEnemiesToSpawn(TurnCountRef);

		if (enemiesToSpawn > 0 && !_hasEnemySpawned)
		{
			_hasEnemySpawned = true;
			MusicManager.Instance.TransitionToTrack(MusicManager.Instance.InvasionMusic);
		}

		for (var i = 0; i < enemiesToSpawn; i++)
		{
			SpawnRandomEnemy();
			yield return new WaitForSeconds(SpawnInterval);
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

		_speedupFactor = SpeedupFactorCurve.Evaluate(
			Mathf.Clamp01(
				(float) (children.Count - SpeedupCurveMin) / (SpeedupCurveMax - SpeedupCurveMin)
			)
		);

		EndTurnManager.actions.Enqueue(GroundEnemyActionsCoroutine(children, attractionMap));
	}

	private IEnumerator GroundEnemyActionsCoroutine(
		IEnumerable<Transform> children,
		IReadOnlyDictionary<Vector3Int, float> attractionMap
	)
	{
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
				yield return StartCoroutine(AttackBuildingCoroutine(tilePosition, bestTarget));
			}
			else
			{
				yield return StartCoroutine(MoveEnemyCoroutine(tilePosition, bestTarget));
			}
		}
	}

	private IEnumerator AttackBuildingCoroutine(Vector3Int enemyPosition, Vector3Int targetPosition)
	{
		GameObject oldEnemy = Tilemaps.Enemies.GetInstantiatedObject(enemyPosition);
		var oldAttack = oldEnemy.GetComponent<EnemyAttack>();
		var oldHealth = oldEnemy.GetComponent<HealthPool>();

		var tile = Tilemaps.Enemies.GetTile<ConditionalTile>(enemyPosition);
		Vector3Int direction = targetPosition - enemyPosition;
		tile.State = GetStateFromDirection(direction);
		Tilemaps.Enemies.RefreshTile(enemyPosition);

		GameObject newEnemy = Tilemaps.Enemies.GetInstantiatedObject(enemyPosition);
		PersistEnemyAttack(oldAttack, newEnemy.GetComponent<EnemyAttack>());
		PersistEnemyHealth(oldHealth, newEnemy.GetComponent<HealthPool>());

		var attack = newEnemy.GetComponent<EnemyAttack>();

		Tilemaps.Buildings
			.GetInstantiatedObject(targetPosition)
			.GetComponent<HealthPool>()
			.Damage(attack.AttackStrength);
		attack.PlayAttackSound();

		yield return new WaitForSeconds(AttackAftermathInterval * _speedupFactor);
	}

	private IEnumerator MoveEnemyCoroutine(Vector3Int from, Vector3Int to)
	{
		TileBase enemyTile = Tilemaps.Enemies.GetTile(from);
		Tilemaps.Enemies.SetTile(to, enemyTile);

		Tilemaps.Enemies.GetTile<ConditionalTile>(to).State = GetStateFromDirection(to - from);
		Tilemaps.Enemies.RefreshTile(to);

		GameObject oldEnemy = Tilemaps.Enemies.GetInstantiatedObject(from);
		GameObject newEnemy = Tilemaps.Enemies.GetInstantiatedObject(to);
		PersistEnemyAttack(oldEnemy.GetComponent<EnemyAttack>(), newEnemy.GetComponent<EnemyAttack>());
		PersistEnemyHealth(oldEnemy.GetComponent<HealthPool>(), newEnemy.GetComponent<HealthPool>());

		Tilemaps.Enemies.SetTile(from, null);

		if (Tilemaps.ConstructionPlanner.HasTile(to))
		{
			GameManager.Instance.ConstructionQueueManager.CancelConstructionAtPosition(to);
			GameManager.Instance.FlytextManager.SpawnFlytextWorldPosition(
				Tilemaps.ConstructionPlanner.GetCellCenterWorld(to),
				"Cancelled by enemy occupation!",
				0, 0, 2
			);
		}

		yield return new WaitForSeconds(MovementAftermathInterval * _speedupFactor);
	}

	private static void PersistEnemyAttack(EnemyAttack oldAttack, EnemyAttack newAttack)
	{
		newAttack.AttackStrength = oldAttack.AttackStrength;
	}

	private static void PersistEnemyHealth(HealthPool oldHealth, HealthPool newHealth)
	{
		if (oldHealth.Health < oldHealth.MaxHealth)
		{
			newHealth.Damage(oldHealth.MaxHealth - oldHealth.Health);
		}
	}

	private static string GetStateFromDirection(Vector3Int direction)
	{
		if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
		{
			return direction.x > 0 ? "NE" : "SW";
		}

		if (Mathf.Abs(direction.x) < Mathf.Abs(direction.y))
		{
			return direction.y > 0 ? "NW" : "SE";
		}

		return null;
	}
}