using UnityEngine;

public class GameManager : MonoBehaviour
{
	public static GameManager Instance;

	public ResourceManager ResourceManager;
	public EnemyManager EnemyManager;
	public ConstructionManager ConstructionManager;
	public TileSelectionManager TileSelectionManager;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Debug.LogWarning("A duplicate instance of GameManager is attempting to initialize. Destroying it.");
			Destroy(gameObject);
		}
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			ConstructionManager.SelectBuildTile(null);
			TileSelectionManager.SetSelection(null);

			ConstructionManager.enabled = true;
			TileSelectionManager.enabled = true;
		}
	}

	public void DisableOtherManagers(MonoBehaviour active)
	{
		foreach (MonoBehaviour manager in new MonoBehaviour[] {ConstructionManager, TileSelectionManager})
		{
			manager.enabled = manager == active;
		}
	}

	private void OnDestroy()
	{
		Instance = null;
	}
}