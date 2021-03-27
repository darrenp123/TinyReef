/*  
 *  AUTHOR: Jon Munro & Darren Piscini
 *  CREATED: 14/03/2021 
 */

using UnityEngine;

public class GameSpeedController : MonoBehaviour
{
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
    public void Pause()
    {
        Time.timeScale = 0f;
    }
    public void Play()
    {
        Time.timeScale = 1f;
    }
}
