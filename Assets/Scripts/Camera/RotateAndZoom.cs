using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class RotateAndZoom : MonoBehaviour
{

    private CinemachineInputProvider inputProvider;
    private CinemachineFreeLook freeLookCamera;
    [SerializeField]
    private float zoomSpeed = 5f;
    [SerializeField]
    private float zoomInMax = 5f;
    [SerializeField]
    private float zoomOutMax = 40f;
    [SerializeField]
    private float fishFocusMiddleRadious = 7f;
    [SerializeField]
    private float fishFocusTopBottomRadious = 5f;
    [SerializeField]
    private float tankFocusMiddleRadious = 20f;
    [SerializeField]
    private float tankFocusTopBottomRadious = 18f;
    [SerializeField]
    private InputActionReference axisControl;
    [SerializeField]
    private GameObject center;
    [SerializeField]
    private GameObject[] fishList;
    private int currentFish;

    private void Awake() {
        inputProvider = GetComponent<CinemachineInputProvider>();
        freeLookCamera = GetComponent<CinemachineFreeLook>();
        currentFish = 0;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Zoom
        float z = inputProvider.GetAxisValue(2);
        if (z != 0) ZoomScreen(z);
    }

    void ZoomScreen(float increment) {
        float FOV = freeLookCamera.m_Lens.FieldOfView;
        float target = Mathf.Clamp(FOV + increment, zoomInMax, zoomOutMax);
        freeLookCamera.m_Lens.FieldOfView = Mathf.Lerp(FOV, target, zoomSpeed* Time.deltaTime);
    }

    public void OnLeftClick(InputAction.CallbackContext context) {
        if (context.performed) {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit)) {
                Debug.Log(hit.transform.tag);
                if (hit.transform.tag == "Fish") {
                    Debug.Log("This is a Fish");
                    freeLookCamera.m_Follow = hit.transform.gameObject.transform;
                    freeLookCamera.m_LookAt = hit.transform.gameObject.transform;
                    FocusFish(true);
                }
            }
        }
    }

    public void CycleLeft(InputAction.CallbackContext context) {
        if (context.performed) {
            currentFish--;
            if (currentFish < 0) currentFish = fishList.Length - 1;
            freeLookCamera.m_Follow = fishList[currentFish].transform;
            freeLookCamera.m_LookAt = fishList[currentFish].transform;
            FocusFish(true);
        }
    }

    public void CycleRight(InputAction.CallbackContext context) {
        if (context.performed) {
            currentFish++;
            if (currentFish >= fishList.Length) currentFish = 0;
            freeLookCamera.m_Follow = fishList[currentFish].transform;
            freeLookCamera.m_LookAt = fishList[currentFish].transform;
            FocusFish(true);
        }
    }

    public void ReturnCamera(InputAction.CallbackContext context) {
        if (context.performed) {
            freeLookCamera.m_Follow = center.transform;
            freeLookCamera.m_LookAt = center.transform;
            FocusFish(false);
        }
    }

    public void DisableEnableCameraInput(InputAction.CallbackContext context) {
        if (context.performed) {
            Debug.Log("Enable");
            inputProvider.XYAxis = axisControl;
        }
        else if(context.canceled) {
            Debug.Log("Disable");
            inputProvider.XYAxis = null;
        }
    }


    public void FocusFish(bool state) {
        if (state) {
            freeLookCamera.m_Orbits[0].m_Radius = fishFocusTopBottomRadious;
            freeLookCamera.m_Orbits[1].m_Radius = fishFocusMiddleRadious;
            freeLookCamera.m_Orbits[2].m_Radius = fishFocusTopBottomRadious;
        } else {
            freeLookCamera.m_Orbits[0].m_Radius = tankFocusTopBottomRadious;
            freeLookCamera.m_Orbits[1].m_Radius = tankFocusMiddleRadious;
            freeLookCamera.m_Orbits[2].m_Radius = tankFocusTopBottomRadious;
        }
    }
}
