using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Main Menus (Defaults)")]
    public GameObject main;
    public GameObject options;

    private Button[] buttons;

    private void Start()
    {
        buttons = this.GetComponentsInChildren<Button>();

        main.SetActive(true);
        options.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown("escape"))
        {
            QuitGame();
        }
    }

    public void NewGame()
    {
        // TODO : Maybe there's a better way than just using index;
        SceneManager.LoadScene(2);
    }

    public void ShowOptions(bool toogle)
    {
        main.SetActive(!toogle);
        options.SetActive(toogle);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        // Application.Quit() does not work in the editor so
        UnityEditor.EditorApplication.isPlaying = false;
#else
         Application.Quit();
#endif
    }
}
