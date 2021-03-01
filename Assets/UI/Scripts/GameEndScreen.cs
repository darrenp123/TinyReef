using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameEndScreen : MonoBehaviour
{
    [SerializeField]
    TMP_Dropdown speciesDropdown, traitDropdown;
    [SerializeField]
    TMP_Text speciesText, traitText;
    [SerializeField]
    GameObject graph, statisticsMenu, endScreen;

    string currentSelectedSpecies, currentSelectedTrait;
    // Start is called before the first frame update
    void Start()
    {
        currentSelectedSpecies = speciesDropdown.options[traitDropdown.value].text;
        currentSelectedTrait = traitDropdown.options[traitDropdown.value].text;
        DisplayGraph(currentSelectedSpecies, currentSelectedTrait);
    }

    public void OnSpeciesSelected()
    {
        currentSelectedSpecies = speciesDropdown.options[speciesDropdown.value].text;
        DisplayGraph(currentSelectedSpecies, currentSelectedTrait);
    }

    public void OnTraitSelected(string traitSelected)
    {
        currentSelectedTrait = traitDropdown.options[traitDropdown.value].text;
        DisplayGraph(currentSelectedSpecies, currentSelectedTrait);
    }

    void DisplayGraph(string species, string trait)
    {
        graph.SetActive(true);
        speciesText.text = species;
        traitText.text = trait;
    }

    public void ShowStatisticsScreen()
    {
        statisticsMenu.SetActive(true);
    }

    public void CloseStatisticsScreen()
    {
        statisticsMenu.SetActive(false);
    }

    public void ShowGameEndScreen()
    {
        endScreen.SetActive(true);
    }
}
