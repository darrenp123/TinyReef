/*  
 *  AUTHOR: Jon Munro 
 *  CREATED: 23/02/2021 
 */

using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField]
    GameObject instructionsPage, optionsPage, creditsPage, mainMenuPage, gamemodeSelectionPage, challengeModeText, sandboxModeText, gamemodeSelectionPlayButton;

    public void OpenInstructionsPage()
    {
        mainMenuPage.SetActive(false);
        instructionsPage.SetActive(true);
    }

    public void CloseInstructionsPage()
    {
        instructionsPage.SetActive(false);
        mainMenuPage.SetActive(true);
    }

    public void OpenOptionsPage()
    {
        mainMenuPage.SetActive(false);
        optionsPage.SetActive(true);
    }

    public void CloseOptionsPage()
    {
        optionsPage.SetActive(false);
        mainMenuPage.SetActive(true);
    }

    public void OpenCreditsPage()
    {
        mainMenuPage.SetActive(false);
        creditsPage.SetActive(true);
    }

    public void CloseCreditsPage()
    {
        creditsPage.SetActive(false);
        mainMenuPage.SetActive(true);
    }

    #region GamemodeSelection

    public void OpenGamemodeSelectionPage()
    {
        mainMenuPage.SetActive(false);
        gamemodeSelectionPage.SetActive(true);
    }

    public void CloseGamemodeSelectionPage()
    {
        gamemodeSelectionPage.SetActive(false);
        mainMenuPage.SetActive(true);
    }

    public void SelectedSandboxMode()
    {
        challengeModeText.SetActive(false);
        sandboxModeText.SetActive(true);
        //TODO set gamemode
        gamemodeSelectionPlayButton.GetComponent<Button>().interactable = true;
    }

    public void SelectedChallengeMode()
    {
        sandboxModeText.SetActive(false);
        challengeModeText.SetActive(true);
        //TODO set gamemode
        gamemodeSelectionPlayButton.GetComponent<Button>().interactable = true;
    }

    #endregion

    public void QuitGame()
    {
        Application.Quit();
    }
}
