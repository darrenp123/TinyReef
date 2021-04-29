using System.Collections;
using System.Collections.Generic;
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
