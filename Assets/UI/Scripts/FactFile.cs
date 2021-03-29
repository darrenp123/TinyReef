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
    GameObject factFileContainer, nextPageButton, prevPageButton, pauseMenuPage, contentsPage, contentsPageButton;
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
            if(currentPage == 1)
            {
                contentsPage.SetActive(true);
            }else if(currentPage != 1)
            {
                contentsPage.SetActive(false);
            }
            if(currentPage == (pages.Length-1))
            {
                nextPageButton.SetActive(false);
            }
            if(currentPage > 1)
            {
                contentsPageButton.SetActive(true);
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
            if (currentPage == 1)
            {
                contentsPage.SetActive(true);
            }
            else if (currentPage != 1)
            {
                contentsPage.SetActive(false);
            }
            if (currentPage <= 1)
            {
                contentsPageButton.SetActive(false);
            }
        }
    }

    public void OpenFactFile()
    {
        factFileContainer.SetActive(true);
        //ResetPage();
    }

    public void CloseFactFileToMainMenu()
    {
        ResetPage();
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
        contentsPage.SetActive(false);
        contentsPageButton.SetActive(false);
    }

    public void OpenPage(int pageNumber)
    {
        currentPage = pageNumber;
        pageDisplay.texture = pages[currentPage];
        if(currentPage == (pages.Length-1))
        {
            nextPageButton.SetActive(false);
        }
        contentsPage.SetActive(false);
        contentsPageButton.SetActive(true);
    }

    public void ContentsPage()
    {
        currentPage = 1;
        pageDisplay.texture = pages[currentPage];
        contentsPage.SetActive(true);
    }
}
