using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class RotateAndZoom : MonoBehaviour
{

    private CinemachineInputProvider InputProvider;
    private CinemachineFreeLook FreeLookCamera;
    [SerializeField]
    private float ZoomSpeed = 5f;
    [SerializeField]
    private float ZoomInMax = 5f;
    [SerializeField]
    private float ZoomOutMax = 40f;
    [SerializeField]
    private float FishFocusMiddleRadious = 7f;
    [SerializeField]
    private float FishFocusTopBottomRadious = 5f;
    [SerializeField]
    private float TankFocusMiddleRadious = 20f;
    [SerializeField]
    private float TankFocusTopBottomRadious = 18f;
    [SerializeField]
    private InputActionReference AxisControl;
    [SerializeField]
    private GameObject Center;
    [SerializeField]
    private GameObject[] FishList;
    [SerializeField]
    private Player PlayerState;
    private int CurrentFish;

    private void Awake() {
        InputProvider = GetComponent<CinemachineInputProvider>();
        FreeLookCamera = GetComponent<CinemachineFreeLook>();
        CurrentFish = 0;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Zoom
        float z = InputProvider.GetAxisValue(2);
        if (z != 0) ZoomScreen(z);
    }

    void ZoomScreen(float increment) {
        float FOV = FreeLookCamera.m_Lens.FieldOfView;
        float target = Mathf.Clamp(FOV + increment, ZoomInMax, ZoomOutMax);
        FreeLookCamera.m_Lens.FieldOfView = Mathf.Lerp(FOV, target, ZoomSpeed* Time.deltaTime);
    }

    public void OnLeftClick(InputAction.CallbackContext context) {
        if (context.performed) {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit)) {
                var fish = hit.transform.GetComponent<SFlockUnit>();
                if (fish) {
                    FreeLookCamera.m_Follow = hit.transform;
                    FreeLookCamera.m_LookAt = hit.transform;
                    FocusFish(true, fish);
                }
            }
        }
    }

    public void CycleLeft(InputAction.CallbackContext context) {
        if (context.performed) {
            CurrentFish--;
            if (CurrentFish < 0) CurrentFish = FishList.Length - 1;
            FreeLookCamera.m_Follow = FishList[CurrentFish].transform;
            FreeLookCamera.m_LookAt = FishList[CurrentFish].transform;
            FocusFish(true, FishList[CurrentFish].GetComponent<SFlockUnit>());
        }
    }

    public void CycleRight(InputAction.CallbackContext context) {
        if (context.performed) {
            CurrentFish++;
            if (CurrentFish >= FishList.Length) CurrentFish = 0;
            FreeLookCamera.m_Follow = FishList[CurrentFish].transform;
            FreeLookCamera.m_LookAt = FishList[CurrentFish].transform;
            FocusFish(true, FishList[CurrentFish].GetComponent<SFlockUnit>());
        }
    }

    public void ReturnCamera(InputAction.CallbackContext context) {
        if (context.performed) {
            FreeLookCamera.m_Follow = Center.transform;
            FreeLookCamera.m_LookAt = Center.transform;
            FocusFish(false, null);
        }
    }

    public void DisableEnableCameraInput(InputAction.CallbackContext context) {
        if (context.performed) {
            Debug.Log("Enable");
            InputProvider.XYAxis = AxisControl;
        }
        else if(context.canceled) {
            Debug.Log("Disable");
            InputProvider.XYAxis = null;
        }
    }


    public void FocusFish(bool state, SFlockUnit fish) {
        if (state) {
            FreeLookCamera.m_Orbits[0].m_Radius = FishFocusTopBottomRadious;
            FreeLookCamera.m_Orbits[1].m_Radius = FishFocusMiddleRadious;
            FreeLookCamera.m_Orbits[2].m_Radius = FishFocusTopBottomRadious;
            PlayerState.IsLookingAtFishState(true, fish);
        } else {
            FreeLookCamera.m_Orbits[0].m_Radius = TankFocusTopBottomRadious;
            FreeLookCamera.m_Orbits[1].m_Radius = TankFocusMiddleRadious;
            FreeLookCamera.m_Orbits[2].m_Radius = TankFocusTopBottomRadious;
            PlayerState.IsLookingAtFishState(false, fish);
        }
    }
}
