using UnityEngine;
using UnityEngine.Tilemaps;

public class BuildOnTile : MonoBehaviour
{
	public Tilemap Map;
	
	private Camera _mainCamera;
	private static Plane _zPlane = new Plane(Vector3.forward, 0);
	
	private void Start()
	{
		_mainCamera = Camera.main;
	}

	private void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			Ray mouseRay = _mainCamera.ScreenPointToRay(Input.mousePosition);
			if (!_zPlane.Raycast(mouseRay, out float distance))
			{
				Debug.LogError("Failed to raycast onto Z plane.");
				return;
			}
			
			Vector3Int tilePosition = Map.WorldToCell(mouseRay.GetPoint(distance));
			Debug.Log(tilePosition);
			// TODO Implement placement logic
		}
	}
}