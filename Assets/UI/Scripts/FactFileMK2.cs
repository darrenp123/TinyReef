/*  
 *  AUTHOR: Jon Munro
 *  CREATED: 29/03/2021 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FactFileMK2 : MonoBehaviour
{
    [SerializeField]
    GameObject factFileContainer, pauseMenuPage;
    public Texture[] pages;
    [SerializeField]
    int currentPage;
    [SerializeField]
    RawImage pageDisplay;

    public void OpenFactFile()
    {
        factFileContainer.SetActive(true);
        //ResetPage();
    }

    public void CloseFactFileToMainMenu()
    {
        factFileContainer.SetActive(false);
    }

    public void CloseFactFileToPauseMenu()
    {
        factFileContainer.SetActive(false);
        pauseMenuPage.SetActive(true);
    }


    public void OpenPage(int pageNumber)
    {
        currentPage = pageNumber;
        pageDisplay.texture = pages[currentPage];
    }
}
