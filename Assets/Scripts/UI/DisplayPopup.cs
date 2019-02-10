using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DisplayPopup : MonoBehaviour
{
    public TextMeshProUGUI titleField;
    public TextMeshProUGUI textField;
    public Button continueButton;

    private Queue<Message> messages = new Queue<Message>();
    private Animator anim;
    private string visible = "Visible"; // So I don't mess up spelling in multiple places

    private void Awake()
    {
        anim = GetComponent<Animator>();    
        continueButton.onClick.AddListener(ShowNextMessage); // Would it be better to do this in the Unity Editor?
    }

    public void EnqueueMessage(Message message)
    {
        messages.Enqueue(message);

        if (messages.Count == 1 && !anim.GetBool(visible))
        {
            ShowNextMessage();
        }
    }

    private void ShowNextMessage()
    {
        if (messages.Count == 0)
        {
            anim.SetBool(visible, false);
        }
        else
        {
            Message message = messages.Dequeue();
            titleField.text = message.title;
            textField.text = message.text;
            anim.SetBool(visible, true); // Maybe I should check if it equals false first
        }
    }
}
