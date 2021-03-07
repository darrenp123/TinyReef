using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OldFlock : MonoBehaviour
{
    [Header("Spawn Setup")]
    [SerializeField] private OldFlockUnit flockUnitPrefab;
    [SerializeField] private int flockSize;
    [SerializeField] private Vector3 spawnBounds;

    [Header("Detection Distances")]
    [Range(0, 10)]
    [SerializeField] private float minSpeed;
    [Range(0, 10)]
    [SerializeField] private float maxSpeed;

    [Header("Detection Distances")]
    [Range(0, 10)]
    [SerializeField] private float cohesionDistance;
    [Range(0, 10)]
    [SerializeField] private float avoidanceDistance;
    [Range(0, 10)]
    [SerializeField] private float alignmentDistance;
    [Range(0, 10)]
    [SerializeField] private float obstacleAvoidanceDistance;
    [Range(0, 100)]
    [SerializeField] private float boundsDistance;

    [Header("Behaviour Weights")]
    [Range(0, 10)]
    [SerializeField] private float cohesionWeight;
    [Range(0, 10)]
    [SerializeField] private float avoidanceWeight;
    [Range(0, 10)]
    [SerializeField] private float alignmentWeight;
    [Range(0, 100)]
    [SerializeField] private float obstacleAvoidanceWeight;
    [Range(0, 10)]
    [SerializeField] private float boundsWeight;

    public OldFlockUnit[] AllUnits { get; set; }

    public float MinSpeed => minSpeed;
    public float MaxSpeed => maxSpeed;
    public float CohesionDistance => cohesionDistance;
    public float AvoidanceDistance => avoidanceDistance;
    public float AlignmentDistance => alignmentDistance;
    public float CohesionWeight => cohesionWeight;
    public float AvoidanceWeight => avoidanceWeight;
    public float AlignmentWeight => alignmentWeight;
    public float BoundsDistance => boundsDistance;
    public float BoundsWeight => boundsWeight;
    public float ObstacleAvoidanceDistance => obstacleAvoidanceDistance;
    public float ObstacleAvoidanceWeight => obstacleAvoidanceWeight;

    void Start()
    {
        GenarateUnits();
    }

    private void Update()
    {
        foreach (OldFlockUnit Unit in AllUnits)
        {
            Unit.MoveUnit();
        }
    }

    private void GenarateUnits()
    {
        AllUnits = new OldFlockUnit[flockSize];
        for (int i = 0; i < flockSize; i++)
        {
            Vector3 ramdomVector = Vector3.Scale(UnityEngine.Random.insideUnitSphere, spawnBounds);

            OldFlockUnit newUnit = Instantiate(flockUnitPrefab, transform.position + ramdomVector,
                Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0), transform);
            AllUnits[i] = newUnit;
            newUnit.AssignedFlock = this;
            newUnit.Speed = UnityEngine.Random.Range(minSpeed, maxSpeed);
        }
    }
}
