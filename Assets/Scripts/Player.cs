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
    [SerializeField]
    int TraitCost;
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
        if(Timer >= 1f) {
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
        return GenePoints >= TraitCost;
    }

    public void PurchasePoints(int trait, int value) {
        if (IsPurchasePossible()) {
            switch (trait) {
                //Lifespan
                case 1:
                    float lifespan = CurrentFish.GetLifespan();
                    if (value > 0 && lifespan >= 0 && lifespan < 10 || 
                        value < 0 && lifespan > 0 && lifespan <= 10) {
                        CurrentFish.SetLifespan(value);
                        UpdateGenePoints(-TraitCost);
                    }         
                    break;
                //Size
                case 2:
                    float size = CurrentFish.GetSize();
                    if (value > 0 && size >= 0 && size < 10 ||
                        value < 0 && size > 0 && size <= 10) {
                        CurrentFish.SetSize(value);
                        UpdateGenePoints(-TraitCost);
                    }
            break;
                //Speed
                case 3:
                    float speed = CurrentFish.GetSpeed();
                    if (value > 0 && speed >= 0 && speed < 10 ||
                        value < 0 && speed > 0 && speed <= 10) {
                        CurrentFish.SetSpeed(value);
                        UpdateGenePoints(-TraitCost);
                    }
                    break;
                //SensoryRadious
                case 4:
                    float sensoryRadious = CurrentFish.GetSensoryRadious();
                    if (value > 0 && sensoryRadious >= 0 && sensoryRadious < 10 ||
                        value < 0 && sensoryRadious > 0 && sensoryRadious <= 10) {
                        CurrentFish.SetSensoryRadious(value);
                        UpdateGenePoints(-TraitCost);
                    }
                    break;
                //Camouflage
                case 5:
                    float camouflage = CurrentFish.GetCamouflage();
                    if (value > 0 && camouflage >= 0 && camouflage < 10 ||
                        value < 0 && camouflage > 0 && camouflage <= 10) {
                        CurrentFish.SetCamouflage(value);
                        UpdateGenePoints(-TraitCost);
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
