using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

[Serializable]
public struct TileAttraction
{
	public TileBase Tile;
	public float Attraction;
}

[CreateAssetMenu(menuName = "Config/Enemy Pathfinding")]
public class EnemyPathfinding : ScriptableObject
{
	public TileAttraction[] TileAttractions;
	public AnimationCurve FalloffCurve;
	public int MaxRadius = 16;

	// An attraction map works similar to potential energy, except negative.
	// The higher the attraction value, the more preferable that tile is - either for attacking or for moving into.
	public Dictionary<Vector3Int, float> MakeAttractionMap(Tilemap buildingMap, IEnumerable<Vector3Int> targetPositions)
	{
		DateTime startTime = DateTime.Now;
		Dictionary<TileBase, float> tileAttractionValues = TileAttractions.ToDictionary(
			tileAttraction => tileAttraction.Tile,
			tileAttraction => tileAttraction.Attraction
		);

		var attractionMap = new Dictionary<Vector3Int, float>();

		foreach (Vector3Int targetPosition in targetPositions)
		{
			float attraction = 0;
			foreach (Vector3Int buildingPosition in buildingMap.cellBounds.allPositionsWithin)
			{
				if (!buildingMap.HasTile(buildingPosition)) continue;
				TileBase buildingTile = buildingMap.GetTile(buildingPosition);

				int distance = Mathf.Abs(buildingPosition.x - targetPosition.x)
				               + Mathf.Abs(buildingPosition.y - targetPosition.y);
				float baseAttraction;
				baseAttraction = tileAttractionValues.TryGetValue(buildingTile, out baseAttraction)
					? baseAttraction
					: 0;
				float weightedAttraction = baseAttraction
				                           * FalloffCurve.Evaluate((float) distance / MaxRadius);
				attraction += weightedAttraction;
			}

			attractionMap.Add(targetPosition, attraction);
		}

		Debug.Log($"Attraction map made in {(DateTime.Now - startTime).TotalMilliseconds} milliseconds");

		return attractionMap;
	}
}