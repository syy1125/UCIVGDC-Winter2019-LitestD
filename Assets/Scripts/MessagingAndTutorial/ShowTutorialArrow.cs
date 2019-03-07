using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowTutorialArrow : MonoBehaviour
{
    public TutorialArrowInterface tutorialArrowInterface;
    public GameEvent gameEvent;

    [Header("Positioning")]
    public Vector3 position = Vector3.zero;
    [Range(-180, 180)] public float rotation = 0;

    private bool canShow;

    private void Awake()
    {
        canShow = true;
    }

    public void Show()
    {
        if (canShow)
        {
            gameEvent.AddListener(Hide);
            tutorialArrowInterface.Show(position, rotation);
            canShow = false;
        }
    }

    public void Hide()
    {
        if (!canShow)
        {
            tutorialArrowInterface.Hide();
            gameEvent.RemoveListener(Hide);
        }
    }
}
