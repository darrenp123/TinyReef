/*  
 *  AUTHOR: Jon Munro 
 *  CREATED: 23/02/2021 
 */

using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [SerializeField]
    InstructionsMenu instructionsMenuScript;

    public static bool isPaused;
    [SerializeField]
    GameObject pauseMenu, quitMenu, instructionsMenu;
    // Start is called before the first frame update

    void Start()
    {
        isPaused = false;
    }

    // Update is called once per frame
    void Update()
    {
        /*
        if(Input.GetButtonDown("Pause"))
        {
            if(isPaused)
            {
                CloseAllMenus();
                UnPauseGame();
            }
            else
            {
                PauseGame();
            }
        }
        */
    }

    void CloseAllMenus()
    {
        instructionsMenuScript.CloseAllMenus();
        CloseInstructionsMenu();
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0;
        pauseMenu.SetActive(true);
    }
    public void UnPauseGame()
    {
        if (quitMenu.activeInHierarchy == true)
        {
            CloseQuitOptions();
        }
        isPaused = false;
        Time.timeScale = 1;
        pauseMenu.SetActive(false);
    }

    public void OpenQuitOptions()
    {
        quitMenu.SetActive(true);
    }
    public void CloseQuitOptions()
    {
        quitMenu.SetActive(false);
    }

    public void OpenInstructionsMenu()
    {
        instructionsMenu.SetActive(true);
    }

    public void CloseInstructionsMenu()
    {
        instructionsMenu.SetActive(false);
    }
}
