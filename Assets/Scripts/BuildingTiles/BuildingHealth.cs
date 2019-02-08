using UnityEngine;
using UnityEngine.Tilemaps;

public class BuildingHealth : MonoBehaviour
{
	public TileBase EmptyGroundTile;
	public int MaxHealth = 10;
	private int _health;

	private Tilemap _groundTilemap;

	private void Start()
	{
		_health = MaxHealth;
		_groundTilemap = GameObject.FindWithTag("GroundTilemap").GetComponent<Tilemap>();
	}

	public void Damage(int amount)
	{
		_health -= amount;

		if (_health <= 0)
		{
			_groundTilemap.SetTile(_groundTilemap.WorldToCell(transform.position), EmptyGroundTile);
		}
	}

	private void OnDestroy()
	{
		Debug.Log("Building Health Script Destroy");
	}
}
