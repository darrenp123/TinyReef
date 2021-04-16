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
            spotLight.cookie = frames[_frameIndex];
        }

        _frameIndex = (_frameIndex + 1) % frames.Length;
    }
}
