using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
	public FlytextManager FlytextManager;

    [Header("Pausing")]
    public GameObject pauseMenu;

	[Header("Status")]
	public GameObject StatusBar;
	public TextMeshProUGUI StatusText;

    [Header("Events")]
    public GameEvent beginningOfTurnEvent;
    public GameEvent enterSelectionModeEvent;
	public GameEvent[] RaiseOnStart;

	private bool _canEnterSelectionMode = false;
    private bool inSelectionMode = false;
    private bool isPaused = false;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
            beginningOfTurnEvent.AddListener(EnterSelectionMode);
            pauseMenu.SetActive(false);
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
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else if (!inSelectionMode && _canEnterSelectionMode)
            {
                EnterSelectionMode();
            }
            else if (inSelectionMode)
            {
                PauseGame();
            }
        }
	}

    public void PauseGame()
    {
        isPaused = true;
        pauseMenu.SetActive(true);
    }

    public void ResumeGame()
    {
        isPaused = false;
        pauseMenu.SetActive(false);
    }

    public void QuitToMainMenu()
    {
        SceneManager.LoadScene(0);
    }

	public void EnterSelectionMode()
	{
		_canEnterSelectionMode = true;
        inSelectionMode = true;

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

        enterSelectionModeEvent.Raise();
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
        inSelectionMode = false;
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