
// this script controls how the species outliner works 
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpeciesOutliner : MonoBehaviour
{
    // References to the necessary UI elements needed for display
    [SerializeField] private TMP_Dropdown dropdown;
    [SerializeField] private VerticalLayoutGroup verticalLayoutGroup;
    [SerializeField] private SpeciesFishButton spiciesButtonPrefab;
    private SFlock[] _allFlocks;
    private List<SpeciesFishButton> buttons;

    [SerializeField]
    private CinemachineSwitcher switcher;
    [SerializeField]
    private FollowCamera followCamera;

    void Awake()
    {
        _allFlocks = FindObjectsOfType<SFlock>();
        List<string> flockNames = new List<string>(_allFlocks.Length);
        buttons = new List<SpeciesFishButton>();

        for (int i = 0; i < _allFlocks.Length; i++)
        {
            flockNames.Add(_allFlocks[i].FlockName);
        }

        // set the dropdown menu with all the names off the species present in that tank
        if (dropdown)
        {
            dropdown.AddOptions(flockNames);
            dropdown.onValueChanged.AddListener(FillSpiciesConteiner);
        }
    }

    private void Start()
    {
        FillSpiciesConteiner(0);
    }

    // update the container that displays all the fishes names from the selected type
    // it tries to reuse buttons already created for the display, by just setting them on and of and updating
    // them with the correct information. If the necessary buttons have not been created it makes new ones and
    // stores them for later uses 
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

    // updates the species confiner if the button clicked is invalid, if it isn't it tells the camera to follow 
    // the selected fish
    private void ViewSelectedFish(int buttonIndex)
    {
        List<SFlockUnit> currentFlock = _allFlocks[dropdown.value].AllUnits;

        if (buttonIndex >= currentFlock.Count)
        {
            FillSpiciesConteiner(dropdown.value);
            return;
        };

        if (switcher.freeCamera.GetCurrentState()) switcher.SwitchState();
        followCamera.FollowFish(currentFlock[buttonIndex]);
    }
}
