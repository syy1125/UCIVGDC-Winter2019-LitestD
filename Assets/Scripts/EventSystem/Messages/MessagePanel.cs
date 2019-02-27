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

    private RectTransform rt;
    private Vector2 defaultPivotAndAnchors;
    private Vector2 defaultAnchoredPos;
    private float margin = 10f;
    private float titleHeight = 20f;

	private void Awake()
	{
		anim = GetComponent<Animator>();
		messageEvent.Register(this);

        rt = GetComponent<RectTransform>();
        defaultPivotAndAnchors = rt.pivot;
        defaultAnchoredPos = rt.anchoredPosition;
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
        SetPosition(message.position);

		anim.SetBool(visible, true);
        SetButtonsInteractable(false);
	}

	private void HidePopup()
	{
		anim.SetBool(visible, false);
        SetButtonsInteractable(true);
	}

    private void SetPosition(Position position)
    {

        switch (position)
        {
            case Position.Default:
                SetRectTransform(defaultPivotAndAnchors.x, defaultPivotAndAnchors.y, defaultAnchoredPos.x, defaultAnchoredPos.y);
                break;
            case Position.Center:
                SetRectTransform(0.5f, 0.5f, 0, 0);
                break;
            case Position.TopLeft:
                SetRectTransform(0, 1, margin, -(margin + titleHeight));
                break;
            case Position.TopRight:
                SetRectTransform(1, 1, -margin, -(margin + titleHeight));
                break;
            case Position.BottomRight:
                SetRectTransform(1, 0, -margin, margin);
                break;
            case Position.BottomLeft:
                SetRectTransform(0, 0, margin, margin);
                break;
        }
    }

    private void SetRectTransform(float pivotX, float pivotY, float offsetX, float offsetY)
    {
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(pivotX, pivotY);
        rt.anchoredPosition = new Vector2(offsetX, offsetY);
    }

    private void SetButtonsInteractable(bool interactable)
    {
        foreach (Button button in buttonsToDisable)
        {
            button.interactable = interactable;
        }
    }
}