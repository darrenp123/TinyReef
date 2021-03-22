/*  
 *  AUTHOR: Jon Munro
 *  CREATED: 22/03/2021 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FactFile : MonoBehaviour
{
    [SerializeField]
    GameObject factFileContainer, nextPageButton, prevPageButton, pauseMenuPage;
    public Texture[] pages;
    [SerializeField]
    int currentPage;
    [SerializeField]
    RawImage pageDisplay;

    // Update is called once per frame
    void Update()
    {
        //add arrow key page swapping
    }

    public void NextPage()
    {
        if(currentPage != pages.Length)
        {
            currentPage++;
            pageDisplay.texture = pages[currentPage];
            prevPageButton.SetActive(true);
            if(currentPage == (pages.Length-1))
            {
                nextPageButton.SetActive(false);
            }
        }
    }

    public void PrevPage()
    {
        if (currentPage != 0)
        {
            currentPage--;
            pageDisplay.texture = pages[currentPage];
            nextPageButton.SetActive(true);
            if(currentPage == 0)
            {
                prevPageButton.SetActive(false);
            }
        }
    }

    public void OpenFactFile()
    {
        factFileContainer.SetActive(true);
        ResetPage();
    }

    public void CloseFactFileToMainMenu()
    {
        factFileContainer.SetActive(false);
    }

    public void CloseFactFileToPauseMenu()
    {
        ResetPage();
        factFileContainer.SetActive(false);
        pauseMenuPage.SetActive(true);
    }

    void ResetPage()
    {
        currentPage = 0;
        pageDisplay.texture = pages[currentPage];
        nextPageButton.SetActive(true);
        prevPageButton.SetActive(false);
    }
}
