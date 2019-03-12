using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    public Toggle tutorialToggle;

    private string showTutorialString = "ShowTutorial";

    private void Awake()
    {
        tutorialToggle.onValueChanged.AddListener((value) => { ToggleTutorial(value); });
        if (PlayerPrefs.HasKey(showTutorialString))
        {
            tutorialToggle.isOn = PlayerPrefs.GetInt(showTutorialString) == 1;
        }
        else
        {
            ToggleTutorial(true);
        }
    }

    private void ToggleTutorial(bool value)
    {
        PlayerPrefs.SetInt(showTutorialString, value ? 1 : 0);
        tutorialToggle.isOn = value;
    }
}
