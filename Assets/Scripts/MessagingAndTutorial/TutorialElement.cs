using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Conditional Elements/Tutorial Element")]
public class TutorialElement : ScriptableObject
{ 
    [Header("Showing")]
    public GameEvent showEvent;
    public Condition condition;

    [Header("Hiding")]
    public GameEvent hideEvent;
    public GameEvent triggerNextTutorial;

    [Header("Tutorial Message")]
    public Message message;

    [Header("Tutorial Arrow")]
    public Vector3 position = Vector3.zero;
    [Range(-180, 180)] public float rotation = 0;

    private bool canShow;
    private MessageInterface messageInterface;
    private TutorialArrowInterface tutorialArrowInterface;

    public void Setup(MessageInterface mi, TutorialArrowInterface tai)
    {
        message.ResetTimesShown();

        canShow = true;
        messageInterface = mi;
        tutorialArrowInterface = tai;

        showEvent.AddListener(Show, true);
    }

    public void Show()
    {
        if (canShow && condition.IsMet())
        {
            showEvent.RemoveListener(Show, true);
            hideEvent.AddListener(Hide);
            tutorialArrowInterface.Show(position, rotation);
            messageInterface.Enqueue(message);
            canShow = false;
        }
    }

    public void Hide()
    {
        if (!canShow)
        {
            tutorialArrowInterface.Hide();
            messageInterface.Continue();
            hideEvent.RemoveListener(Hide);
            triggerNextTutorial.Raise();
        }
    }
}
