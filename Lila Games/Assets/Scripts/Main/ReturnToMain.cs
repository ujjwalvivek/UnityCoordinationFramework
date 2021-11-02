using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnToMain : MonoBehaviour
{
    public KeyCode exitScene;

    public void Update()
    {
        ExitScene(0);
    }

    public void ExitScene(int _index)
    {
        if (Input.GetKeyDown(exitScene))
        {
            SceneManager.LoadSceneAsync(_index, LoadSceneMode.Single);
        }
    }
}
