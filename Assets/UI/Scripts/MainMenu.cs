/*  
 *  AUTHOR: Jon Munro 
 *  CREATED: 23/02/2021 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField]
    GameObject instructionsPage, mainMenuPage, gamemodeSelectionPage, gameplayPage, TraitsPage, controlsPage, challengeModeText, sandboxModeText, gamemodeSelectionPlayButton;

    #region InstructionsPage

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

    public void OpenGameplayInstructions()
    {
        gameplayPage.SetActive(true);
    }
    public void OpenTraitsInstructions()
    {
        TraitsPage.SetActive(true);
    }

    public void OpenTraitMoreInfoMenu(GameObject moreInfoScreen)
    {
        //instructionsPage.SetActive(false);
        moreInfoScreen.SetActive(true);
    }

    public void CloseTraitMoreInfoMenu(GameObject moreInfoScreen)
    {
        moreInfoScreen.SetActive(false);
        //instructionsPage.SetActive(true);
    }

    public void OpenControlsInstructions()
    {
        controlsPage.SetActive(true);
    }

    public void BackToInstructionsPage()
    {
        gameplayPage.SetActive(false);
        TraitsPage.SetActive(false);
        controlsPage.SetActive(false);
    }
    #endregion

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
