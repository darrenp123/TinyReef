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
    [SerializeField]
    AudioClip[] bubbleClips;
    AudioClip soundToPlay;
    bool muted;

    private void Start()
    {
        if (!PlayerPrefs.HasKey("AudioVolume")) PlayerPrefs.SetFloat("AudioVolume", 0.5f);
        source = GetComponent<AudioSource>();
        audioSlider.value = PlayerPrefs.GetFloat("AudioVolume");
        muted = false;
    }

    public void PlaySound(string sound)
    {
        for (int i = 0; i < clips.Length; i++)
        {
            if (clips[i].name == sound)
            {
                soundToPlay = clips[i];
                break;
            }
            else if (sound == "Bubbles")
            {
                int rand = Random.Range(0, 2);
                soundToPlay = bubbleClips[rand];
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
