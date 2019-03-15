using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class TurretAttack : MonoBehaviour
{
	public int AttackStrength;
	public int Range;
	public int ExplosiveAttackStrength;

	[Header("Effects and Timing")]
	public GameObject ProjectilePrefab;
	public float WarmUpInterval;
	public float ProjectileLifetime;
	public float AftermathInterval;
	public float MinPitch;
	public float MaxPitch;

	public TileBase HighlightTile;

	public static Vector3Int ExplosiveShotTarget;
	public static bool ExplosiveShotPlanned;
	[HideInInspector]
	public bool WillFireExplosiveShot;

	private TilemapRegistry _tilemaps;
	private AudioSource _audio;

	private void Start()
	{
		_tilemaps = GetComponentInParent<TilemapRegistry>();
		_audio = GetComponent<AudioSource>();
	}

	private int GetTargetValue(Vector3Int targetPosition)
	{
		System.Diagnostics.Debug.Assert(_tilemaps.Enemies.HasTile(targetPosition));

		var enemyHealth = _tilemaps.Enemies.GetInstantiatedObject(targetPosition).GetComponent<HealthPool>();
		var enemyAttack = _tilemaps.Enemies.GetInstantiatedObject(targetPosition).GetComponent<EnemyAttack>();

		int value = enemyHealth.MaxHealth - enemyHealth.Health;
		foreach (Vector3Int direction in new[]
		{
			Vector3Int.up, Vector3Int.left, Vector3Int.down, Vector3Int.right
		})
		{
			Vector3Int adjacentPosition = targetPosition + direction;

			if (!_tilemaps.Buildings.HasTile(adjacentPosition)) continue;
			value += 1;

			if (
				_tilemaps.Buildings.GetInstantiatedObject(adjacentPosition).GetComponent<HealthPool>().Health
				> enemyAttack.AttackStrength
			) continue;
			value += 1;
		}

		return value;
	}

	private IEnumerator BuildingActionSequence()
	{
		if (WillFireExplosiveShot)
		{
			yield return StartCoroutine(AttackEnemy(ExplosiveShotTarget));
			yield break;
		}

		Vector3Int[] nearbyEnemyPositions = GetPositionsInRange().Where(_tilemaps.Enemies.HasTile).ToArray();

		if (nearbyEnemyPositions.Length <= 0) yield break;

		Vector3Int bestTarget = nearbyEnemyPositions[0];
		int targetValue = GetTargetValue(bestTarget);

		for (var i = 1; i < nearbyEnemyPositions.Length; i++)
		{
			int value = GetTargetValue(nearbyEnemyPositions[i]);
			if (value <= targetValue) continue;

			bestTarget = nearbyEnemyPositions[i];
			targetValue = value;
		}

		yield return StartCoroutine(AttackEnemy(bestTarget));
	}

	private bool CanHaveEnemy(Vector3Int target)
	{
		return _tilemaps.Ground.HasTile(target) || _tilemaps.OuterEdge.HasTile(target);
	}

	public IEnumerable<Vector3Int> GetPositionsInRange(
		Vector3Int? source = null,
		Predicate<Vector3Int> isValidTarget = null
	)
	{
		Vector3Int turretPosition = source ?? _tilemaps.Buildings.WorldToCell(transform.position);
		isValidTarget = isValidTarget ?? CanHaveEnemy;
		var locationsInRange = new HashSet<Vector3Int>();

		for (int x = -Range; x <= Range; x++)
		{
			for (int y = -Range; y <= Range; y++)
			{
				if (Mathf.Abs(x) + Mathf.Abs(y) > Range) continue;
				Vector3Int target = turretPosition + new Vector3Int(x, y, 0);
				if (!isValidTarget(target)) continue;

				locationsInRange.Add(target);
			}
		}

		return locationsInRange;
	}

	public void OnBuildingAction()
	{
		EndTurnManager.actions.Enqueue(BuildingActionSequence());
	}

	private IEnumerator AttackEnemy(Vector3Int targetPosition)
	{
		Transform projectileTransform = Instantiate(ProjectilePrefab).transform;
		projectileTransform.position += Vector3.back;

		_audio.pitch = Random.Range(MinPitch, MaxPitch);
		_audio.volume = OptionsMenu.GetEffectsVolume();
		_audio.Play();

		_tilemaps.Highlights.SetTile(_tilemaps.Buildings.WorldToCell(transform.position), HighlightTile);
		_tilemaps.Highlights.SetTile(targetPosition, HighlightTile);
		if (WillFireExplosiveShot)
		{
			for (int dx = -1; dx <= 1; dx++)
			{
				for (int dy = -1; dy <= 1; dy++)
				{
					_tilemaps.Highlights.SetTile(
						targetPosition + new Vector3Int(dx, dy, 0),
						HighlightTile
					);
				}
			}
		}

		yield return new WaitForSeconds(WarmUpInterval);

		Vector3 startPosition =
			_tilemaps.Buildings.GetCellCenterWorld(_tilemaps.Buildings.WorldToCell(transform.position));
		Vector3 enemyPosition = _tilemaps.Enemies.GetCellCenterWorld(targetPosition);

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

		while (Time.time - startTime < ProjectileLifetime)
		{
			projectileTransform.position =
				Vector3.Lerp(
					startPosition, enemyPosition,
					(Time.time - startTime) / ProjectileLifetime
				) + Vector3.back;
			yield return null;
		}

		Destroy(projectileTransform.gameObject);

		if (WillFireExplosiveShot)
		{
			WillFireExplosiveShot = false;
			ExplosiveShotPlanned = false;

			for (int dx = -1; dx <= 1; dx++)
			{
				for (int dy = -1; dy <= 1; dy++)
				{
					Vector3Int target = targetPosition + new Vector3Int(dx, dy, -1);
					if (_tilemaps.Buildings.HasTile(target))
					{
						_tilemaps.Buildings
							.GetInstantiatedObject(target)
							.GetComponent<HealthPool>()
							.Damage(ExplosiveAttackStrength);
					}

					if (_tilemaps.Enemies.HasTile(target))
					{
						_tilemaps.Enemies
							.GetInstantiatedObject(target)
							.GetComponent<HealthPool>()
							.Damage(ExplosiveAttackStrength);
					}
				}
			}
		}
		else
		{
			var enemyHealth = _tilemaps.Enemies.GetInstantiatedObject(targetPosition).GetComponent<HealthPool>();
			enemyHealth.Damage(AttackStrength);
		}

		_tilemaps.Highlights.ClearAllTiles();
		yield return new WaitForSeconds(AftermathInterval);
	}
}