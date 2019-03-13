using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

[Serializable]
public struct TileCondition
{
	public string Name;
	public Sprite Sprite;
}

[CreateAssetMenu(menuName = "Conditional Tile")]
public class ConditionalTile : TileBase
{
	public TileCondition[] TileConditions;
	[HideInInspector]
	public string State;

	public GameObject LogicObject;

	private Dictionary<string, Sprite> _rules = new Dictionary<string, Sprite>();

	public override bool StartUp(Vector3Int position, ITilemap tilemap, GameObject go)
	{
		_rules = TileConditions.ToDictionary(
			tileCondition => tileCondition.Name,
			tileCondition => tileCondition.Sprite
		);

		if (go == null) return true;

		Transform t = go.transform;
		t.position = tilemap.GetComponent<Tilemap>().GetCellCenterWorld(position);
		t.rotation = Quaternion.identity;

		return true;
	}

	public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
	{
		base.GetTileData(position, tilemap, ref tileData);
		tileData.sprite = _rules.TryGetValue(State, out Sprite sprite) ? sprite : TileConditions[0].Sprite;
		tileData.gameObject = LogicObject;
	}
}