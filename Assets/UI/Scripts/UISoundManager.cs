/*  
 *  AUTHOR: Jon Munro & Tiago Guerra
 *  CREATED: 02/03/2021 
 */

using UnityEngine;
using UnityEngine.UI;

public class UISoundManager : MonoBehaviour
{
    AudioSource source;
    [SerializeField]
    Slider audioSlider;
    [SerializeField]
    AudioClip[] clips;
    AudioClip soundToPlay;
    bool muted;

    private void Start()
    {
        source = GetComponent<AudioSource>();
        audioSlider.value = PlayerPrefs.GetFloat("AudioVolume");
        muted = false;
    }

    public void PlaySound(string sound)
    {
        for (int i = 0; i < clips.Length; i++)
        {
            if(clips[i].name == sound)
            {
                soundToPlay = clips[i];
                break;
            }
        }
        if (soundToPlay != null)
        {
            source.PlayOneShot(soundToPlay);
            soundToPlay = null;
        }
    }

    public void AssignSlider(Slider newSlider) {
        audioSlider = newSlider;
        audioSlider.value = PlayerPrefs.GetFloat("AudioVolume");
    }

    public void VolumeControl(float volume) {
        source.volume = volume;
        PlayerPrefs.SetFloat("AudioVolume", volume);
        PlayerPrefs.Save();
    }

    public void Mute() {
        if (muted) {
            source.volume = PlayerPrefs.GetFloat("AudioVolume");
            muted = false;
        } else {
            source.volume = 0;
            muted = true;
        }
    }
}
