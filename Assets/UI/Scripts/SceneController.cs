/*  
 *  AUTHOR: Jon Munro 
 *  CREATED: 23/02/2021 
 */

using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{

    OptionsManager optionsManager;
    [SerializeField]
    MainMenu mainMenu;

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

    public void LoadLevelMenu() {
        SceneManager.LoadSceneAsync(mainMenu.ReturnCurrentLevel());
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
