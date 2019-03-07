using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowTutorialElements : MonoBehaviour
{
    public MessageInterface messageInterface;
    public TutorialArrowInterface tutorialArrowInterface;
    public GameEvent showTutorialEvent;

    public List<TutorialElement> tutorialElements = new List<TutorialElement>();

    private void Awake()
    {
        foreach (TutorialElement tutorialElement in tutorialElements)
        {
            tutorialElement.Setup(messageInterface, tutorialArrowInterface);
        }
    }
}
