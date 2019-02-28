using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class HealthPool : MonoBehaviour
{
	public int MaxHealth = 10;
	[FormerlySerializedAs("_health")]
	[HideInInspector]
	public int Health;

	public GameObject HealthbarPrefab;
	private Image _healthbarImage;

	[Header("Events")]
	public UnityEvent BeforeDeath;

	private Tilemap _parentTilemap;
	private Tilemap ParentTilemap => _parentTilemap ? _parentTilemap : _parentTilemap = GetComponentInParent<Tilemap>();

	private void Awake()
	{
		Health = MaxHealth;
		_healthbarImage = null;
	}

	public void Damage(int amount)
	{
		Health -= amount;

		if (ReferenceEquals(_healthbarImage, null))
		{
			CreateHealthbarImage();
		}

		_healthbarImage.fillAmount = (float) Health / MaxHealth;

		if (Health <= 0)
		{
			BeforeDeath.Invoke();
			ParentTilemap.SetTile(ParentTilemap.WorldToCell(transform.position), null);
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