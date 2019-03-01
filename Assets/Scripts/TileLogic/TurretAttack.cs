using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TurretAttack : MonoBehaviour
{
	public int AttackStrength;
	public int Range;

	[Header("Effects and Timing")]
	public GameObject ProjectilePrefab;
	public float ProjectileLifetime;
	public float AftermathInterval;
	public float MinPitch;
	public float MaxPitch;

	private TilemapRegistry _tilemaps;
	private AudioSource _audio;

	private void Start()
	{
		_tilemaps = GetComponentInParent<TilemapRegistry>();
		_audio = GetComponent<AudioSource>();
	}

	private IEnumerator BuildingActionSequence()
	{
		Vector3Int[] nearbyEnemies = GetPositionsInRange().Where(_tilemaps.Enemies.HasTile).ToArray();

		if (nearbyEnemies.Length <= 0) yield break;
		yield return StartCoroutine(AttackEnemy(nearbyEnemies[Random.Range(0, nearbyEnemies.Length)]));
	}

	public IEnumerable<Vector3Int> GetPositionsInRange()
	{
		Vector3Int turretPosition = _tilemaps.Buildings.WorldToCell(transform.position);
		var locationsInRange = new HashSet<Vector3Int>();

		for (int x = -Range; x <= Range; x++)
		{
			for (int y = -Range; y <= Range; y++)
			{
				if (Mathf.Abs(x) + Mathf.Abs(y) > Range) continue;
				Vector3Int target = turretPosition + new Vector3Int(x, y, 0);
				if (!(_tilemaps.Ground.HasTile(target) || _tilemaps.OuterEdge.HasTile(target))) continue;

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
		var enemyHealth = _tilemaps.Enemies.GetInstantiatedObject(targetPosition).GetComponent<HealthPool>();
		Transform projectileTransform = Instantiate(ProjectilePrefab).transform;

		_audio.pitch = Random.Range(MinPitch, MaxPitch);
		_audio.Play();

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