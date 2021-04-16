using UnityEngine;

public class ReefGameMode : GameModeBase
{
    [SerializeField] private SFlock objectiveFlock;
    [SerializeField] private float numOfUnits;

    void Awake()
    {
        switch (state)
        {
            case GameModeSate.DEFAULT:
                break;
            case GameModeSate.SANDBOX:
                objectiveDescription = "";
                break;
            case GameModeSate.CHALLENGE:
                break;
        }
    }

    public override bool ObjectiveComplete()
    {
        return state == GameModeSate.CHALLENGE &&  objectiveFlock.AllUnits.Count > numOfUnits;
    }
}
