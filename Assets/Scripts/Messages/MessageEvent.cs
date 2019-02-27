﻿using UnityEngine;

[CreateAssetMenu(menuName = "Events/MessageEvent")]
public class MessageEvent : ScriptableObject
{
	private MessagePanel messagePanel;

	public void Display(Message message)
	{
		if (messagePanel != null)
		{
			messagePanel.EnqueueMessage(message);
		}
	}

	public void Register(MessagePanel panel)
	{
		messagePanel = panel;
	}
}