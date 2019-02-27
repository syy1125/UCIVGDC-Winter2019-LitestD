using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{

    public Button newGameButton;

    public Button exitGameButton;

    public string newGameSceneName;

    public void PlayGame()
    {
        SceneManager.LoadScene(newGameSceneName);
    }


    public void QuitGame()
    {
        Debug.Log("QUIT!");
        Application.Quit();
    }

}