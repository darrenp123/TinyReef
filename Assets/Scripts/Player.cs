using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{

    SFlockUnit CurrentFish;
    List<SFlockUnit> CurrentFishFlock;
    int CurrentFishFlockCount;
    [SerializeField]
    FishStatsUI FishStats;
    bool IsLookingAtFish;
    [SerializeField]
    int GenePoints;
    [SerializeField]
    int TraitCost;
    float Timer;
    bool UION;
    public SFlock GroupOfFish;
    public GameObject WinText;


    // Start is called before the first frame update
    void Start()
    {
        IsLookingAtFish = false;
        CurrentFishFlockCount = 0;
        Timer = 0f;
        FishStats.UpdateGenePoints(GenePoints);
        UION = false;
        WinText.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
           /*
        Timer += Time.deltaTime;
        if(Timer >= 1f) {
            UpdateGenePoints(1);
            Timer = 0f;
        }
           */
        YouWin();
    }

    //Temp
    public void YouWin() {
        if (GroupOfFish.AllUnits.Count > 15) WinText.SetActive(true);
    }

    public SFlockUnit GoToNextFishInFlock(bool dir) {
        if (dir) {
            //Left
            CurrentFishFlockCount--;
            if (CurrentFishFlockCount < 0) CurrentFishFlockCount = CurrentFishFlock.Count - 1;
        } else {
            //Right
            CurrentFishFlockCount++;
            if (CurrentFishFlockCount >= CurrentFishFlock.Count) CurrentFishFlockCount = 0;
        }
        SetCurrentFish(CurrentFishFlock[CurrentFishFlockCount]);
        return CurrentFishFlock[CurrentFishFlockCount];
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
                UION = false;
                //FishStats.SetCurrentFish(null);
            } else if (IsLookingAtFish) {
                FishStats.TurnOnOffFishStats(true);
                UION = true;
                //FishStats.SetCurrentFish(CurrentFish);
            }
        }
    }

    public void IsLookingAtFishState(bool state, SFlockUnit fish) {
        if (state) {
            IsLookingAtFish = true;
            SetCurrentFish(fish);
            CurrentFishFlock = CurrentFish.GetComponentInParent<SFlock>().AllUnits;
        } else {
            SetCurrentFish(null);
            CurrentFishFlock = null;
            CurrentFishFlockCount = 0;
            IsLookingAtFish = false;
            UION = false;
            FishStats.TurnOnOffFishStats(false);
        }
    }

    public void SetCurrentFish(SFlockUnit fish) {
        CurrentFish = fish;
        FishStats.SetCurrentFish(fish);
    }

    public SFlockUnit GetCurrentFish() {
        return CurrentFish;
    }

    bool IsPurchasePossible() {
        return GenePoints >= TraitCost;
    }

    public bool IsUION() {
        return UION;
    }

    public void PurchasePoints(int trait, int value) {
        if (IsPurchasePossible()) {
            switch (trait) {
                //Lifespan
                case 1:
                    //TODO
                    float lifespan = CurrentFish.InitialLifespan/60;
                    if (value > 0 && lifespan >= 0 && lifespan < 10 || 
                        value < 0 && lifespan > 0 && lifespan <= 10) {
                        CurrentFish.InitialLifespan += value*60;
                        UpdateGenePoints(-TraitCost);
                    }  
                    break;
                //Size
                case 2:
                    float size = CurrentFish.Size;
                    if (value > 0 && size >= 1 && size < 10 ||
                        value < 0 && size > 1 && size <= 10) {
                        CurrentFish.Size += value;
                        CurrentFish.ScaleFish();
                        UpdateGenePoints(-TraitCost);
                    }
            break;
                //Speed
                case 3:
                    int speed = Mathf.RoundToInt(CurrentFish.MaxSpeed);
                    if (value > 0 && speed >= 1 && speed < 10 ||
                        value < 0 && speed > 0 && speed <= 10) {
                        CurrentFish.SetMaxSpeed(value);
                        UpdateGenePoints(-TraitCost);
                    }
                    break;
                //SensoryRadious
                case 4:
                    int sensoryRadious = Mathf.RoundToInt(CurrentFish.SightDistance);
                    if (value > 0 && sensoryRadious >= 0 && sensoryRadious < 10 ||
                        value < 0 && sensoryRadious > 0 && sensoryRadious <= 10) {
                        CurrentFish.SetSightDistance(value);
                        UpdateGenePoints(-TraitCost);
                    }
                    break;
                //Camouflage
                case 5:
                    //TODO
                    /*
                    float camouflage = CurrentFish.GetCamouflage();
                    if (value > 0 && camouflage >= 0 && camouflage < 10 ||
                        value < 0 && camouflage > 0 && camouflage <= 10) {
                        CurrentFish.SetCamouflage(value);
                        UpdateGenePoints(-TraitCost);
                    }
                    */
                    break;
                //MatingUrge
                case 6:

                    break;
                //GestationPeriod
                case 7:

                    break;
                default:
                    break;
            }
        }
    }
}
