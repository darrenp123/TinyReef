using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using UnityEngine.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;

public class Flock : MonoBehaviour
{
    [Header("Spawn Setup")]
    [SerializeField] private FlockUinit flockUnitPrefab;
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

    public FlockUinit[] AllUnits { get; set; }

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

    private void GenarateUnits()
    {
        AllUnits = new FlockUinit[flockSize];
        for (int i = 0; i < flockSize; i++)
        {
            Vector3 ramdomVector = Vector3.Scale(UnityEngine.Random.insideUnitSphere, spawnBounds);

            FlockUinit newUnit = Instantiate(flockUnitPrefab, transform.position + ramdomVector,
                Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0), transform);
            AllUnits[i] = newUnit;
            newUnit.AssignedFlock = this;
            newUnit.Speed = UnityEngine.Random.Range(minSpeed, maxSpeed);
        }
    }

    private void Update()
    {
        //foreach (FlockUinit Unit in AllUnits)
        //{
        //    Unit.MoveUnit();
        //}

        NativeArray<Vector3> unitsForwardDirection = new NativeArray<Vector3>(AllUnits.Length, Allocator.TempJob);
        NativeArray<Vector3> unitsVelocity = new NativeArray<Vector3>(AllUnits.Length, Allocator.TempJob);
        NativeArray<Vector3> unitsPosition = new NativeArray<Vector3>(AllUnits.Length, Allocator.TempJob);
        NativeArray<Vector3> cohesionNeighbors = new NativeArray<Vector3>(AllUnits.Length, Allocator.TempJob);
        NativeArray<Vector3> avoidanceNeighbors = new NativeArray<Vector3>(AllUnits.Length, Allocator.TempJob);
        NativeArray<Vector3> alignmentNeighbors = new NativeArray<Vector3>(AllUnits.Length, Allocator.TempJob);
        NativeArray<Vector3> alignmentNeighborsDirections = new NativeArray<Vector3>(AllUnits.Length, Allocator.TempJob);
        NativeArray<float> unitsSpeed = new NativeArray<float>(AllUnits.Length, Allocator.TempJob);
        NativeArray<float> neighborsSpeed = new NativeArray<float>(AllUnits.Length, Allocator.TempJob);

        for (int i = 0; i < AllUnits.Length; i++)
        {
            unitsForwardDirection[i] = AllUnits[i].MyTransform.forward;
            unitsVelocity[i] = AllUnits[i].CurrentVelocity;
            unitsPosition[i] = AllUnits[i].MyTransform.position;
            cohesionNeighbors[i] = Vector3.zero;
            avoidanceNeighbors[i] = Vector3.zero;
            alignmentNeighbors[i] = Vector3.zero;
            alignmentNeighborsDirections[i] = Vector3.zero;
            unitsSpeed[i] = AllUnits[i].Speed;
            neighborsSpeed[i] = 0.0f;
        }

        MoveJob moveJob = new MoveJob
        {
            UnitsForwardDirection = unitsForwardDirection,
            UnitsVelocity = unitsVelocity,
            UnitsPosition = unitsPosition,
            CohesionNeighbors = cohesionNeighbors,
            AvoidanceNeighbors = avoidanceNeighbors,
            AlignmentNeighbors = alignmentNeighbors,
            AlignmentNeighborsDirections = alignmentNeighborsDirections,
            UnitsSpeed = unitsSpeed,
            NeighborsSpeed = neighborsSpeed,
            FlockPosition = transform.position,
            CohesionDistance = cohesionDistance,
            AvoidanceDistance = avoidanceDistance,
            AlignmentDistance = alignmentDistance,
            ObstacleAvoidanceDistance = obstacleAvoidanceDistance,
            BoundsDistance = boundsDistance,
            CohesionWeight = cohesionWeight,
            AvoidanceWeight = avoidanceWeight,
            AlignmentWeight = alignmentWeight,
            ObstacleAvoidanceWeight = obstacleAvoidanceWeight,
            BoundsWeight = boundsWeight,
            FOVAngle = flockUnitPrefab.GetFOVAngle,
            MinSpeed = minSpeed,
            MaxSpeed = maxSpeed,
            SmoothDamp = flockUnitPrefab.SmoothDamp,
            DeltaTime = Time.deltaTime
        };

        JobHandle handle = moveJob.Schedule(AllUnits.Length, 5);
        handle.Complete();

        for (int i = 0; i < AllUnits.Length; i++)
        {
            // Debug.Log(unitsForwardDirection[i].ToString() + " " + " " + unitsVelocity[i].ToString() + " " + unitsPosition[i].ToString() + " " + unitsSpeed[i]);
            AllUnits[i].MyTransform.forward = moveJob.UnitsForwardDirection[i];
            AllUnits[i].MyTransform.position = moveJob.UnitsPosition[i];
            AllUnits[i].CurrentVelocity = moveJob.UnitsVelocity[i];
            AllUnits[i].Speed = moveJob.UnitsSpeed[i];
        }

        unitsForwardDirection.Dispose();
        unitsVelocity.Dispose();
        unitsPosition.Dispose();
        cohesionNeighbors.Dispose();
        avoidanceNeighbors.Dispose();
        alignmentNeighbors.Dispose();
        alignmentNeighborsDirections.Dispose();
        unitsSpeed.Dispose();
        neighborsSpeed.Dispose();
    }
}

