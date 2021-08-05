// controls how the fog acts, when the camera is inside the bounds of this game object it changes the fog density and
// color. Must be in an object with a collider, in this case it is usually on the post proses volume object 
using UnityEngine;

public class TankFog : MonoBehaviour
{
    [SerializeField] float undewaterFogDensity;
    [SerializeField] Color undewaterFogColor;
    private float _defaultFogDensity;
    private Color _defaultFogColor;
    private Transform _cameraTransform;
    private Bounds _tankBounds;

    void Awake()
    {
        _defaultFogDensity = RenderSettings.fogDensity;
        _defaultFogColor = RenderSettings.fogColor;
        _cameraTransform = Camera.main.transform;
        _tankBounds = GetComponent<Collider>().bounds;
    }

    private void Update()
    {
        if (_tankBounds.Contains(_cameraTransform.position))
        {
            RenderSettings.fogDensity = undewaterFogDensity;
            RenderSettings.fogColor = undewaterFogColor;
        }
        else
        {
            RenderSettings.fogDensity = _defaultFogDensity;
            RenderSettings.fogColor = _defaultFogColor;
        }
    }
}
