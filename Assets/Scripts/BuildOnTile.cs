using UnityEngine;
using UnityEngine.Tilemaps;

public class BuildOnTile : MonoBehaviour
{
	public Tilemap Map;

	private Camera _mainCamera;
	private Plane _zPlane;

	private void Start()
	{
		_mainCamera = Camera.main;
		_zPlane = new Plane(Vector3.forward, 0);
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
			Debug.Log(mouseRay.GetPoint(10 * mouseRay.direction.magnitude / mouseRay.direction.z));
			Debug.Log(tilePosition);
		}
	}
}