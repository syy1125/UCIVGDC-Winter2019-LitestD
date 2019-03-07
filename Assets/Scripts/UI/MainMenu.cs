using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public string newGameSceneName;

    public void PlayGame()
    {
        SceneManager.LoadScene(newGameSceneName);
    }


    public void QuitGame()
    {
        Application.Quit();
    }

}