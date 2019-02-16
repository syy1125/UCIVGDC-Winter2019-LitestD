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

	private void Start()
	{
		BuildingPreview.sprite = BuildingSprite;
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
	{}

	public void MoveLeft()
	{}

	public void MoveRight()
	{}

	public void MoveRightmost()
	{}

	public void Cancel()
	{
		GameManager.Instance.ConstructionManager.CancelBuildOrder(_queueIndex);
	}
}