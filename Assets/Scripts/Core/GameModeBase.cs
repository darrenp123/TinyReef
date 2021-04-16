using UnityEngine;

public class GameModeBase : MonoBehaviour
{
    public enum GameModeSate
    {
        DEFAULT,
        SANDBOX,
        CHALLENGE
    }

    [SerializeField] protected string objectiveDescription;
    protected static GameModeSate state;

    public GameModeSate State { get => state; set => state = value; }
    public string ObjectiveDescription => objectiveDescription;

    public virtual bool ObjectiveComplete() => false;
}
