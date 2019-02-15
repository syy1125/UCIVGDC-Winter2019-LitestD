using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

public class BuildingHealth : MonoBehaviour
{
	public int MaxHealth = 10;
	private int _health;

	[Header("Events")]
	public UnityEvent BeforeDeath;

	private Tilemap _tilemap;

	private void Start()
	{
		_health = MaxHealth;
		_tilemap = GetComponentInParent<Tilemap>();
	}

	public void Damage(int amount)
	{
		_health -= amount;

		if (_health <= 0)
		{
			BeforeDeath.Invoke();
			_tilemap.SetTile(_tilemap.WorldToCell(transform.position), null);
		}
	}
}