/*  
 *  AUTHOR: Jon Munro 
 *  CREATED: 23/02/2021 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public static bool isPaused;
    [SerializeField]
    GameObject pauseMenu, quitOptions;
    // Start is called before the first frame update
    void Start()
    {
        isPaused = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetButtonDown("Pause"))
        {
            if(isPaused)
            {
                
                UnPauseGame();
            }
            else
            {
                PauseGame();
            }
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
        if (quitOptions.activeInHierarchy == true)
        {
            CloseQuitOptions();
        }
        isPaused = false;
        Time.timeScale = 1;
        pauseMenu.SetActive(false);
    }

    public void OpenQuitOptions()
    {
        quitOptions.SetActive(true);
    }
    public void CloseQuitOptions()
    {
        quitOptions.SetActive(false);
    }
}
