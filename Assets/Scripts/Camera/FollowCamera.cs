using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class FollowCamera : MonoBehaviour
{
    [SerializeField] InputAction tt;
    private CinemachineInputProvider InputProvider;
    private CinemachineFreeLook FreeLookCamera;
    [SerializeField]
    private float ZoomSpeed = 5f;
    [SerializeField]
    private float ZoomInMax = 5f;
    [SerializeField]
    private float ZoomOutMax = 40f;
    [SerializeField]
    private float MaxZoomFishFocusMiddleRadious = 7f;
    [SerializeField]
    private float MaxZoomFishFocusTopBottomRadious = 5f;
    [SerializeField]
    private float MinZoomFishFocusMiddleRadious = 7f;
    [SerializeField]
    private float MinZoomFishFocusTopBottomRadious = 5f;
    [SerializeField]
    private float MaxZoomTankFocusMiddleRadious = 20f;
    [SerializeField]
    private float MaxZoomTankFocusTopBottomRadious = 18f;
    [SerializeField]
    private float MinZoomTankFocusMiddleRadious = 20f;
    [SerializeField]
    private float MinZoomTankFocusTopBottomRadious = 18f;
    [SerializeField]
    private InputActionReference AxisControl;
    [SerializeField]
    private GameObject Center;
    [SerializeField]
    private Player PlayerState;
    [SerializeField]
    //private InputActionAsset controls;
    bool FishInFocus;
    float FishZoomDifference, TankZoomDifference;

    private void Awake()
    {
        InputProvider = GetComponent<CinemachineInputProvider>();
        FreeLookCamera = GetComponent<CinemachineFreeLook>();
    }

    void Start()
    {
        FreeLookCamera.m_Follow = Center.transform;
        FreeLookCamera.m_LookAt = Center.transform;
        FishZoomDifference = (MaxZoomFishFocusTopBottomRadious - MinZoomFishFocusTopBottomRadious) / (MaxZoomFishFocusMiddleRadious - MinZoomFishFocusMiddleRadious);
        TankZoomDifference = (MaxZoomTankFocusTopBottomRadious - MinZoomTankFocusTopBottomRadious) / (MaxZoomTankFocusMiddleRadious - MinZoomTankFocusMiddleRadious);
        FishInFocus = false;
        FocusFish(false);

        //Set actions
        
    }

    void Update()
    {
        //Zoom
        if (EventSystem.current.IsPointerOverGameObject()) return;

        float z = InputProvider.GetAxisValue(2);
        if (z != 0) ZoomInZoomOut(z);
    }

    void ZoomScreen(float increment)
    {
        float FOV = FreeLookCamera.m_Lens.FieldOfView;
        float target = Mathf.Clamp(FOV + increment, ZoomInMax, ZoomOutMax);
        FreeLookCamera.m_Lens.FieldOfView = Mathf.Lerp(FOV, target, ZoomSpeed * Time.unscaledDeltaTime);
    }

    public void OnLeftClick(InputAction.CallbackContext context)
    {
        if (!PlayerState.IsUION() && context.performed && !EventSystem.current.IsPointerOverGameObject())
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit hit;
            int layer_mask = LayerMask.GetMask("Tier1", "Tier2", "Tier3",
                "Sise_1", "Sise_2", "Sise_3", "Sise_4", "Sise_5", "Sise_6", "Sise_7", "Sise_8", "Sise_9", "Sise_10");
          
            if (Physics.SphereCast(ray, 0.1f, out hit, Mathf.Infinity, layer_mask))
            {
                var fish = hit.transform.GetComponent<SFlockUnit>();
                if (fish)
                {
                    FreeLookCamera.m_Follow = hit.transform;
                    FreeLookCamera.m_LookAt = hit.transform;
                    FocusFish(true);
                    PlayerState.IsLookingAtFishState(true, fish);
                }
            }
        }
    }

    public void CycleLeft(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            SFlockUnit fish = PlayerState.GoToNextFishInFlock(true);
            FreeLookCamera.m_Follow = fish.MyTransform;
            FreeLookCamera.m_LookAt = fish.MyTransform;
            FocusFish(true);
        }
    }

    public void CycleRight(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            SFlockUnit fish = PlayerState.GoToNextFishInFlock(false);
            FreeLookCamera.m_Follow = fish.MyTransform;
            FreeLookCamera.m_LookAt = fish.MyTransform;
            FocusFish(true);
        }
    }

    public void ReturnCamera(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            FreeLookCamera.m_Follow = Center.transform;
            FreeLookCamera.m_LookAt = Center.transform;
            FocusFish(false);
            PlayerState.IsLookingAtFishState(false, null);
        }
    }

    public void DisableEnableCameraInput(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            InputProvider.XYAxis = AxisControl;
        }
        else if (context.canceled)
        {
            InputProvider.XYAxis = null;
        }
    }

    public void ZoomInZoomOut(float increment)
    {
        float topOrbit = FreeLookCamera.m_Orbits[0].m_Radius;
        float middleOrbit = FreeLookCamera.m_Orbits[1].m_Radius;
        float targetTop, targetMiddle;
        if (FishInFocus)
        {
            targetTop = Mathf.Clamp(topOrbit + (increment * FishZoomDifference), MinZoomFishFocusTopBottomRadious, MaxZoomFishFocusTopBottomRadious);
            targetMiddle = Mathf.Clamp(middleOrbit + (increment * FishZoomDifference), MinZoomFishFocusMiddleRadious, MaxZoomFishFocusMiddleRadious);
        }
        else
        {
            targetTop = Mathf.Clamp(topOrbit + (increment * TankZoomDifference), MinZoomTankFocusTopBottomRadious, MaxZoomTankFocusTopBottomRadious);
            targetMiddle = Mathf.Clamp(middleOrbit + (increment * TankZoomDifference), MinZoomTankFocusMiddleRadious, MaxZoomTankFocusMiddleRadious);
        }
        FreeLookCamera.m_Orbits[0].m_Radius = Mathf.Lerp(topOrbit, targetTop, ZoomSpeed * Time.unscaledDeltaTime);
        FreeLookCamera.m_Orbits[1].m_Radius = Mathf.Lerp(middleOrbit, targetMiddle, ZoomSpeed * Time.unscaledDeltaTime);
        FreeLookCamera.m_Orbits[2].m_Radius = Mathf.Lerp(topOrbit, targetTop, ZoomSpeed * Time.unscaledDeltaTime);
    }

    public void FocusFish(bool state)
    {
        if (state)
        {
            FreeLookCamera.m_Orbits[0].m_Radius = MaxZoomFishFocusTopBottomRadious;
            FreeLookCamera.m_Orbits[1].m_Radius = MaxZoomFishFocusMiddleRadious;
            FreeLookCamera.m_Orbits[2].m_Radius = MaxZoomFishFocusTopBottomRadious;
            FishInFocus = true;
        }
        else
        {
            FreeLookCamera.m_Orbits[0].m_Radius = MaxZoomTankFocusTopBottomRadious;
            FreeLookCamera.m_Orbits[1].m_Radius = MaxZoomTankFocusMiddleRadious;
            FreeLookCamera.m_Orbits[2].m_Radius = MaxZoomTankFocusTopBottomRadious;
            FishInFocus = false;
        }
    }
}
