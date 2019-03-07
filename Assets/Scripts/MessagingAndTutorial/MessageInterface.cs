using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Interfaces/Message")]
public class MessageInterface : ScriptableObject
{
    [HideInInspector] public Action ContinueMade;
    [HideInInspector] public Action PanelHidden;

    private MessagePanel messagePanel;

    public void SetMessagePanel(MessagePanel mp)
    {
        if (messagePanel == null)
        {
            messagePanel = mp;
        }
    }

    public void Enqueue(Message message)
    {
        if (messagePanel != null)
        {
            if (message.CanShow())
            {
                message.Shown();
                messagePanel.Enqueue(message);
            }
        }
    }

    public void Continue()
    {
        if (messagePanel != null)
        {
            messagePanel.Continue();
        }
    }

    public void Clear()
    {
        if (messagePanel != null)
        {
            messagePanel.Clear();
        }
    }

    public Message CurrentMessage()
    {
        if (messagePanel != null)
        {
            return messagePanel.CurrentMessage();
        }
        return null;
    }
}
