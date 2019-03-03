using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
	public static GameManager Instance;

	public ResourceManager ResourceManager;
	public EnemyManager EnemyManager;
	[FormerlySerializedAs("ConstructionManager")]
	public PlanConstructionManager PlanConstructionManager;
	public ConstructionQueueManager ConstructionQueueManager;
	public TileSelectionManager TileSelectionManager;
	public EndTurnManager EndTurnManager;

	public GameEvent[] RaiseOnStart;

    private bool canEnterSelectionMode = false;

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
		if (canEnterSelectionMode && (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1)))
		{
			ResetSelectionMode();
		}
	}

	public void ResetSelectionMode()
	{
		PlanConstructionManager.SelectBuildTile(null);
		ConstructionQueueManager.SelectIndex(-1);
		TileSelectionManager.SetSelection(null);
		EventSystem.current.SetSelectedGameObject(null);

        EnterSelectionMode();
    }

    public void EnterSelectionMode()
    {
        canEnterSelectionMode = true;

        PlanConstructionManager.enabled = true;
        ConstructionQueueManager.enabled = true;
        TileSelectionManager.enabled = true;
        EndTurnManager.enabled = true;
    }

    public void EnterSpectatorMode()
    {
        canEnterSelectionMode = false;
        DisableOtherManagers(null);
    }

	public void DisableOtherManagers(MonoBehaviour active)
	{
		foreach (MonoBehaviour manager in new MonoBehaviour[]
			{PlanConstructionManager, ConstructionQueueManager, TileSelectionManager, EndTurnManager})
		{
			manager.enabled = manager == active;
		}
	}

	private void OnDestroy()
	{
		Instance = null;
	}
}