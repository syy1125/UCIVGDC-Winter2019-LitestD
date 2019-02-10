using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DisplayPopup : MonoBehaviour
{
    public TextMeshProUGUI textField;
    public Button continueButton;

    private Queue<string> messages = new Queue<string>();
    private Animator anim;
    private string visible = "Visible"; // So I don't mess up spelling in multiple places

    private void Awake()
    {
        anim = GetComponent<Animator>();    
        continueButton.onClick.AddListener(ShowNextMessage); // Would it be better to do this in the Unity Editor?
    }

    public void EnqueueMessage(string message)
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
            textField.text = messages.Dequeue();
            anim.SetBool(visible, true); // Maybe I should check if it equals false first
        }
    }
}
