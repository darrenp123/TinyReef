// Game mode class used for the game, inherits from game mode base
// meant to evaluate the size of the chosen fish species 
using UnityEngine;

public class ReefGameMode : GameModeBase
{
    [SerializeField] private SFlock objectiveFlock;
    [SerializeField] private float numOfUnits;

    void Awake()
    {
        // initial set up based on state
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
                    // setting a function to run whenever the chosen "flock" changes in size
                    objectiveFlock.OnFlockSizeChange += flock => OnObjectiveValueChange?.Invoke(flock);
                }
                break;
        }
    }

    public override bool ObjectiveComplete()
    {
        return objectiveFlock && state == GameModeSate.CHALLENGE &&  objectiveFlock.AllUnits.Count == numOfUnits;
    }
}
