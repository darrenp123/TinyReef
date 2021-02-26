/*  
 *  AUTHOR: Jon Munro 
 *  CREATED: 23/02/2021 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField]
    GameObject instructionsPage, mainMenuPage, gameplayPage, TraitsPage, controlsPage;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

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

    #region InstructionsPage

    public void OpenGameplayInstructions()
    {
        gameplayPage.SetActive(true);
    }
    public void OpenTraitsInstructions()
    {
        TraitsPage.SetActive(true);
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

    public void QuitGame()
    {
        Application.Quit();
    }
}
