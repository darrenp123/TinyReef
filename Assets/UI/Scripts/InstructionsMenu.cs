/*  
 *  AUTHOR: Jon Munro 
 *  CREATED: 02/03/2021 
 */

using UnityEngine;

public class InstructionsMenu : MonoBehaviour
{
    [SerializeField]
    TRPauseMenu pauseMenu;
    [SerializeField]
    GameObject instructionsPage, mainMenuPage, pauseMenuPage, gameplayPage, TraitsPage, controlsPage;
    [SerializeField]
    GameObject[] traitMoreInfoPages;

    public void OpenInstructionsPage()
    {
        mainMenuPage.SetActive(false);
        instructionsPage.SetActive(true);
    }

    public void CloseInstructionsPageToMainMenu()
    {
        instructionsPage.SetActive(false);
        mainMenuPage.SetActive(true);
    }

    public void CloseInstructionsPageToPauseMenu()
    {
        instructionsPage.SetActive(false);
        pauseMenuPage.SetActive(true);
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

    public void OpenTraitMoreInfoMenuFromOutliner(GameObject moreInfoScreen)
    {
        pauseMenu.PauseGame();
        pauseMenu.OpenInstructionsMenu();
        OpenTraitsInstructions();
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

    public void CloseAllMenus()
    {
        for (int i = 0; i < traitMoreInfoPages.Length; i++)
        {
            traitMoreInfoPages[i].SetActive(false);
        }
        BackToInstructionsPage();
    }

    public void BackToGame()
    {
        CloseAllMenus();
        pauseMenu.CloseInstructionsMenu();
        pauseMenu.UnPauseGame();
    }
}
