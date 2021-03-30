/*  
 *  AUTHOR: Jon Munro 
 *  CREATED: 30/03/2021 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionsMenu : MonoBehaviour
{
    public bool isMuted;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ToggleMuteAudio()
    {
        if(isMuted == true)
        {
            isMuted = false;
            AudioListener.volume = 1;
        }else if(isMuted == false)
        {
            isMuted = true;
            AudioListener.volume = 0;
        }
    }
}
