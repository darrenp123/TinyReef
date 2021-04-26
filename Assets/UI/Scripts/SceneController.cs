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
        optionsManager = GameObject.FindGameObjectWithTag("OptionsManager").GetComponent<OptionsManager>();
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
        optionsManager.InitialiseCameras();
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
