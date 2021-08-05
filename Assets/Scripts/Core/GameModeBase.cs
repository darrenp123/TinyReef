// class used to create game mode, if you want to create a game mode class you should inherit from this class
// and implement all its virtual methods
using UnityEngine;

public class GameModeBase : MonoBehaviour
{
    public enum GameModeSate
    {
        DEFAULT,
        SANDBOX,
        CHALLENGE
    }

    public delegate void OnObjectiveValueChangeSignature(SFlock Objectiveflock);

    [SerializeField] protected string objectiveDescription;
    public OnObjectiveValueChangeSignature OnObjectiveValueChange;
    protected static GameModeSate state;

    public GameModeSate State { get => state; set => state = value; }
    public string ObjectiveDescription => objectiveDescription;

    public virtual bool ObjectiveComplete() => false;
}
