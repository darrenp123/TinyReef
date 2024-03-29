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
                if (objectiveFlock)
                {
                    objectiveFlock.OnFlockSizeChange += (flock) => OnObjectiveValueChange?.Invoke(flock);
                }
                break;
        }
    }

    public override bool ObjectiveComplete()
    {
        return objectiveFlock && state == GameModeSate.CHALLENGE &&  objectiveFlock.AllUnits.Count == numOfUnits;
    }
}
