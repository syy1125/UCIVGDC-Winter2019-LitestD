using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConstructionQueueItemPanel : MonoBehaviour
{
	public TextMeshProUGUI OrderNumberText;
	public Image BuildingPreview;

	[HideInInspector]
	public Sprite BuildingSprite;

	[Header("Rendering")]
	public AnimationCurve TransitionCurve;
	public float TransitionDuration = 0.5f;
	public float Spacing;
	public Button ShiftLeftmostButton;
	public Button ShiftLeftButton;
	public Button ShiftRightButton;
	public Button ShiftRightmostButton;
	public Button CancelButton;

	private Button _selfButton;
	public Button SelfButton => _selfButton ? _selfButton : _selfButton = GetComponent<Button>();

	private int _queueIndex;
	private bool _buttonsInteractable;
	private Coroutine _moveCoroutine;
	private static ConstructionQueueManager QueueManager => GameManager.Instance.ConstructionQueueManager;

	private void Start()
	{
		BuildingPreview.sprite = BuildingSprite;
	}

	public void SetQueueIndex(int index)
	{
		if (_queueIndex == index) return;

		_queueIndex = index;
		OrderNumberText.text = (_queueIndex + 1).ToString();

		DisplayButtons();

		if (_moveCoroutine != null)
		{
			StopCoroutine(_moveCoroutine);
		}

		_moveCoroutine = StartCoroutine(MoveToPosition());
	}

	private IEnumerator MoveToPosition()
	{
		var t = GetComponent<RectTransform>();
		Vector2 startMin = t.offsetMin;
		Vector2 startMax = t.offsetMax;
		float width = t.rect.width;
		var endMin = new Vector2(Spacing + (width + Spacing) * _queueIndex, startMin.y);
		var endMax = new Vector2((width + Spacing) * (_queueIndex + 1), startMax.y);

		float startTime = Time.time;
		while (Time.time - startTime < TransitionDuration)
		{
			float progress = TransitionCurve.Evaluate((Time.time - startTime) / TransitionDuration);
			t.offsetMin = startMin * (1 - progress) + endMin * progress;
			t.offsetMax = startMax * (1 - progress) + endMax * progress;
			yield return null;
		}

		t.offsetMin = endMin;
		t.offsetMax = endMax;
		_moveCoroutine = null;
	}

	public void SetButtonInteractable(bool interactable)
	{
		_buttonsInteractable = interactable;

		DisplayButtons();
	}

	private void DisplayButtons()
	{
		SelfButton.interactable = _buttonsInteractable;
		CancelButton.interactable = _buttonsInteractable;

		ShiftLeftmostButton.interactable = _buttonsInteractable && _queueIndex > 0;
		ShiftLeftButton.interactable = _buttonsInteractable && _queueIndex > 0;
		ShiftRightButton.interactable = _buttonsInteractable && _queueIndex < QueueManager.QueueLength - 1;
		ShiftRightmostButton.interactable = _buttonsInteractable && _queueIndex < QueueManager.QueueLength - 1;
	}

	public void MoveLeftmost()
	{
		QueueManager.MoveQueueItem(_queueIndex, 0);
	}

	public void MoveLeft()
	{
		QueueManager.MoveQueueItem(_queueIndex, _queueIndex - 1);
	}

	public void MoveRight()
	{
		QueueManager.MoveQueueItem(_queueIndex, _queueIndex + 1);
	}

	public void MoveRightmost()
	{
		QueueManager.MoveQueueItem(_queueIndex, QueueManager.QueueLength - 1);
	}

	public void SelectThis()
	{
		QueueManager.SelectIndex(_queueIndex);
	}

	public void Cancel()
	{
		QueueManager.CancelBuildOrder(_queueIndex);
	}
}