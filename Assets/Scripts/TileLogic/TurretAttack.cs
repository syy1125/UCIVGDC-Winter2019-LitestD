using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class TurretAttack : MonoBehaviour
{
	public int AttackStrength;
	public int Range;

	private Tilemap _enemyTilemap;
	private Tilemap _buildingTilemap;
	private PowerConsumer _power;
	private WorkerProvider _worker;

	private void Start()
	{
		_enemyTilemap = GetComponentInParent<TilemapRegistry>().Enemies;
		_buildingTilemap = GetComponentInParent<Tilemap>();
		_power = GetComponent<PowerConsumer>();
		_worker = GetComponent<WorkerProvider>();
	}

	public void OnBuildingAction()
	{
		if (!_power.enabled || _worker.AssignedCount != _worker.Capacity) return;

		Vector3Int turretPosition = _buildingTilemap.WorldToCell(transform.position);
		var nearbyEnemies = new List<Vector3Int>();
		for (int x = -Range; x <= Range; x++)
		{
			for (int y = -Range; y <= Range; y++)
			{
				if (Mathf.Abs(x) + Mathf.Abs(y) > Range) continue;
				var offset = new Vector3Int(x, y, 0);
				Vector3Int target = turretPosition + offset;

				if (!_enemyTilemap.HasTile(target)) continue;
				nearbyEnemies.Add(target);
			}
		}

		if (nearbyEnemies.Count <= 0) return;
		AttackEnemy(nearbyEnemies[Random.Range(0, nearbyEnemies.Count)]);
	}

	private void AttackEnemy(Vector3Int targetPosition)
	{
		var enemyHealth = _enemyTilemap.GetInstantiatedObject(targetPosition).GetComponent<HealthPool>();
		enemyHealth.Damage(AttackStrength);
	}
}