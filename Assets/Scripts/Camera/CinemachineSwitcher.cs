using UnityEngine;
using UnityEngine.InputSystem;

public class CinemachineSwitcher : MonoBehaviour
{

    [SerializeField]
    private InputAction action;
    private Animator animator;
    [SerializeField]
    private FollowCamera followCamera;
    [SerializeField]
    public FreeCamera freeCamera;

    private void Awake() {
        animator = GetComponent<Animator>();
    }

    private void OnEnable() {
        action.Enable();
    }

    private void OnDisable() {
        action.Disable();
    }

    void Start()
    {
        action.performed += _ => SwitchState();
    }

    public void SwitchState() {
        if (followCamera.GetCurrentState()) {
            animator.Play("FreeCamera");
            freeCamera.ChangeState(true);
            followCamera.ChangeState(false);
        } else {
            animator.Play("FollowCamera");
            followCamera.ChangeState(true);
            freeCamera.ChangeState(false);
        }
    }
}
