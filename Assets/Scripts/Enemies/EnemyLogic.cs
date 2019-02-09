using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using ExplorationQueue = System.Collections.Generic.Queue<System.Tuple<UnityEngine.Vector3Int, int>>;
using Object = UnityEngine.Object;

[Serializable]
public struct TileValue
{
	public TileBase Tile;
	public int Value;
}

public class EnemyLogic : MonoBehaviour
{
	private Tilemap _groundMap;
	private Tilemap _outerEdgeMap;
	private Tilemap _enemyMap;

	[Header("Tiles")]
	public TileBase GroundTile; // Can only walk on ground tiles
	public TileBase EnemyTile; // Don't try to attack another enemy
	public TileValue[] TileValues;

	private void Start()
	{
		_groundMap = GetComponentInParent<TilemapRegistry>().Ground;
		_outerEdgeMap = GetComponent<TilemapRegistry>().OuterEdge;
		_enemyMap = GetComponentInParent<Tilemap>();
	}

	private Vector2 CalculateMove()
	{
		DateTime startTime = DateTime.Now;

		Vector3Int currentPosition = _enemyMap.WorldToCell(transform.position);

		var valueMap = new Dictionary<Vector3, int>();
		foreach (Vector3Int direction in new[] {Vector3Int.up, Vector3Int.left, Vector3Int.down, Vector3Int.right})
		{
			if (!IsValidTile(currentPosition + direction)) continue;

			valueMap.Add(currentPosition + direction, GetPathPreference(currentPosition + direction));
			Debug.Log($"Direction preference {direction}={valueMap[currentPosition + direction]}");
		}

		Debug.Log($"Pathfinding completed in {(DateTime.Now - startTime).Seconds} seconds.");
		return Vector2.zero;
	}

	private bool IsValidTile(Vector3Int position)
	{
		return _groundMap.HasTile(position) || _outerEdgeMap.HasTile(position);
	}

	// Calculate value for a tile. Higher values are more desirable.
	private int GetPathPreference(Vector3Int position)
	{
		if (_groundMap.GetTile(position) != GroundTile)
		{
			return GetTileValue(_groundMap.GetTile(position));
		}

		int preference = int.MinValue;
		var core = new HashSet<Vector3Int>(new[] {position});
		var boundary = new ExplorationQueue();
		EnqueueAdjacentTiles(boundary, position, 0);

		while (boundary.Count > 0)
		{
			(Vector3Int tilePosition, int baseValue) = boundary.Dequeue();

			if (!core.Add(tilePosition)) continue;
			if (!IsValidTile(tilePosition)) continue;
			if (_enemyMap.GetTile(tilePosition) == EnemyTile) continue;

			if (IsGround(tilePosition))
			{
				EnqueueAdjacentTiles(boundary, tilePosition, baseValue);
			}
			else
			{
				int tileValue = baseValue + GetTileValue(_groundMap.GetTile(tilePosition));
				if (tileValue > preference) preference = tileValue;
			}
		}

		return preference;
	}

	private bool IsGround(Vector3Int position)
	{
		return _groundMap.GetTile(position) == GroundTile || _outerEdgeMap.GetTile(position) == GroundTile;
	}

	private static void EnqueueAdjacentTiles(ExplorationQueue queue, Vector3Int position, int baseValue)
	{
		queue.Enqueue(new Tuple<Vector3Int, int>(position + Vector3Int.up, baseValue - 1));
		queue.Enqueue(new Tuple<Vector3Int, int>(position + Vector3Int.left, baseValue - 1));
		queue.Enqueue(new Tuple<Vector3Int, int>(position + Vector3Int.down, baseValue - 1));
		queue.Enqueue(new Tuple<Vector3Int, int>(position + Vector3Int.right, baseValue - 1));
	}

	private int GetTileValue(Object tile)
	{
		return (from tileValue in TileValues where tileValue.Tile == tile select tileValue.Value).FirstOrDefault();
	}
}