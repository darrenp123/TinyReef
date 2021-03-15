using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coral : MonoBehaviour, IFood
{
    public void Consume()
    {
        Debug.Log("coral ate");
    }
}
