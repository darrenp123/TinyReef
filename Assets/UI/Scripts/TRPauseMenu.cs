/*  
 *  AUTHOR: Jon Munro 
 *  CREATED: 23/02/2021 
 */

using UnityEngine;
using UnityEngine.InputSystem;

public class TRPauseMenu : MonoBehaviour
{
    [SerializeField]
    InstructionsMenu instructionsMenuScript;
    [SerializeField]
    private InputAction pauseGame;

    public static bool isPaused;
    [SerializeField]
    GameObject pauseMenu, quitMenu, instructionsMenu, optionsMenu, factFile;

    private void OnEnable() {
        pauseGame.Enable();
    }

    private void OnDisable() {
        pauseGame.Disable();
    }

    private void Awake() {
        pauseGame.performed += _ => PauseUnpause();
    }

    // Start is called before the first frame update
    void Start()
    {
        isPaused = false;
    }

    public void OpenPauseMenu(InputAction.CallbackContext context) {
        if (context.performed) {
            if (isPaused) {
                CloseAllMenus();
                UnPauseGame();
            } else {
                PauseGame();
            }
        }
    }

    void CloseAllMenus()
    {
        instructionsMenuScript.CloseAllMenus();
        CloseInstructionsMenu();
    }

    public void PauseUnpause() {
        if (isPaused) {
            UnPauseGame();
        } else {
            PauseGame();
        }
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

    public void OpenOptionsMenu()
    {
        optionsMenu.SetActive(true);
    }
    public void CloseOptionsMenu()
    {
        optionsMenu.SetActive(false);
    }

    public void OpenFactFile()
    {
        factFile.SetActive(true);
    }

    public void CloseFactFile()
    {
        factFile.SetActive(false);
    }
}