[BurstCompile]
public struct MoveJob : IJobParallelFor
{
    [NativeDisableParallelForRestriction]
    public NativeArray<Vector3> UnitsForwardDirection;
    [NativeDisableParallelForRestriction]
    public NativeArray<Vector3> UnitsVelocity;
    [NativeDisableParallelForRestriction]
    public NativeArray<Vector3> UnitsPosition;
    [NativeDisableParallelForRestriction]
    public NativeArray<Vector3> CohesionNeighbors;
    [NativeDisableParallelForRestriction]
    public NativeArray<Vector3> AvoidanceNeighbors;
    [NativeDisableParallelForRestriction]
    public NativeArray<Vector3> AlignmentNeighbors;
    [NativeDisableParallelForRestriction]
    public NativeArray<Vector3> AlignmentNeighborsDirections;
    [NativeDisableParallelForRestriction]
    public NativeArray<float> UnitsSpeed;
    [NativeDisableParallelForRestriction]
    public NativeArray<float> NeighborsSpeed;

    public Vector3 FlockPosition;
    public float CohesionDistance;
    public float AvoidanceDistance;
    public float AlignmentDistance;
    public float ObstacleAvoidanceDistance;
    public float BoundsDistance;
    public float CohesionWeight;
    public float AvoidanceWeight;
    public float AlignmentWeight;
    public float ObstacleAvoidanceWeight;
    public float BoundsWeight;
    public float FOVAngle;
    public float MinSpeed;
    public float MaxSpeed;
    public float SmoothDamp;
    public float DeltaTime;


