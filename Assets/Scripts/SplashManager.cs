using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashManager : MonoBehaviour
{
    [SerializeField] private string targetNameScene;

    public void LoadMenu()
    {
        SceneManager.LoadScene(targetNameScene);
    }
}
