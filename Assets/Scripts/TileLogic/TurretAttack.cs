using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class TurretAttack : MonoBehaviour
{
	public int AttackStrength;
	public int Range;

	[Header("Effects and Timing")]
	public GameObject ProjectilePrefab;
	public float ProjectileLifetime;
	public float AftermathInterval;

	private Tilemap _enemyTilemap;
	private Tilemap _buildingTilemap;
	private PowerConsumer _power;

	private void Start()
	{
		_enemyTilemap = GetComponentInParent<TilemapRegistry>().Enemies;
		_buildingTilemap = GetComponentInParent<Tilemap>();
		_power = GetComponent<PowerConsumer>();
	}

	private IEnumerator BuildingActionSequence()
	{
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

		if (nearbyEnemies.Count <= 0) yield break;
		yield return StartCoroutine(AttackEnemy(nearbyEnemies[Random.Range(0, nearbyEnemies.Count)]));
	}

	public void OnBuildingAction()
	{
		// TODO implement power mechanics
//		if (!_power.enabled || _worker.AssignedCount != _worker.Capacity) return;

		EndTurnManager.actions.Enqueue(BuildingActionSequence());
	}

	private IEnumerator AttackEnemy(Vector3Int targetPosition)
	{
		var enemyHealth = _enemyTilemap.GetInstantiatedObject(targetPosition).GetComponent<HealthPool>();
		Transform projectileTransform = Instantiate(ProjectilePrefab).transform;

		Vector3 startPosition = _buildingTilemap.GetCellCenterWorld(_buildingTilemap.WorldToCell(transform.position));
		Vector3 enemyPosition = _enemyTilemap.GetCellCenterWorld(targetPosition);

		projectileTransform.position = startPosition + Vector3.forward;
		projectileTransform.rotation =
			Quaternion.AngleAxis(
				Mathf.Atan2(
					enemyPosition.y - projectileTransform.position.y,
					enemyPosition.x - projectileTransform.position.x
				) * Mathf.Rad2Deg,
				Vector3.forward
			);

		float startTime = Time.time;

		while ((Time.time - startTime) < ProjectileLifetime)
		{
			projectileTransform.position = Vector3.Lerp(
				                               startPosition, enemyPosition,
				                               (Time.time - startTime) / ProjectileLifetime
			                               ) + Vector3.forward;
			yield return null;
		}

		Destroy(projectileTransform.gameObject);

		enemyHealth.Damage(AttackStrength);
		
		yield return new WaitForSeconds(AftermathInterval);
	}
}