using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MessagePanel : MonoBehaviour
{
	public MessageEvent messageEvent;

	[Header("Panel Components")]
	public TextMeshProUGUI titleField;
	public TextMeshProUGUI textField;

	[Header("Buttons to Disable")]
	public List<Button> buttonsToDisable = new List<Button>();

	private Queue<Message> messages = new Queue<Message>();
	private Animator anim;
	private string visible = "Visible";

	private void Awake()
	{
		anim = GetComponent<Animator>();
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

	public void Continue()
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
        SetButtonsInteractable(false);
	}

	private void HidePopup()
	{
		anim.SetBool(visible, false);
        SetButtonsInteractable(true);
	}

    private void SetButtonsInteractable(bool interactable)
    {
        foreach (Button button in buttonsToDisable)
        {
            button.interactable = interactable;
        }
    }
}