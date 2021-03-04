using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fish : MonoBehaviour
{

    string FishName, FishType;
    float Hunger, Desirability, Lifespan, Size, Speed, SensoryRadious, Camouflage;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    #region FishStats
    public void SetFishName(string name) {
        FishName = name;
    }

    public string GetFishName() {
        return FishName;
    }

    public void SetFishType(string type) {
        FishType = type;
    }

    public string GetFishType() {
        return FishType;
    }

    public void SetHunger(float hunger) {
        Hunger = hunger;
    }

    public float GetHunger() {
        return Hunger;
    }

    public void SetDesirability(float desire) {
        Desirability = desire;
    }

    public float GetDesirability() {
        return Desirability;
    }

    public void SetLifespan(float life) {
        Lifespan = life;
    }

    public float GetLifespan() {
        return Lifespan;
    }

    public void SetSize(float size) {
        Size = size;
    }

    public float GetSize() {
        return Size;
    }

    public void SetSpeed(float speed) {
        Speed = speed;
    }

    public float GetSpeed() {
        return Speed;
    }

    public void SetSensoryRadious(float sensoryRadious) {
        SensoryRadious = sensoryRadious;
    }

    public float GetSensoryRadious() {
        return SensoryRadious;
    }

    public void SetCamouflage(float camouflage) {
        Camouflage = camouflage;
    }

    public float GetCamouflage() {
        return Camouflage;
    }
    #endregion
}
