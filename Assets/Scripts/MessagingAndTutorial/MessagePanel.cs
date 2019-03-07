using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Animator))]
public class MessagePanel : MonoBehaviour
{
    [Header("Components")]
    public TextMeshProUGUI title;
    public TextMeshProUGUI text;
    public Button continueButton;
    public MessageInterface messageInterface;

    [Header("Style")]
    public float margin = 30f;
    public float titleHeight = 20f;

    private Queue<Message> messages = new Queue<Message>();
    private Message currentMessage;
    private bool visible;
    private Animator anim;
    private RectTransform rt;
    private Vector2 defaultPivotAndAnchors;
    private Vector2 defaultAnchoredPos;

    private void Awake()
    {
        continueButton.onClick.AddListener(Continue);
        messageInterface.SetMessagePanel(this);

        visible = false;
        anim = GetComponent<Animator>();
        rt = GetComponent<RectTransform>();
        defaultPivotAndAnchors = rt.pivot;
        defaultAnchoredPos = rt.anchoredPosition;
    }

    public void Enqueue(Message message)
    {
        messages.Enqueue(message);
        if (messages.Count == 1 && !visible)
            Continue();
    }

    public void Continue()
    {
        if (messages.Count == 0)
        {
            Hide();
        }
        else
        {
            currentMessage = messages.Dequeue();

            text.text = currentMessage.text;
            title.text = currentMessage.title;
            SetPosition(currentMessage.position);
            continueButton.gameObject.SetActive(currentMessage.showContinueButton);
            // Add in functionality to disableUI

            Show();
        }

        messageInterface.ContinueMade?.Invoke();
    }

    public void Clear()
    {
        messages.Clear();
        Hide();
    }

    public Message CurrentMessage()
    {
        return currentMessage;
    }

    private void Show()
    {
        if (!visible)
        {
            visible = true;
            anim.SetBool("visible", true);
        }
    }

    private void Hide()
    {
        if (visible)
        {
            visible = false;
            anim.SetBool("visible", false);
        }
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
}
