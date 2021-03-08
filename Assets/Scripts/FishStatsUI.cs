using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;


public class FishStatsUI : MonoBehaviour
{

    SFlockUnit CurrentFish;
    [SerializeField]
    GameObject FishStats;
    [SerializeField]
    TMP_Text FishName;
    [SerializeField]
    TMP_Text Fishtype;
    [SerializeField]
    TMP_Text Hunger;
    [SerializeField]
    Slider HungerSlider;
    [SerializeField]
    TMP_Text Desirability;
    [SerializeField]
    Slider DesirabilitySlider;
    [SerializeField]
    TMP_Text Lifespan;
    [SerializeField]
    Slider LifespanSlider;
    [SerializeField]
    TMP_Text Size;
    [SerializeField]
    Slider SizeSlider;
    [SerializeField]
    TMP_Text Speed;
    [SerializeField]
    Slider SpeedSlider;
    [SerializeField]
    TMP_Text SensoryRadious;
    [SerializeField]
    Slider SensoryRaidousSlider;
    [SerializeField]
    TMP_Text Camouflage;
    [SerializeField]
    Slider CamouflageSlider;
    [SerializeField]
    TMP_Text GenePoints;

    // Start is called before the first frame update
    void Start()
    {
        FishStats.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (FishStats.activeSelf) {
            UpdateChangeableStats(CurrentFish);
        }
    }

    public void SetCurrentFish(SFlockUnit fish) {
        CurrentFish = fish;
        if(fish != null) UpdateAllFishStats();
    }
    public bool IsFishStatsActive() {
        return FishStats.activeSelf;
    }

    public void TurnOnOffFishStats(bool state) {
        if (state) {
            FishStats.SetActive(true);
        } else {
            FishStats.SetActive(false);
        }
    }

    #region UpdateUI
    public void UpdateAllFishStats() {
        FishName.text = CurrentFish.UnitName;
        Fishtype.text = CurrentFish.UnitType;
        UpdateChangeableStats(CurrentFish);
    }

    void UpdateChangeableStats(SFlockUnit fish) {
        UpdateHunger(fish);
        UpdateDesirability(fish);
        UpdateLifespan(fish);
        UpdateSize(fish);
        UpdateSpeed(fish);
        UpdateSensoryRadious(fish);
        UpdateCamouflage(fish);
    }

    void UpdateHunger(SFlockUnit fish) {
        float value = Mathf.RoundToInt(fish.CurrrentHunger);
        if (value < 0) value = 0;
        Hunger.text = value.ToString();
        HungerSlider.value = value;
    }

    void UpdateDesirability(SFlockUnit fish) {
        //TODO
        /*
        float value = fish.GetDesirability();
        Desirability.text = value.ToString();
        DesirabilitySlider.value = value;
        */
    }

    void UpdateLifespan(SFlockUnit fish) {
        //TODO
        /*
        float value = fish.GetLifespan();
        Lifespan.text = value.ToString();
        LifespanSlider.value = value;
        */
    }

    void UpdateSize(SFlockUnit fish) {
        float value = fish.Size;
        Size.text = value.ToString();
        SizeSlider.value = value;
    }

    void UpdateSpeed(SFlockUnit fish) {
        int value = Mathf.RoundToInt(fish.MaxSpeed);
        Speed.text = value.ToString();
        SpeedSlider.value = value;
    }

    void UpdateSensoryRadious(SFlockUnit fish) {
        float value = Mathf.RoundToInt(fish.SightDistance);
        SensoryRadious.text = value.ToString();
        SensoryRaidousSlider.value = value;
    }

    void UpdateCamouflage(SFlockUnit fish) {
        //TODO
        /*
        float value = fish.GetCamouflage();
        Camouflage.text = value.ToString();
        CamouflageSlider.value = value;
        */
    }

    public void UpdateGenePoints(int points) {
        GenePoints.text = points.ToString();
    }
    #endregion
}
