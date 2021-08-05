
// script to made set coral values and behavior
// Every game object that can be eaten by fish should use the interface IFood
// on the editor corals must have a collider and its layer must be set to the correct one, so fish are able to
// detected it
using UnityEngine;

public class Coral : MonoBehaviour, IFood
{
    [SerializeField] private float nutricionValue;

    public void Consume()
    {

    }

    public string GetFoodName() => "Coral";

    public float GetNutricionValue() => nutricionValue;
}
