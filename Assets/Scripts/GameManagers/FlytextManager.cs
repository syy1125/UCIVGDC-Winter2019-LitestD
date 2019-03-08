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

	public void SpawnFlytextScreenPosition(Vector2 position, string message)
	{
		StartCoroutine(DisplayFlytext(position, message));
	}

	private Vector2 WorldToScreenPosition(Vector2 worldPosition)
	{
		return _mainCamera.WorldToScreenPoint(worldPosition)
		       / new Vector2(Screen.width, Screen.height)
		       * Canvas.pixelRect.size;
	}

	public void SpawnFlytextWorldPosition(Vector2 position, string message)
	{
		SpawnFlytextScreenPosition(WorldToScreenPosition(position), message);
	}

	private IEnumerator DisplayFlytext(Vector2 position, string message)
	{
		Vector2 startPosition = position + new Vector2(0, -50);
		Vector2 endPosition = position + new Vector2(0, 50);

		GameObject flytextObject = Instantiate(FlytextPrefab, transform);
		var flytext = flytextObject.GetComponent<TextMeshProUGUI>();
		flytext.text = message;
		flytext.CrossFadeAlpha(1, FadeInTime, false);

		flytextObject.transform.position = startPosition;
		float startTime = Time.time;
		while (Time.time - startTime < FadeInTime)
		{
			flytextObject.transform.position = Vector2.Lerp(
				startPosition,
				position,
				(Time.time - startTime) / FadeInTime
			);
			yield return null;
		}

		flytextObject.transform.position = position;
		yield return new WaitForSeconds(StayTime);

		flytext.CrossFadeAlpha(0, FadeOutTime, false);
		startTime = Time.time;
		while (Time.time - startTime < FadeOutTime)
		{
			flytextObject.transform.position = Vector2.Lerp(
				position,
				endPosition,
				(Time.time - startTime) / FadeOutTime
			);
			yield return null;
		}

		Destroy(flytextObject);
	}
}