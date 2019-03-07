using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[RequireComponent(typeof(Animator))]
public class TutorialArrow : MonoBehaviour
{
    public TutorialArrowInterface tutorialArrowInterface;

    private bool visible;
    private Animator anim;
    private RectTransform rt;

    private void Awake()
    {
        visible = false;
        tutorialArrowInterface.SetTutorialArrow(this);
        anim = GetComponent<Animator>();
        rt = GetComponent<RectTransform>();
    }

    public void Show(Vector3 position, float rotation)
    {
        rt.anchoredPosition = position;
        rt.rotation = Quaternion.Euler(new Vector3(0f, 0f, rotation));

        if (!visible)
        {
            visible = true;
            anim.SetBool("visible", true);
        }
    }

    public void Hide()
    {
        if (visible)
        {
            visible = false;
            anim.SetBool("visible", false);
        }
    }
}
