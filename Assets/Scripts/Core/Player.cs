using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] private ParticleSystem bubblesPrefab;
    SFlockUnit CurrentFish;
    List<SFlockUnit> CurrentFishFlock;
    int CurrentFishFlockCount;
    [SerializeField] FishStatsUI FishStats;
    bool IsLookingAtFish;
    [SerializeField] int GenePoints;
    [SerializeField] int TraitCost;
    bool UION;
    public GameObject WinText;
    private GameModeBase _gameMode;
    private ConsumablePool _consumablePool;

    void Start()
    {
        IsLookingAtFish = false;
        CurrentFishFlockCount = 0;
        FishStats.UpdateGenePoints(GenePoints);
        UION = false;
        WinText.SetActive(false);
        _gameMode = FindObjectOfType<GameModeBase>();
        if (_gameMode && _gameMode.State == GameModeBase.GameModeSate.SANDBOX)
            InvokeRepeating(nameof(SanboxMode), 1, 1);

        _consumablePool = FindObjectOfType<ConsumablePool>();
    }

    private void SanboxMode()
    {
        UpdateGenePoints(1);
    }

    void Update()
    {
        YouWin();
    }

    public void YouWin()
    {
        if (_gameMode && _gameMode.ObjectiveComplete())
            WinText.SetActive(true);
    }

    public SFlockUnit GoToNextFishInFlock(bool dir)
    {
        if (dir)
        {
            //Left
            CurrentFishFlockCount--;
            if (CurrentFishFlockCount < 0) CurrentFishFlockCount = CurrentFishFlock.Count - 1;
        }
        else
        {
            //Right
            CurrentFishFlockCount++;
            if (CurrentFishFlockCount >= CurrentFishFlock.Count) CurrentFishFlockCount = 0;
        }
        SetCurrentFish(CurrentFishFlock[CurrentFishFlockCount]);
        return CurrentFishFlock[CurrentFishFlockCount];
    }

    void UpdateGenePoints(int updateValue)
    {
        GenePoints += updateValue;
        FishStats.UpdateGenePoints(GenePoints);
    }

    int GetGenePoints()
    {
        return GenePoints;
    }

    public void SeeFishStats(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (FishStats.IsFishStatsActive())
            {
                FishStats.TurnOnOffFishStats(false);
                UION = false;
                //FishStats.SetCurrentFish(null);
            }
            else if (IsLookingAtFish)
            {
                FishStats.TurnOnOffFishStats(true);
                UION = true;
                //FishStats.SetCurrentFish(CurrentFish);
            }
        }
    }

    public void IsLookingAtFishState(bool state, SFlockUnit fish)
    {
        if (state)
        {
            IsLookingAtFish = true;
            SetCurrentFish(fish);
            CurrentFishFlock = CurrentFish.GetComponentInParent<SFlock>().AllUnits;
        }
        else
        {
            SetCurrentFish(null);
            CurrentFishFlock = null;
            CurrentFishFlockCount = 0;
            IsLookingAtFish = false;
            UION = false;
            FishStats.TurnOnOffFishStats(false);
        }
    }

    public void SetCurrentFish(SFlockUnit fish)
    {
        CurrentFish = fish;
        FishStats.SetCurrentFish(fish);
    }

    public SFlockUnit GetCurrentFish()
    {
        return CurrentFish;
    }

    bool IsPurchasePossible()
    {
        return GenePoints >= TraitCost;
    }

    public bool IsUION()
    {
        return UION;
    }

    public void PurchasePoints(int trait, int value)
    {
        if (IsPurchasePossible())
        {
            DoBuyEffects();
            switch (trait)
            {
                //Lifespan
                case 1:
                    //TODO
                    float lifespan = CurrentFish.InitialLifespan / 60;
                    if (value > 0 && lifespan >= 1 && lifespan < 10 ||
                        value < 0 && lifespan > 1 && lifespan <= 10)
                    {
                        //CurrentFish.InitialLifespan += value*60;
                        UpdateGenePoints(-TraitCost);
                    }
                    break;
                //Size
                case 2:
                    float size = CurrentFish.Size;
                    if (value > 0 && size >= 1 && size < 10 ||
                        value < 0 && size > 1 && size <= 10)
                    {
                        CurrentFish.Size += value;
                        CurrentFish.ScaleFish();
                        UpdateGenePoints(-TraitCost);
                    }
                    break;
                //Speed
                case 3:
                    int speed = Mathf.RoundToInt(CurrentFish.MaxSpeed);
                    if (value > 0 && speed >= 1 && speed < 10 ||
                        value < 0 && speed > 1 && speed <= 10)
                    {
                        CurrentFish.SetMaxSpeed(value);
                        UpdateGenePoints(-TraitCost);
                    }
                    break;
                //SensoryRadious
                case 4:
                    int sensoryRadious = Mathf.RoundToInt(CurrentFish.SightDistance);
                    if (value > 0 && sensoryRadious >= 1 && sensoryRadious < 10 ||
                        value < 0 && sensoryRadious > 1 && sensoryRadious <= 10)
                    {
                        CurrentFish.SetSightDistance(value);
                        UpdateGenePoints(-TraitCost);
                    }
                    break;
                //Camouflage
                case 5:
                    //TODO
                    /*
                    float camouflage = CurrentFish.GetCamouflage();
                    if (value > 0 && camouflage >= 1 && camouflage < 10 ||
                        value < 0 && camouflage > 1 && camouflage <= 10) {
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

    private void DoBuyEffects()
    {
        _ = StartCoroutine(ManageConsumableEffect());

        // To remove
        //if (!bubblesPrefab) return;

        //foreach (ParticleSystem bubbleEffect in _bubblesEffectPool)
        //{
        //    if (!bubbleEffect.isPlaying)
        //    {
        //        bubbleEffect.transform.position = CurrentFish.transform.position;
        //        bubbleEffect.transform.parent = CurrentFish.transform;
        //        bubbleEffect.Play();
        //        return;
        //    }
        //}

        //ParticleSystem newBubbleEffect = Instantiate(bubblesPrefab, CurrentFish.transform.position, Quaternion.identity, CurrentFish.transform);
        //newBubbleEffect.Play();
        //_bubblesEffectPool.Add(newBubbleEffect);
    }

    private IEnumerator ManageConsumableEffect()
    {
        if (!_consumablePool) yield break;

        ParticleSystem bubbleEffect = _consumablePool.GetItemFromPool(ItemPool.BUBBLES_BURST, bubblesPrefab)
            .GetComponent<ParticleSystem>();

        bubbleEffect.transform.position = CurrentFish.transform.position;
        bubbleEffect.transform.parent = CurrentFish.transform;
        bubbleEffect.gameObject.SetActive(true);
        bubbleEffect.Play();

        yield return new WaitWhile(() => bubbleEffect.isPlaying);

        _consumablePool.ReturnToPool(ItemPool.BUBBLES_BURST, bubbleEffect.gameObject);
    }
}
