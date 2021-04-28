/*  
 *  AUTHOR: Jon Munro
 *  CREATED: 29/03/2021 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FactFileMK2 : MonoBehaviour
{
    [SerializeField]
    GameObject factFileContainer, pauseMenuPage, statsContainer;
    [SerializeField]
    TMP_Text sizeText, speedText, sensText, lifespanText, hungerText;
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
        statsContainer.SetActive(false);
        pageDisplay.texture = pages[currentPage];
    }

    public void ShowStats(FishStatObj stats)
    {
        statsContainer.SetActive(true);
        sizeText.text = stats.size.ToString();
        speedText.text = stats.speed.ToString();
        sensText.text = stats.sensoryRadius.ToString();
        lifespanText.text = stats.lifespan.ToString();
        //hungerText.text = stats.hunger.ToString();
        hungerText.text = "n/a";
    }
}
