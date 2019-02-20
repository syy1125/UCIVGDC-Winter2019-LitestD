using UnityEngine;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour
{
	public static GameManager Instance;

	public ResourceManager ResourceManager;
	public EnemyManager EnemyManager;
	[FormerlySerializedAs("ConstructionManager")]
	public PlanConstructionManager PlanConstructionManager;
	public ConstructionQueueManager ConstructionQueueManager;
	public TileSelectionManager TileSelectionManager;

	public GameEvent[] RaiseOnStart;

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

	private void Start()
	{
		foreach (GameEvent gameEvent in RaiseOnStart)
		{
			gameEvent.Raise();
		}
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			PlanConstructionManager.SelectBuildTile(null);
			ConstructionQueueManager.SelectIndex(-1);
			TileSelectionManager.SetSelection(null);

			PlanConstructionManager.enabled = true;
			ConstructionQueueManager.enabled = true;
			TileSelectionManager.enabled = true;
		}
	}

	public void DisableOtherManagers(MonoBehaviour active)
	{
		foreach (MonoBehaviour manager in new MonoBehaviour[]
			{PlanConstructionManager, ConstructionQueueManager, TileSelectionManager})
		{
			manager.enabled = manager == active;
		}
	}

	private void OnDestroy()
	{
		Instance = null;
	}
}