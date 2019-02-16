using System.Collections;
using System.Collections.Generic;
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
	private int _queueIndex;
	private Coroutine _moveCoroutine;
	private ConstructionManager _constructionManager;

	private void Start()
	{
		BuildingPreview.sprite = BuildingSprite;
		_constructionManager = GameManager.Instance.ConstructionManager;
	}

	public void SetQueueIndex(int index)
	{
		if (_queueIndex == index) return;

		_queueIndex = index;
		OrderNumberText.text = (_queueIndex + 1).ToString();

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
		var endMin = new Vector2((width + Spacing) * _queueIndex, startMin.y);
		var endMax = new Vector2(width + (width + Spacing) * _queueIndex, startMax.y);

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

	public void MoveLeftmost()
	{
		_constructionManager.MoveQueueItem(_queueIndex, 0);
	}

	public void MoveLeft()
	{
		_constructionManager.MoveQueueItem(_queueIndex, _queueIndex - 1);
	}

	public void MoveRight()
	{
		_constructionManager.MoveQueueItem(_queueIndex, _queueIndex + 1);
	}

	public void MoveRightmost()
	{
		_constructionManager.MoveQueueItem(_queueIndex, _constructionManager.QueueLength - 1);
	}

	public void Cancel()
	{
		_constructionManager.CancelBuildOrder(_queueIndex);
	}
}