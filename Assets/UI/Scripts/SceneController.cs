/*  
 *  AUTHOR: Jon Munro 
 *  CREATED: 23/02/2021 
 */

using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{

    OptionsManager optionsManager;

    private void Start()
    {
        optionsManager = FindObjectOfType<OptionsManager>();
        if(optionsManager) optionsManager.InitialiseCameras();
    }

    public void ResetLevel()
    {
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadSceneAsync(scene.name);
        Time.timeScale = 1;
    }

    public void LoadLevel(string level)
    {
        SceneManager.LoadSceneAsync(level);
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadSceneAsync("MainMenuScene");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
