using TMPro;
using UnityEngine;

public class ObjevtiveUI : MonoBehaviour
{
    [SerializeField] private TMP_Text objectiveText;
    private GameModeBase _gameMode;
    
    private void Awake()
    {
        _gameMode = FindObjectOfType<GameModeBase>();

        if (objectiveText && _gameMode)
        {
            if (_gameMode.State == GameModeBase.GameModeSate.SANDBOX)
            {
                gameObject.SetActive(false);
            }
            else if(_gameMode.State == GameModeBase.GameModeSate.CHALLENGE)
            {
                objectiveText.SetText(_gameMode.ObjectiveDescription);
                _gameMode.OnObjectiveValueChange += UpdateFlockUnitsNumber;
            }
        }
    }

    private void UpdateFlockUnitsNumber(SFlock flock)
    {
        objectiveText.SetText(_gameMode.ObjectiveDescription + "\n" + "The current number of " + flock.FlockName + " is " + flock.AllUnits.Count);
    }
}
