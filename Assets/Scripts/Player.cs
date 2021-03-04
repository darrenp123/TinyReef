using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{

    Fish CurrentFish;
    [SerializeField]
    FishStatsUI FishStats;
    bool IsLookingAtFish;
    int GenePoints;
    float Timer;


    // Start is called before the first frame update
    void Start()
    {
        IsLookingAtFish = false;
        Timer = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        Timer += Time.deltaTime;
        if(Timer >= 120f) {
            GenePoints++;
            Timer = 0f;
        }
    }

    public void UpdateGenePoints(int updateValue) {
        GenePoints = updateValue;
    }

    public void SeeFishStats(InputAction.CallbackContext context) {
        if (context.performed) {
            if (FishStats.IsFishStatsActive()) {
                FishStats.TurnOnOffFishStats(false);
                FishStats.SetCurrentFish(null);
            } else if (IsLookingAtFish) {
                FishStats.TurnOnOffFishStats(true);
                FishStats.SetCurrentFish(CurrentFish);
            }
        }
    }

    public void IsLookingAtFishState(bool state, Fish fish) {
        if (state) {
            IsLookingAtFish = true;
            CurrentFish = fish;
        } else {
            CurrentFish = null;
            IsLookingAtFish = false;
            FishStats.TurnOnOffFishStats(false);
            FishStats.SetCurrentFish(null);
        }
    }

    public void SetCurrentFish(Fish fish) {
        CurrentFish = fish;
    }

    public Fish GetCurrentFish() {
        return CurrentFish;
    }
}
