using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishStatsUI : MonoBehaviour
{

    GameObject FishStats;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void TurnOnOffFishStats(bool state) {
        if (state) {
            FishStats.SetActive(true);
        } else {
            FishStats.SetActive(false);
        }
    }
}
