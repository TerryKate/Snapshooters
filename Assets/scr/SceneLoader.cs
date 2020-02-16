using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public Image loadingBar;

    private void Start()
    {
        loadingBar.fillAmount = 0;

        // start async;
        StartCoroutine(LoadAsyncOperation());
    }

    IEnumerator LoadAsyncOperation()
    {
        //
        yield return new WaitForSeconds(0.5f);

        // create operation;
        AsyncOperation game = SceneManager.LoadSceneAsync(2);
        
        // update progress;
        while(game.progress < 1)
        {
            loadingBar.fillAmount = game.progress;
            yield return new WaitForEndOfFrame();
        }

        // load scene;
        yield return new WaitForSeconds(2);


    }
}
