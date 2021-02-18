using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class RotateAndZoom : MonoBehaviour
{

    private CinemachineInputProvider inputProvider;
    private CinemachineFreeLook freeLookCamera;
    [SerializeField]
    private float zoomSpeed = 5f;
    [SerializeField]
    private float zoomInMax = 5f;
    [SerializeField]
    private float zoomOutMax = 10f;

    private void Awake() {
        inputProvider = GetComponent<CinemachineInputProvider>();
        freeLookCamera = GetComponent<CinemachineFreeLook>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float z = inputProvider.GetAxisValue(2);
        if (z != 0) ZoomScreen(z);
    }

    void ZoomScreen(float increment) {
        float FOV = freeLookCamera.m_Lens.FieldOfView;
        float target = Mathf.Clamp(FOV + increment, zoomInMax, zoomOutMax);
        freeLookCamera.m_Lens.FieldOfView = Mathf.Lerp(FOV, target, zoomSpeed* Time.deltaTime);
    }

}
