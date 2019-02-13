using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class TileSelectionManager : MonoBehaviour
{
	public Tilemap GroundMap;
	public Tilemap BuildingMap;
	public Vector3Int? Selection { get; private set; }

	[Header("Rendering")]
	public GameEvent UpdateUIEvent;
	public TextMeshProUGUI BuildingNameText;
	public TextMeshProUGUI BuildingFlavourText;
	public string GroundName;
	[TextArea]
	public string GroundFlavourText;

	private static Plane _zPlane = new Plane(Vector3.forward, 0);
	private Camera _mainCamera;

	private void Start()
	{
		_mainCamera = Camera.main;
	}

	private void Update()
	{
		if (!Input.GetMouseButtonDown(0)) return;

		Ray mouseRay = _mainCamera.ScreenPointToRay(Input.mousePosition);
		if (!_zPlane.Raycast(mouseRay, out float distance))
		{
			Debug.LogError("Failed to raycast onto Z plane.");
			return;
		}

		Vector3Int tilePosition = GroundMap.WorldToCell(mouseRay.GetPoint(distance));
		if (!GroundMap.HasTile(tilePosition)) return;

		SetSelection(tilePosition);
	}

	public void SetSelection(Vector3Int? target)
	{
		if (Selection != null)
		{
			GroundMap.SetColor(Selection.Value, Color.white);
			GroundMap.SetTileFlags(Selection.Value, TileFlags.LockColor);
			BuildingMap.SetColor(Selection.Value, Color.white);
			BuildingMap.SetTileFlags(Selection.Value, TileFlags.LockColor);
		}

		Selection = target;

		if (Selection != null)
		{
			GroundMap.SetTileFlags(Selection.Value, TileFlags.None);
			GroundMap.SetColor(Selection.Value, Color.cyan);
			BuildingMap.SetTileFlags(Selection.Value, TileFlags.None);
			BuildingMap.SetColor(Selection.Value, Color.cyan);
		}

		GameManager.Instance.DisableOtherManagers(this);
		UpdateUIEvent.Raise();
	}

	public void Display()
	{
		if (Selection == null)
		{
			GetComponent<Image>().enabled = false;
			foreach (Transform child in transform)
			{
				child.gameObject.SetActive(false);
			}
		}
		else
		{
			Vector3Int selectedTile = Selection.Value;
			
			GetComponent<Image>().enabled = true;
			foreach (Transform child in transform)
			{
				child.gameObject.SetActive(true);
			}

			if (BuildingMap.HasTile(selectedTile))
			{
				var description =
					BuildingMap.GetInstantiatedObject(selectedTile).GetComponent<BuildingDescription>();

				BuildingNameText.text = description.Name;
				BuildingFlavourText.text = description.FlavourText;
			}
			else
			{
				BuildingNameText.text = GroundName;
				BuildingFlavourText.text = GroundFlavourText;
			}
		}
	}
}