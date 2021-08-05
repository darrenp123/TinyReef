//This script handles the logic of switching images to make the water caustics seem real
//this script must be on the parent object that contains all the spotlights that what to have water caustics
using UnityEngine;

public class AnimatedProjector : MonoBehaviour
{
    [SerializeField] private float fps = 30.0f;
    [SerializeField] private Texture2D[] frames;
    private int _frameIndex;
    private Light[] _SpotLights;

    void Start()
    {
        InvokeRepeating(nameof(NextFrame), 1 / fps, 1 / fps);
        _SpotLights = GetComponentsInChildren<Light>(false);
    }

    void NextFrame()
    {
        foreach (Light spotLight in _SpotLights)
        {
            // sets the spotlight with a new image every frame
            spotLight.cookie = frames[_frameIndex];
        }
        // resets index when _frameIndex + 1 is equal the size of the images array;
        _frameIndex = (_frameIndex + 1) % frames.Length;
    }
}
