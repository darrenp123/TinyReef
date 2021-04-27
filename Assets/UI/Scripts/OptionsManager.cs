/*  
 *  AUTHOR: Jon Munro & Tiago Guerra
 *  CREATED: 30/03/2021 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionsManager : MonoBehaviour
{
    [SerializeField]
    Slider freeCamSensSlider, freeCamMoveSpeedSlider, followCameraSensSlider, followCameraZoomSpeedSlider;

    public void ChangeFreeCamSensitivity(float speed)
    {
        PlayerPrefs.SetFloat("FreeCamSens", speed);
        PlayerPrefs.Save();
    }

    public void ChangeFreeCamMoveSpeed(float speed)
    {
        PlayerPrefs.SetFloat("FreeCamMoveSpeed", speed);
        PlayerPrefs.Save();
    }

    public void ChangeFollowCameraSensitivity(float speed)
    {
        PlayerPrefs.SetFloat("FollowCamRotSpeed", speed);
        PlayerPrefs.Save();
    }

    public void ChangeFollowCameraZoomSpeed(float speed)
    {
        PlayerPrefs.SetFloat("FollowCamZoomSpeed", speed);
        PlayerPrefs.Save();
    }

    public void InitialiseCameras()
    {
        freeCamSensSlider.value = PlayerPrefs.GetFloat("FreeCamSens");
        freeCamMoveSpeedSlider.value = PlayerPrefs.GetFloat("FreeCamMoveSpeed");
        followCameraSensSlider.value = PlayerPrefs.GetFloat("FollowCamRotSpeed");
        followCameraZoomSpeedSlider.value = PlayerPrefs.GetFloat("FollowCamZoomSpeed");
    }
}
