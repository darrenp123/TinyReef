using Cinemachine;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static TMPro.TMP_Dropdown;

public class SpeciesOutliner : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown dropdown;
    [SerializeField] private VerticalLayoutGroup verticalLayoutGroup;
    [SerializeField] private SpeciesFishButton spiciesButtonPrefab;
    private SFlock[] _allFlocks;
    private List<SpeciesFishButton> buttons;

    // refactor later
    private CinemachineFreeLook _freeLookCamera;

    void Awake()
    {
        _allFlocks = FindObjectsOfType<SFlock>();
        List<string> flockNames = new List<string>(_allFlocks.Length);
        buttons = new List<SpeciesFishButton>();

        for (int i = 0; i < _allFlocks.Length; i++)
        {
            flockNames.Add(_allFlocks[i].FlockName);
        }

        if (dropdown)
        {
            dropdown.AddOptions(flockNames);
            DropdownEvent dropdownEvent = new DropdownEvent();
            dropdownEvent.AddListener(FillSpiciesConteiner);
            dropdown.onValueChanged = dropdownEvent;
        }
        // refactor later
        _freeLookCamera = FindObjectOfType<CinemachineFreeLook>();
    }

    private void Start()
    {
        FillSpiciesConteiner(0);
    }

    private void FillSpiciesConteiner(int flockIndex)
    {
        if (flockIndex >= _allFlocks.Length) return;

        List<SFlockUnit> currentFlock = _allFlocks[flockIndex].AllUnits;

        if (currentFlock.Count > buttons.Count)
        {
            for (int i = buttons.Count; i < currentFlock.Count; i++)
            {
                SpeciesFishButton button = Instantiate(spiciesButtonPrefab, verticalLayoutGroup.transform);
                button.FishIndex = i;
                button.SetOnClickCallBack(ViewSelectedFish);
                buttons.Add(button);
            }
        }
        else
        {
            for (int i = currentFlock.Count; i < buttons.Count; i++)
            {
                buttons[i].gameObject.SetActive(false);
            }
        }

        for (int i = 0; i < currentFlock.Count; i++)
        {
            SpeciesFishButton currentButton = buttons[i];
            currentButton.gameObject.SetActive(true);
            currentButton.SetDisplayNameText(currentFlock[i].UnitName);
        }
    }

    private void ViewSelectedFish(int buttonIndex)
    {
        List<SFlockUnit> currentFlock = _allFlocks[dropdown.value].AllUnits;

        if (buttonIndex >= currentFlock.Count)
        {
            FillSpiciesConteiner(dropdown.value);
            return;
        };

        // refactor later
        _freeLookCamera.m_Follow = currentFlock[buttonIndex].MyTransform;
        _freeLookCamera.m_LookAt = currentFlock[buttonIndex].MyTransform;
    }
}
