/*  
 *  AUTHOR: Jon Munro & Darren Piscini
 *  CREATED: 14/03/2021 
 */

using UnityEngine;
using UnityEngine.UI;

public class GameSpeedController : MonoBehaviour
{
    bool isPaused;
    [SerializeField]
    Image playButtonImage;
    [SerializeField]
    Sprite playSprite, pauseSprite;

    public void HalfSpeed()
    {
        Time.timeScale = .5f;
    }
    public void DoubleSpeed()
    {
        Time.timeScale = 2f;
    }
    public void QuadSpeed()
    {
        Time.timeScale = 4f;
    }
    public void StandardSpeed()
    {
        Time.timeScale = 1f;
    }
    public void PlayToggle()
    {
        if(isPaused == true)
        {
            isPaused = false;
            Time.timeScale = 1f;
            playButtonImage.sprite = pauseSprite;

        }else if(isPaused == false)
        {
            isPaused = true;
            Time.timeScale = 0f;
            playButtonImage.sprite = playSprite;
        }
    }
}
