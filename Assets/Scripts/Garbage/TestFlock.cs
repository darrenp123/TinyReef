using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using UnityEngine.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;

public class TestFlock : MonoBehaviour
{
    [Header("Spawn Setup")]
    [SerializeField] private TestFlockUnit flockUnitPrefab;
    [SerializeField] private int flockSize;
    [SerializeField] private Vector3 spawnBounds;
    [SerializeField] private LayerMask obstacleMask;

    [Header("Speed Setup")]
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
    [SerializeField] private float aligementDistance;
    [Range(0, 10)]
    [SerializeField] private float obstacleDistance;
    [Range(0, 100)]
    [SerializeField] private float boundsDistance;

    [Header("Behaviour Weights")]
    [Range(0, 10)]
    [SerializeField] private float cohesionWeight;
    [Range(0, 10)]
    [SerializeField] private float avoidanceWeight;
    [Range(0, 10)]
    [SerializeField] private float aligementWeight;
    [Range(0, 100)]
    [SerializeField] private float obstacleAvoidanceWeight;
    [Range(0, 10)]
    [SerializeField] private float boundsWeight;

    public TestFlockUnit[] AllUnits { get; set; }

    private NativeArray<Vector3> _sightDirections;

    private void Start()
    {
        GenerateUnits();
        _sightDirections = new NativeArray<Vector3>(AllUnits.Length, Allocator.Persistent);
        for (int i = 0; i < flockUnitPrefab.Directions.Length; i++)
        {
            _sightDirections[i] = flockUnitPrefab.Directions[i];
        }
    }

