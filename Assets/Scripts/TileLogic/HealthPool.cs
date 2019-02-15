using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class HealthPool : MonoBehaviour
{
	public int MaxHealth = 10;
	private int _health;

	public GameObject HealthbarPrefab;
	private Image _healthbarImage;

	[Header("Events")]
	public UnityEvent BeforeDeath;

	private Tilemap _tilemap;

	private void Start()
	{
		_health = MaxHealth;
		_healthbarImage = null;
		_tilemap = GetComponentInParent<Tilemap>();
	}

	public void Damage(int amount)
	{
		_health -= amount;

		if (ReferenceEquals(_healthbarImage, null))
		{
			CreateHealthbarImage();
		}

		_healthbarImage.fillAmount = (float) _health / MaxHealth;

		if (_health <= 0)
		{
			BeforeDeath.Invoke();
			_tilemap.SetTile(_tilemap.WorldToCell(transform.position), null);
		}
	}

	private void CreateHealthbarImage()
	{
		GameObject healthbarParent = GameObject.FindWithTag("HealthbarParent");
		GameObject healthbar = Instantiate(
			HealthbarPrefab,
			healthbarParent.transform
		);
		var mainCanvas = GameObject.FindWithTag("MainCanvas").GetComponent<Canvas>();
		Vector2 healthbarPosition = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 0.8f)
		                            / new Vector2(Screen.width, Screen.height)
		                            * mainCanvas.pixelRect.size;
		healthbar.transform.position = healthbarPosition;

		_healthbarImage = healthbar.transform.GetChild(0).GetComponent<Image>();
	}

	private void OnDestroy()
	{
		if (!ReferenceEquals(_healthbarImage, null))
		{
			Destroy(_healthbarImage.transform.parent.gameObject);
		}
	}
}