using UnityEngine;
using UnityEngine.Tilemaps;

public class TileSelectionManager : MonoBehaviour
{
	public Tilemap GroundMap;
	public Tilemap BuildingMap;
	public Vector3Int? Selection { get; private set; }

	public GameEvent UpdateUIEvent;

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
}