    private void Update()
    {
        var unitForwardDirections = new NativeArray<Vector3>(AllUnits.Length, Allocator.TempJob);
        var unitCurrentVelocities = new NativeArray<Vector3>(AllUnits.Length, Allocator.TempJob);
        var unitPositions = new NativeArray<Vector3>(AllUnits.Length, Allocator.TempJob);
        var allUnitsSpeeds = new NativeArray<float>(AllUnits.Length, Allocator.TempJob);
        var unitRotaions = new NativeArray<Quaternion>(AllUnits.Length, Allocator.TempJob);

        var obstacleChecks = new NativeArray<SpherecastCommand>(AllUnits.Length, Allocator.TempJob);
        var obstacleResults = new NativeArray<RaycastHit>(AllUnits.Length, Allocator.TempJob);

        var allUnitSightDirections = new NativeArray<SpherecastCommand>(flockUnitPrefab.Directions.Length * AllUnits.Length, Allocator.TempJob);
        var allUnitSightResults = new NativeArray<RaycastHit>(flockUnitPrefab.Directions.Length * AllUnits.Length, Allocator.TempJob);

        for (int i = 0; i < AllUnits.Length; i++)
        {
            TestFlockUnit currentUnit = AllUnits[i];
            unitForwardDirections[i] = currentUnit.MyTransform.forward;
            unitCurrentVelocities[i] = currentUnit.CurrentVelocity;
            unitPositions[i] = currentUnit.MyTransform.position;
            allUnitsSpeeds[i] = currentUnit.Speed;
            unitRotaions[i] = currentUnit.MyTransform.rotation;
        }

        SecheduleRaysJob secheduleRaysJob = new SecheduleRaysJob
        {
            UnitPositions = unitPositions,
            UnitForwardDirections = unitForwardDirections,
            UnitRotaions = unitRotaions,

            SightDirections = _sightDirections,
            ObstacleChecks = obstacleChecks,
            AllUnitSightDirections = allUnitSightDirections,

            ObstacleDistance = obstacleDistance,
            ObstacleMask = obstacleMask
        };

        JobHandle secheduleHandle = secheduleRaysJob.Schedule(AllUnits.Length, 1, default);
        JobHandle sightSchedulingHandle = SpherecastCommand.ScheduleBatch(allUnitSightDirections, allUnitSightResults, 1, secheduleHandle);
        JobHandle checkObstacleHandle = SpherecastCommand.ScheduleBatch(obstacleChecks, obstacleResults, 1, sightSchedulingHandle);

        MoveJobTest moveJob = new MoveJobTest
        {
            UnitForwardDirections = unitForwardDirections,
            UnitCurrentVelocities = unitCurrentVelocities,
            UnitPositions = unitPositions,
            AllUnitsSpeeds = allUnitsSpeeds,

            ObstacleResults = obstacleResults,
            AllUnitSightResults = allUnitSightResults,

            CohesionDistance = cohesionDistance,
            AvoidanceDistance = avoidanceDistance,
            AligementDistance = aligementDistance,
            BoundsDistance = boundsDistance,
            ObstacleDistance = obstacleDistance,

            CohesionWeight = cohesionWeight,
            AvoidanceWeight = avoidanceWeight,
            AligementWeight = aligementWeight,
            BoundsWeight = boundsWeight,
            ObstacleAvoidanceWeight = obstacleAvoidanceWeight,

            FovAngle = flockUnitPrefab.FOVAngle,
            MinSpeed = minSpeed,
            MaxSpeed = maxSpeed,
            SmoothDamp = flockUnitPrefab.SmoothDamp,
            FlockPosition = transform.position,
            DeltaTime = Time.deltaTime,
        };

        JobHandle moveHandle = moveJob.Schedule(AllUnits.Length, 1, checkObstacleHandle);
        moveHandle.Complete();

        for (int i = 0; i < AllUnits.Length; i++)
        {
            TestFlockUnit currentUnit = AllUnits[i];
            currentUnit.MyTransform.forward = moveJob.UnitForwardDirections[i];
            currentUnit.MyTransform.position = moveJob.UnitPositions[i];
            currentUnit.CurrentVelocity = moveJob.UnitCurrentVelocities[i];
            currentUnit.Speed = moveJob.AllUnitsSpeeds[i];
        }

        unitForwardDirections.Dispose();
        unitCurrentVelocities.Dispose();
        unitPositions.Dispose();
        allUnitsSpeeds.Dispose();
        obstacleChecks.Dispose();
        obstacleResults.Dispose();
        allUnitSightDirections.Dispose();
        allUnitSightResults.Dispose();
        unitRotaions.Dispose();
    }

    private void GenerateUnits()
    {
        AllUnits = new TestFlockUnit[flockSize];
        for (int i = 0; i < flockSize; i++)
        {
            Vector3 randomVector = Vector3.Scale(UnityEngine.Random.insideUnitSphere, spawnBounds);
            AllUnits[i] = Instantiate(flockUnitPrefab, transform.position + randomVector,
                Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0), transform);
            AllUnits[i].AssignedFlock = this;
            AllUnits[i].Speed = UnityEngine.Random.Range(minSpeed, maxSpeed);
        }
    }

    private void OnDestroy()
    {
        _sightDirections.Dispose();
    }

    //private void OnDrawGizmos()
    //{
    //    if (AllUnits != null && AllUnits.Length > 0)
    //    {
    //        Gizmos.color = Color.red;
    //        Gizmos.DrawWireSphere(AllUnits[0].MyTransform.position + (AllUnits[0].MyTransform.forward)* obstacleDistance/1.5f, 0.27f);

    //        for (int i = 0; i < flockUnitPrefab.Directions.Length; i++)
    //        {
    //            var dir = AllUnits[0].transform.TransformDirection(flockUnitPrefab.Directions[i].normalized);
    //            // var dir = AllUnits[0].transform.rotation * TestFlockUnit.Directions[i];
    //            //Debug.Log("Dir: " + dir);
    //             //Gizmos.DrawRay(AllUnits[0].MyTransform.position, dir * obstacleDistance);
    //            Gizmos.DrawWireSphere(AllUnits[0].MyTransform.position + (dir)*obstacleDistance, 0.27f);

    //        }
    //    }
    //}
}


