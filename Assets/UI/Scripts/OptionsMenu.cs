/*  
 *  AUTHOR: Jon Munro 
 *  CREATED: 30/03/2021 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionsMenu : MonoBehaviour
{
    public GameObject videoMenu, audioMenu, gameplayMenu, inputMenu;

    public void VideoOptions()
    {
        CloseMenus();
        videoMenu.SetActive(true);
    }
    public void AudioOptions()
    {
        CloseMenus();
        audioMenu.SetActive(true);
    }
    public void GameplayOptions()
    {
        CloseMenus();
        gameplayMenu.SetActive(true);
    }
    public void InputOptions()
    {
        CloseMenus();
        inputMenu.SetActive(true);
    }

    void CloseMenus()
    {
        videoMenu.SetActive(false);
        audioMenu.SetActive(false);
        gameplayMenu.SetActive(false);
        inputMenu.SetActive(false);
    }
}
