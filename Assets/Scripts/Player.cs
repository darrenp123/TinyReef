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
        FishStats.UpdateGenePoints(GenePoints);
    }

    // Update is called once per frame
    void Update()
    {
        Timer += Time.deltaTime;
        if(Timer >= 2f) {
            UpdateGenePoints(1);
            Timer = 0f;
        }
    }

    void UpdateGenePoints(int updateValue) {
        GenePoints += updateValue;
        FishStats.UpdateGenePoints(GenePoints);
    }

    int GetGenePoints() {
        return GenePoints;
    }

    public void SeeFishStats(InputAction.CallbackContext context) {
        if (context.performed) {
            if (FishStats.IsFishStatsActive()) {
                FishStats.TurnOnOffFishStats(false);
                //FishStats.SetCurrentFish(null);
            } else if (IsLookingAtFish) {
                FishStats.TurnOnOffFishStats(true);
                //FishStats.SetCurrentFish(CurrentFish);
            }
        }
    }

    public void IsLookingAtFishState(bool state, Fish fish) {
        if (state) {
            IsLookingAtFish = true;
            SetCurrentFish(fish);
        } else {
            SetCurrentFish(null);
            IsLookingAtFish = false;
            FishStats.TurnOnOffFishStats(false);
        }
    }

    public void SetCurrentFish(Fish fish) {
        CurrentFish = fish;
        FishStats.SetCurrentFish(fish);
    }

    public Fish GetCurrentFish() {
        return CurrentFish;
    }

    bool IsPurchasePossible() {
        return GenePoints >= 5;
    }

    public void PurchasePoints(int trait, int value) {
        if (IsPurchasePossible()) {
            switch (trait) {
                //Lifespan
                case 1:
                    if(CurrentFish.GetLifespan() > 0 && CurrentFish.GetLifespan() < 10) {
                        CurrentFish.SetLifespan(value);
                        UpdateGenePoints(-5);
                    }         
                    break;
                //Size
                case 2:
                    if (CurrentFish.GetSize() > 0 && CurrentFish.GetSize() < 10) { 
                        CurrentFish.SetSize(value);
                        UpdateGenePoints(-5);
                    }
            break;
                //Speed
                case 3:
                    if (CurrentFish.GetSpeed() > 0 && CurrentFish.GetSpeed() < 10) {
                        CurrentFish.SetSpeed(value);
                        UpdateGenePoints(-5);
                    }
                    break;
                //SensoryRadious
                case 4:
                    if (CurrentFish.GetSensoryRadious() > 0 && CurrentFish.GetSensoryRadious() < 10) {
                        CurrentFish.SetSensoryRadious(value);
                        UpdateGenePoints(-5);
                    }
                    break;
                //Camouflage
                case 5:
                    if (CurrentFish.GetCamouflage() > 0 && CurrentFish.GetCamouflage() < 10) {
                        CurrentFish.SetCamouflage(value);
                        UpdateGenePoints(-5);
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
