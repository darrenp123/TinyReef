/*  
 *  AUTHOR: Jon Munro 
 *  CREATED: 30/03/2021 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionsManager : MonoBehaviour
{
    [SerializeField]
    GameObject freeCam, followCam;
    [SerializeField]
    Slider freeCamSensSlider, freeCamMoveSpeedSlider, followCameraSensSlider, followCameraZoomSpeedSlider;

    public void changeFreeCamSensitivity()
    {
        if(freeCam != null)
            freeCam.GetComponent<FreeCamera>().ChangeRotationSpeed(freeCamSensSlider.value);
        PlayerPrefs.SetFloat("FreeCamSens", freeCamSensSlider.value);
    }
    public void changeFreeCamMoveSpeed()
    {
        if (freeCam != null)
            freeCam.GetComponent<FreeCamera>().ChangeCameraSpeed(freeCamMoveSpeedSlider.value);
        PlayerPrefs.SetFloat("FreeCamMoveSpeed", freeCamMoveSpeedSlider.value);
    }
    public void changeFollowCameraSensitivity()
    {
        if (followCam != null)
            followCam.GetComponent<FollowCamera>().ChangeRotationSpeed(followCameraSensSlider.value);
        PlayerPrefs.SetFloat("FollowCamRotSpeed", followCameraSensSlider.value);
    }
    public void changeFollowCameraZoomSpeed()
    {
        if (followCam != null)
            followCam.GetComponent<FollowCamera>().ChangeRotationSpeed(followCameraZoomSpeedSlider.value);
        PlayerPrefs.SetFloat("FollowCamZoomSpeed", followCameraZoomSpeedSlider.value);
    }

    public void InitialiseCameras()
    {
        freeCam = GameObject.FindGameObjectWithTag("FreeCam");
        followCam = GameObject.FindGameObjectWithTag("FollowCam");
        InitCamOptions();
    }

    //public void GetSliders(GameObject menu)
    //{
    //    Slider[] sliders = menu.GetComponentsInChildren<Slider>();
    //    freeCamSensSlider = sliders[0];
    //    freeCamMoveSpeedSlider = sliders[1];
    //    followCameraSensSlider = sliders[2];
    //    followCameraZoomSpeedSlider = sliders[3];
    //}

    void InitCamOptions()
    {
        freeCamSensSlider.value = PlayerPrefs.GetFloat("FreeCamSens");
        freeCamMoveSpeedSlider.value = PlayerPrefs.GetFloat("FreeCamMoveSpeed");
        followCameraSensSlider.value = PlayerPrefs.GetFloat("FollowCamRotSpeed");
        followCameraZoomSpeedSlider.value = PlayerPrefs.GetFloat("FollowCamZoomSpeed");
        SetCamOptions();
    }

    void SetCamOptions()
    {
        changeFreeCamSensitivity();
        changeFreeCamMoveSpeed();
        changeFollowCameraSensitivity();
        changeFollowCameraZoomSpeed();
    }
}
