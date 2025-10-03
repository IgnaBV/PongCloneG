using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public SceneTransition sceneTransition;

    public void PlayGame()
    {
        sceneTransition.LoadScene("SampleScene");
    }

    public void OpenOptions()
    {
        Debug.Log("Abrir opciones...");
    }

    public void QuitGame()
    {
        Debug.Log("Saliste del juego...");
        Application.Quit();
    }
}


