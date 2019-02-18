using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MessagePanel : MonoBehaviour
{
    public MessageEvent messageEvent;

    [Header("Panel Components")]
    public TextMeshProUGUI titleField;
    public TextMeshProUGUI textField;
    public Button continueButton;

    [Header("Items to Disable")]
    public Button endTurnButton;

    private Queue<Message> messages = new Queue<Message>();
    private Animator anim;
    private string visible = "Visible";

    private void Awake()
    {
        anim = GetComponent<Animator>();    
        continueButton.onClick.AddListener(Continue);
        messageEvent.Register(this);
    }

    public void EnqueueMessage(Message message)
    {
        messages.Enqueue(message);

        if (messages.Count == 1 && !anim.GetBool(visible))
        {
            Continue();
        }
    }

    private void Continue()
    {
        if (messages.Count == 0)
        {
            HidePopup();
        }
        else
        {
            ShowPopup();
        }
    }

    private void ShowPopup()
    {
        Message message = messages.Dequeue();
        titleField.text = message.title;
        textField.text = message.text;
        anim.SetBool(visible, true);
        endTurnButton.interactable = false;
    }

    private void HidePopup()
    {
        anim.SetBool(visible, false);
        endTurnButton.interactable = true;
    }
}