[BurstCompile]
public struct SecheduleRaysJob : IJobParallelFor
{
    [NativeDisableParallelForRestriction]
    [ReadOnly]
    public NativeArray<Vector3> UnitPositions;
    [NativeDisableParallelForRestriction]
    [ReadOnly]
    public NativeArray<Vector3> UnitForwardDirections;
    [NativeDisableParallelForRestriction]
    [ReadOnly]
    public NativeArray<Quaternion> UnitRotaions;
    [NativeDisableParallelForRestriction]
    [ReadOnly]
    public NativeArray<Vector3> SightDirections;

    [NativeDisableParallelForRestriction]
    public NativeArray<SpherecastCommand> ObstacleChecks;
    [NativeDisableParallelForRestriction]
    public NativeArray<SpherecastCommand> AllUnitSightDirections;

    public float ObstacleDistance;
    public LayerMask ObstacleMask;

    public void Execute(int index)
    {
        Vector3 currentUnitPosition = UnitPositions[index];

        ObstacleChecks[index] = new SpherecastCommand(
                 currentUnitPosition, 0.27f, UnitForwardDirections[index], ObstacleDistance, ObstacleMask);

        int particion = (SightDirections.Length / UnitPositions.Length) * index;
        for (int i = 0; i < SightDirections.Length; i++)
        {
            Vector3 dir = UnitRotaions[index] * SightDirections[i];
            AllUnitSightDirections[i + particion] = new SpherecastCommand(
               currentUnitPosition, 0.2f, dir, ObstacleDistance, ObstacleMask);
        }
    }
}

[BurstCompile]
public struct MoveJobTest : IJobParallelFor
{
    [NativeDisableParallelForRestriction]
    public NativeArray<Vector3> UnitCurrentVelocities;
    [NativeDisableParallelForRestriction]
    public NativeArray<Vector3> UnitForwardDirections;
    [NativeDisableParallelForRestriction]
    public NativeArray<Vector3> UnitPositions;
    [NativeDisableParallelForRestriction]
    public NativeArray<float> AllUnitsSpeeds;

    [NativeDisableParallelForRestriction]
    [ReadOnly]
    public NativeArray<RaycastHit> ObstacleResults;
    [NativeDisableParallelForRestriction]
    [ReadOnly]
    public NativeArray<RaycastHit> AllUnitSightResults;

    public float CohesionDistance;
    public float AvoidanceDistance;
    public float AligementDistance;
    public float BoundsDistance;
    public float ObstacleDistance;

    public float CohesionWeight;
    public float AvoidanceWeight;
    public float AligementWeight;
    public float ObstacleAvoidanceWeight;
    public float BoundsWeight;

    public Vector3 FlockPosition;
    public float FovAngle;
    public float MinSpeed;
    public float MaxSpeed;
    public float SmoothDamp;
    public float DeltaTime;

