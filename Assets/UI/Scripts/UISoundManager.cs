/*  
 *  AUTHOR: Jon Munro 
 *  CREATED: 02/03/2021 
 */

using UnityEngine;

public class UISoundManager : MonoBehaviour
{
    AudioSource source;
    [SerializeField]
    AudioClip[] clips;
    AudioClip soundToPlay;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
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
}
