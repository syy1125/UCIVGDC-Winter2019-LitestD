using System.Collections;
using TMPro;
using UnityEngine;

public class FlytextManager : MonoBehaviour
{
	public Canvas Canvas;
	public GameObject FlytextPrefab;
	public float FadeInTime = 1;
	public float StayTime = 1;
	public float FadeOutTime = 1;

	private Camera _mainCamera;

	private void Awake()
	{
		_mainCamera = Camera.main;
	}

	public void SpawnFlytextScreenPosition(
		Vector2 position,
		string message,
		float fadeInTime,
		float stayTime,
		float fadeOutTime
	)
	{
		StartCoroutine(DisplayFlytext(position, message, fadeInTime, stayTime, fadeOutTime));
	}

	public void SpawnFlytextScreenPosition(Vector2 position, string message)
	{
		SpawnFlytextScreenPosition(position, message, FadeInTime, StayTime, FadeOutTime);
	}

	private Vector2 WorldToScreenPosition(Vector2 worldPosition)
	{
		return _mainCamera.WorldToScreenPoint(worldPosition)
		       / new Vector2(Screen.width, Screen.height)
		       * Canvas.pixelRect.size;
	}

	public void SpawnFlytextWorldPosition(
		Vector2 position,
		string message,
		float fadeInTime,
		float stayTime,
		float fadeOutTime
	)
	{
		SpawnFlytextScreenPosition(WorldToScreenPosition(position), message, fadeInTime, stayTime, fadeOutTime);
	}

	public void SpawnFlytextWorldPosition(Vector2 position, string message)
	{
		SpawnFlytextWorldPosition(position, message, FadeInTime, StayTime, FadeOutTime);
	}

	private IEnumerator DisplayFlytext(
		Vector2 position,
		string message,
		float fadeInTime,
		float stayTime,
		float fadeOutTime
	)
	{
		Vector2 startPosition = position + new Vector2(0, -50);
		Vector2 endPosition = position + new Vector2(0, 50);

		GameObject flytextObject = Instantiate(FlytextPrefab, transform);
		var flytext = flytextObject.GetComponent<TextMeshProUGUI>();
		flytext.text = message;
		flytext.CrossFadeAlpha(1, fadeInTime, false);

		flytextObject.transform.position = startPosition;
		float startTime = Time.time;
		while (Time.time - startTime < fadeInTime)
		{
			flytextObject.transform.position = Vector2.Lerp(
				startPosition,
				position,
				(Time.time - startTime) / fadeInTime
			);
			yield return null;
		}

		flytextObject.transform.position = position;
		yield return new WaitForSeconds(stayTime);

		flytext.CrossFadeAlpha(0, fadeOutTime, false);
		startTime = Time.time;
		while (Time.time - startTime < fadeOutTime)
		{
			flytextObject.transform.position = Vector2.Lerp(
				position,
				endPosition,
				(Time.time - startTime) / fadeOutTime
			);
			yield return null;
		}

		Destroy(flytextObject);
	}
}