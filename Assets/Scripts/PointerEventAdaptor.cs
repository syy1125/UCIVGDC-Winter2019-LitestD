using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[Serializable]
public class PointerEvent : UnityEvent<PointerEventData>
{}

public class PointerEventAdaptor : MonoBehaviour,
	IPointerClickHandler,
	IPointerUpHandler,
	IPointerDownHandler,
	IPointerEnterHandler,
	IPointerExitHandler
{
	public PointerEvent PointerClick;
	public PointerEvent PointerUp;
	public PointerEvent PointerDown;
	public PointerEvent PointerEnter;
	public PointerEvent PointerExit;
	
	public void OnPointerClick(PointerEventData eventData)
	{
		PointerClick.Invoke(eventData);
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		PointerUp.Invoke(eventData);
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		PointerDown.Invoke(eventData);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		PointerEnter.Invoke(eventData);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		PointerExit.Invoke(eventData);
	}
}