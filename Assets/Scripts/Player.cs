using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{

    Fish CurrentFish;
    [SerializeField]
    GameObject FishStats;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SeeFishStats(InputAction.CallbackContext context) {
        if (context.performed) {
            if (FishStats.activeSelf) {
                FishStats.SetActive(false);
            } else {
                FishStats.SetActive(true);
            }
        }
    }

    public void SetCurrentFish(Fish fish) {
        CurrentFish = fish;
    }

    public Fish GetCurrentFish() {
        return CurrentFish;
    }
}
