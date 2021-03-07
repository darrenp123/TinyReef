using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;


public class FishStatsUI : MonoBehaviour
{

    Fish CurrentFish;
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

    public void SetCurrentFish(Fish fish) {
        CurrentFish = fish;
        UpdateAllFishStats();
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
        FishName.text = CurrentFish.GetFishName();
        Fishtype.text = CurrentFish.GetFishType();
        UpdateChangeableStats(CurrentFish);
    }

    void UpdateChangeableStats(Fish fish) {
        UpdateHunger(fish);
        UpdateDesirability(fish);
        UpdateLifespan(fish);
        UpdateSize(fish);
        UpdateSpeed(fish);
        UpdateSensoryRadious(fish);
        UpdateCamouflage(fish);
    }

    void UpdateHunger(Fish fish) {
        float value = fish.GetHunger();
        Hunger.text = value.ToString();
        HungerSlider.value = value;
    }

    void UpdateDesirability(Fish fish) {
        float value = fish.GetDesirability();
        Desirability.text = value.ToString();
        DesirabilitySlider.value = value;
    }

    void UpdateLifespan(Fish fish) {
        float value = fish.GetLifespan();
        Lifespan.text = value.ToString();
        LifespanSlider.value = value;
    }

    void UpdateSize(Fish fish) {
        float value = fish.GetSize();
        Size.text = value.ToString();
        SizeSlider.value = value;
    }

    void UpdateSpeed(Fish fish) {
        float value = fish.GetSpeed();
        Speed.text = value.ToString();
        SpeedSlider.value = value;
    }

    void UpdateSensoryRadious(Fish fish) {
        float value = fish.GetSensoryRadious();
        SensoryRadious.text = value.ToString();
        SensoryRaidousSlider.value = value;
    }

    void UpdateCamouflage(Fish fish) {
        float value = fish.GetCamouflage();
        Camouflage.text = value.ToString();
        CamouflageSlider.value = value;
    }

    public void UpdateGenePoints(int points) {
        GenePoints.text = points.ToString();
    }
    #endregion
}
