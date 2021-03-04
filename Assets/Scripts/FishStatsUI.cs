using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FishStatsUI : MonoBehaviour
{

    Fish CurrentFish;
    [SerializeField]
    GameObject FishStats;
    [SerializeField]
    TextMesh FishName;
    [SerializeField]
    TextMesh Fishtype;
    [SerializeField]
    TextMesh Hunger;
    [SerializeField]
    TextMesh Desirability;
    [SerializeField]
    TextMesh Lifespan;
    [SerializeField]
    TextMesh Size;
    [SerializeField]
    TextMesh Speed;
    [SerializeField]
    TextMesh SensoryRadious;
    [SerializeField]
    TextMesh Camouflage;

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
        UpdateDesirability(fish);
        UpdateLifespan(fish);
        UpdateSize(fish);
        UpdateSpeed(fish);
        UpdateSensoryRadious(fish);
        UpdateCamouflage(fish);
    }

    void UpdateDesirability(Fish fish) {
        Desirability.text = fish.GetDesirability().ToString();
    }

    void UpdateLifespan(Fish fish) {
        Lifespan.text = fish.GetLifespan().ToString();
    }

    void UpdateSize(Fish fish) {
        Size.text = fish.GetSize().ToString();
    }

    void UpdateSpeed(Fish fish) {
        Speed.text = fish.GetSpeed().ToString();
    }

    void UpdateSensoryRadious(Fish fish) {
        SensoryRadious.text = fish.GetSensoryRadious().ToString();
    }

    void UpdateCamouflage(Fish fish) {
        Camouflage.text = fish.GetCamouflage().ToString();
    }
    #endregion
}
