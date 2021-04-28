using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.UI.Button;

public class SpeciesFishButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI fishNameText;
    // might keep reference to fish unit instead of index
    private int _fishIndex;
    private Action<int> _onClickButton;
    private Button _button;

    public int FishIndex { get => _fishIndex; set => _fishIndex = value; }

    public void SetDisplayNameText(string name) => fishNameText.text = name;

    public void SetOnClickCallBack(Action<int> callBack) => _onClickButton = callBack;

    private void Start()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(() => _onClickButton?.Invoke(_fishIndex));
    }
}
