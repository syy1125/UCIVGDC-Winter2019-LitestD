using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Interfaces/Tutorial Arrow")]
public class TutorialArrowInterface : ScriptableObject
{
    private TutorialArrow tutorialArrow;

    public void SetTutorialArrow(TutorialArrow ta)
    {
        if (tutorialArrow == null)
        {
            tutorialArrow = ta;
        }
    }

    public void Show(Vector3 position, float rotation)
    {
        if (tutorialArrow != null)
        {
            tutorialArrow.Show(position, rotation);
        }
    }

    public void Hide()
    {
        if (tutorialArrow != null)
        {
            tutorialArrow.Hide();
        }
    }
}
