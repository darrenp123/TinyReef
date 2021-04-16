using TMPro;
using UnityEngine;

public class ObjevtiveUI : MonoBehaviour
{
    [SerializeField] private TMP_Text objectiveText;
    private void Start()
    {
        var gameMode = FindObjectOfType<GameModeBase>();

        if (objectiveText && gameMode)
        {
            objectiveText.SetText(gameMode.ObjectiveDescription);
            if(gameMode.State == GameModeBase.GameModeSate.SANDBOX)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
