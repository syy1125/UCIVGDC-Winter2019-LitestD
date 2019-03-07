using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
	public static GameManager Instance;

	[Header("Managers")]
	public ResourceManager ResourceManager;
	public EnemyManager EnemyManager;
	[FormerlySerializedAs("ConstructionManager")]
	public PlanConstructionManager PlanConstructionManager;
	public ConstructionQueueManager ConstructionQueueManager;
	public TileSelectionManager TileSelectionManager;
	public EndTurnManager EndTurnManager;

	[Header("Status")]
	public GameObject StatusBar;
	public TextMeshProUGUI StatusText;

	[Header("Events")]
	public GameEvent[] RaiseOnStart;

	private bool _canEnterSelectionMode = false;

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
		StatusBar.SetActive(false);

		foreach (GameEvent gameEvent in RaiseOnStart)
		{
			gameEvent.Raise();
		}
	}

	private void Update()
	{
		if (_canEnterSelectionMode && (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1)))
		{
			EnterSelectionMode();
		}
	}

	public void EnterSelectionMode()
	{
		_canEnterSelectionMode = true;

		PlanConstructionManager.SelectBuildTile(null);
		ConstructionQueueManager.SelectIndex(-1);
		TileSelectionManager.SetSelection(null);
		ResourceManager.SetOverlayVisible(false);
		EventSystem.current.SetSelectedGameObject(null);

		PlanConstructionManager.enabled = true;
		ConstructionQueueManager.enabled = true;
		TileSelectionManager.enabled = true;
		EndTurnManager.enabled = true;
		ResourceManager.enabled = true;

		StatusBar.SetActive(false);
	}

	public void EnterSpectatorMode()
	{
		_canEnterSelectionMode = false;
		DisableOtherManagers(null);
	}

	public void SetStatusText(string status)
	{
		StatusBar.SetActive(true);
		StatusText.text = status + "\n[ESC] or [RMB] to cancel";
	}

	public void DisableOtherManagers(MonoBehaviour active)
	{
		foreach (MonoBehaviour manager in new MonoBehaviour[]
			{PlanConstructionManager, ConstructionQueueManager, TileSelectionManager, ResourceManager, EndTurnManager})
		{
			manager.enabled = manager == active;
		}
	}

	private void OnDestroy()
	{
		Instance = null;
	}
}