    public void Execute(int index)
    {
        var cohesionVector = Vector3.zero;
        int cohesionNeighboursInFOV = 0;
        var avoidanceVector = Vector3.zero;
        int AvoidanceNeighboursInFOV = 0;
        var alignmnentVector = UnitForwardDirections[index];
        int AlignmentNeighboursInFOV = 0;
        float speed = 0;
        Vector3 currentUnitPosition = UnitPositions[index];

        for (int i = 0; i < UnitPositions.Length; i++)
        {
            Vector3 currentNeighbourPosition = UnitPositions[i];

            if (currentUnitPosition != currentNeighbourPosition)
            {
                float currentDistanceToNeighbourSqr = Vector3.SqrMagnitude(currentUnitPosition - currentNeighbourPosition);

                if (currentDistanceToNeighbourSqr <= CohesionDistance * CohesionDistance)
                {
                    if (IsInFov(index, currentNeighbourPosition))
                    {
                        cohesionNeighboursInFOV++;
                        cohesionVector += currentNeighbourPosition;
                        speed += AllUnitsSpeeds[i];
                    }
                }
                if (currentDistanceToNeighbourSqr <= AvoidanceDistance * AvoidanceDistance)
                {
                    if (IsInFov(index, currentNeighbourPosition))
                    {
                        AvoidanceNeighboursInFOV++;
                        avoidanceVector += currentUnitPosition - currentNeighbourPosition;
                    }
                }
                if (currentDistanceToNeighbourSqr <= AligementDistance * AligementDistance)
                {
                    if (IsInFov(index, currentNeighbourPosition))
                    {
                        AlignmentNeighboursInFOV++;
                        alignmnentVector += UnitForwardDirections[i];
                    }
                }
            }
        }

        Vector3 offsetToCenter = FlockPosition - UnitPositions[index];
        Vector3 boundsVector = (offsetToCenter.magnitude >= BoundsDistance * 0.9f ? offsetToCenter.normalized : Vector3.zero)
            * BoundsWeight;

        var obstacleAvoidanceVector = Vector3.zero;
        if (ObstacleResults[index].distance > 0)
        {
            obstacleAvoidanceVector = AvoidObstacleDirection(index);
        }

        if (cohesionNeighboursInFOV > 0)
        {
            cohesionVector /= cohesionNeighboursInFOV;
            cohesionVector -= UnitPositions[index];
            cohesionVector = cohesionVector.normalized * CohesionWeight;

            speed /= cohesionNeighboursInFOV;
        }
        speed = Mathf.Clamp(speed, MinSpeed, MaxSpeed);

        if (AvoidanceNeighboursInFOV > 0)
        {
            avoidanceVector /= AvoidanceNeighboursInFOV;
            avoidanceVector = avoidanceVector.normalized * AvoidanceWeight;
        }

        if (AlignmentNeighboursInFOV > 0)
        {
            alignmnentVector /= AlignmentNeighboursInFOV;
            alignmnentVector = alignmnentVector.normalized * AligementWeight;
        }

        Vector3 currVel = UnitCurrentVelocities[index];
        Vector3 moveVector = cohesionVector + avoidanceVector + alignmnentVector + boundsVector + obstacleAvoidanceVector;
        moveVector = Vector3.SmoothDamp(UnitForwardDirections[index], moveVector, ref currVel, SmoothDamp, MaxSpeed, DeltaTime);
        moveVector = moveVector.normalized * speed;

        if (moveVector == Vector3.zero)
        {
            moveVector = UnitForwardDirections[index];
        }

        UnitPositions[index] = UnitPositions[index] + moveVector * DeltaTime;
        UnitForwardDirections[index] = moveVector.normalized;
        AllUnitsSpeeds[index] = speed;
        UnitCurrentVelocities[index] = currVel;
    }

    private bool IsInFov(int index, Vector3 targetPosition)
    {
        return Vector3.Angle(UnitForwardDirections[index], targetPosition - UnitPositions[index]) <= FovAngle;
    }

    private Vector3 AvoidObstacleDirection(int index)
    {
        float maxDistance = int.MinValue;
        Vector3 selectedDirection = UnitForwardDirections[index];
        int particion = AllUnitSightResults.Length / UnitPositions.Length;

        for (int i = particion * index; i < particion * (index + 1); i++)
        {
            RaycastHit result = AllUnitSightResults[i];
            float3 currentDirection = (result.point - UnitPositions[index]).normalized;
            float distanceToHit = result.distance;
            if (distanceToHit > 0)
            {
                if (distanceToHit > maxDistance)
                {
                    maxDistance = distanceToHit;
                    selectedDirection = currentDirection;
                }
            }
            else
            {
                selectedDirection = currentDirection;
                break;
            }
        }

        return selectedDirection * ObstacleAvoidanceWeight;
    }
}
