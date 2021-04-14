using System.Collections;
using System.Collections.Generic;
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