    public void Execute(int index)
    {
        // Find neighbors
        int cohesionIndex = 0;
        int avoidanceIndex = 0;
        int alignmentIndex = 0;

        int ind = 0;
        foreach (Vector3 currentNeighborsPosition in UnitsPosition)
        {
            Vector3 currentUnitPosition = UnitsPosition[index];
            Vector3 currentNeighborDirection = UnitsForwardDirection[ind];

            if (currentUnitPosition != currentNeighborsPosition)
            {
                float currentNeighbourDistanceSqr = Vector3.SqrMagnitude(currentUnitPosition - currentNeighborsPosition);
                if (currentNeighbourDistanceSqr < CohesionDistance * CohesionDistance)
                {
                    CohesionNeighbors[cohesionIndex] = currentNeighborsPosition;
                    NeighborsSpeed[cohesionIndex] = UnitsSpeed[ind];
                    cohesionIndex++;
                }
                if (currentNeighbourDistanceSqr < AvoidanceDistance * AvoidanceDistance)
                {
                    AvoidanceNeighbors[avoidanceIndex] = currentNeighborsPosition;
                    avoidanceIndex++;
                }
                if (currentNeighbourDistanceSqr < AlignmentDistance * AlignmentDistance)
                {
                    AlignmentNeighbors[alignmentIndex] = currentNeighborsPosition;
                    AlignmentNeighborsDirections[alignmentIndex] = currentNeighborDirection;
                    alignmentIndex++;
                }
            }
            ind++;
        }

        float speed = 0.0f;
        Vector3 cohesionVector = Vector3.zero;
        if (CohesionNeighbors.Length != 0)
        {
            // Calculate speed
            // change for NeighborsSpeed
            for (int i = 0; i < CohesionNeighbors.Length; i++)
            {
                speed += NeighborsSpeed[i];
            }

            speed /= CohesionNeighbors.Length;
            speed = Mathf.Clamp(speed, MinSpeed, MaxSpeed);

            //Calculate cohesion
            int cohesionNeighborInFOV = 0;
            foreach (Vector3 cohesionNeighbor in CohesionNeighbors)
            {
                if (IsInFov(UnitsForwardDirection[index], UnitsPosition[index], cohesionNeighbor, FOVAngle) && cohesionNeighbor != Vector3.zero)
                {
                    ++cohesionNeighborInFOV;
                    cohesionVector += cohesionNeighbor;
                }
            }
            cohesionVector /= cohesionNeighborInFOV;
            cohesionVector -= UnitsPosition[index];
            cohesionVector = cohesionVector.normalized * CohesionWeight;
        }

        //Calculate avoidance
        Vector3 avoidanceVector = Vector3.zero;
        if (AvoidanceNeighbors.Length != 0)
        {
            int avoidanceNeighborInFOV = 0;
            foreach (Vector3 avoidanceNeighbor in AvoidanceNeighbors)
            {
                if (IsInFov(UnitsForwardDirection[index], UnitsPosition[index], avoidanceNeighbor, FOVAngle) && avoidanceNeighbor != Vector3.zero)
                {
                    ++avoidanceNeighborInFOV;
                    avoidanceVector += UnitsPosition[index] - avoidanceNeighbor;
                }
            }
            avoidanceVector /= avoidanceNeighborInFOV;
            avoidanceVector = avoidanceVector.normalized * AvoidanceWeight;
        }

        //Calculate alignment
        Vector3 alignmentVector = UnitsForwardDirection[index];
        if (AlignmentNeighbors.Length != 0)
        {
            int alignmentNeighborInFOV = 0;
            int inde = 0;
            foreach (Vector3 alignmentNeighbor in AlignmentNeighbors)
            {
                if (IsInFov(UnitsForwardDirection[index], UnitsPosition[index], alignmentNeighbor, FOVAngle) && alignmentNeighbor != Vector3.zero)
                {
                    alignmentNeighborInFOV++;
                    alignmentVector += AlignmentNeighborsDirections[inde];
                }
                inde++;
            }
            alignmentVector /= alignmentNeighborInFOV;
            alignmentVector = alignmentVector.normalized * AlignmentWeight;
        }

        //Calculate Bounds
        Vector3 offsetToCenter = FlockPosition - UnitsPosition[index];
        bool isNearBound = offsetToCenter.magnitude >= BoundsDistance * 0.9f;
        Vector3 boundsVector = isNearBound ? offsetToCenter.normalized : Vector3.zero;
        boundsVector *= BoundsWeight;

        Vector3 currentVelocity = UnitsVelocity[index];
        Vector3 moveVector = cohesionVector + avoidanceVector + alignmentVector + boundsVector;
        moveVector = Vector3.SmoothDamp(UnitsForwardDirection[index], moveVector, ref currentVelocity, SmoothDamp, 10000, DeltaTime);
        moveVector = moveVector.normalized * speed;

        if (moveVector == Vector3.zero)
            moveVector = UnitsForwardDirection[index];

        UnitsPosition[index] = UnitsPosition[index] + moveVector * DeltaTime;
        UnitsForwardDirection[index] = moveVector.normalized;
        UnitsSpeed[index] = speed;
        UnitsVelocity[index] = currentVelocity;
    }

    private bool IsInFov(Vector3 forward, Vector3 unitPosition, Vector3 targetPosition, float angle)
    {
        return Vector3.Angle(forward, targetPosition - unitPosition) <= angle;
    }
}
