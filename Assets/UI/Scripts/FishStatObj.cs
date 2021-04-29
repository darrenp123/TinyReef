using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "fishStatObj")]
public class FishStatObj : ScriptableObject
{
    public int size;
    public int speed;
    public int sensoryRadius;
    public int lifespan;
    public int hunger;
}
