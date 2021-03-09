using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class IndependentFlock : MonoBehaviour
{
    [Header("Spawn Setup")]
    [SerializeField] private SFlockUnit flockUnitPrefab;
    [SerializeField] private int flockSize;
    [SerializeField] private Vector3 spawnBounds;
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private LayerMask preyMask;
    [Range(0, 1)]
    [SerializeField] private float sphereCastRadius;
    [SerializeField] private FlockWaypoints flockWaypoints;

    [Header("Speed Setup")]
    [Range(0, 10)]
    [SerializeField] private float minSpeed;
    [Range(0, 10)]
    [SerializeField] private float maxSpeed;
    [Range(0, 10)]
    [SerializeField] private float maxSteerForce;

    [Header("Detection Distances")]
    [Range(0, 10)]
    [SerializeField] private float avoidanceDistance;
    [Range(0, 10)]
    [SerializeField] private float patrolWaypointDistance;
    [Range(0, 10)]
    [SerializeField] private float obstacleDistance;
    [Range(0, 10)]
    [SerializeField] private float preyDistance;
    [Range(0, 100)]
    [SerializeField] private float boundsDistance;

    [Header("Behaviour Weights")]
    [Range(0, 10)]
    [SerializeField] private float avoidanceWeight;
    [Range(0, 10)]
    [SerializeField] private float patrolWaypointWeight;
    [Range(0, 100)]
    [SerializeField] private float obstacleAvoidanceWeight;
    [Range(0, 10)]
    [SerializeField] private float preyPersuitWeight;
    [Range(0, 10)]
    [SerializeField] private float boundsWeight;

    public List<SFlockUnit> AllUnits { get; set; }

    private NativeArray<float3> _unitsForwardDirections;
    private NativeArray<float3> _unitsCurrentVelocities;
    private NativeArray<float3> _unitsPositions;
    private NativeArray<Quaternion> _unitsRotaions;
    private NativeArray<UnitStates> _unitsStates;
    private NativeArray<int> _unitsCurrentWaypoints;
    private NativeArray<float3> _sightDirections;
    private NativeArray<SpherecastCommand> _obstacleChecks;
    private NativeArray<RaycastHit> _obstacleResults;
    private NativeArray<float3> _flockWaypoints;
    private NativeArray<float> _unitsHungerTimer;

    void Start()
    {
        Vector3[] waypoints = flockWaypoints.GetWaypoints();
        _flockWaypoints = new NativeArray<float3>(waypoints.Length, Allocator.Persistent);

        for (int i = 0; i < waypoints.Length; i++)
        {
            _flockWaypoints[i] = waypoints[i];
        }

        GenerateUnits();

        // For debugging.
        var debuger = GetComponent<FlockDebuger>();
        if (debuger) debuger.InitDebugger(AllUnits.ToArray(), obstacleDistance, sphereCastRadius);
    }

    void Update()
    {
        int totalUnitAmought = AllUnits.Count;
        int numberOfSightDirections = flockUnitPrefab.NumViewDirections * totalUnitAmought;

        var unitsSightDirectionsCheks = new NativeArray<SpherecastCommand>(numberOfSightDirections, Allocator.TempJob);
        var unitsObstacleSightResults = new NativeArray<RaycastHit>(numberOfSightDirections, Allocator.TempJob);

        var unitsFakeChecks = new NativeArray<SpherecastCommand>(numberOfSightDirections, Allocator.TempJob);

        var unitsPreyChecks = new NativeArray<SpherecastCommand>(numberOfSightDirections, Allocator.TempJob);
        var unitsPreyResults = new NativeArray<RaycastHit>(numberOfSightDirections, Allocator.TempJob);

        var unitsPreyObtacleChecks = new NativeArray<SpherecastCommand>(numberOfSightDirections, Allocator.TempJob);
        var unitsPreyObtacleResults = new NativeArray<RaycastHit>(numberOfSightDirections, Allocator.TempJob);

        var unitsSightDirections = new NativeArray<float3>(numberOfSightDirections, Allocator.TempJob);

        for (int i = 0; i < totalUnitAmought; i++)
        {
            Transform currentUnitTransform = AllUnits[i].MyTransform;
            _unitsPositions[i] = currentUnitTransform.position;
            _unitsForwardDirections[i] = currentUnitTransform.forward;
            _unitsRotaions[i] = currentUnitTransform.rotation;
            _unitsHungerTimer[i] -= Time.deltaTime;
        }

        SecheduleUnitsSightJob secheduleRaysJob = new SecheduleUnitsSightJob
        {
            UnitPositions = _unitsPositions,
            UnitForwardDirections = _unitsForwardDirections,
            UnitRotaions = _unitsRotaions,
            SightDirections = _sightDirections,

            ObstacleChecks = _obstacleChecks,
            UnitsPreysChecks = unitsFakeChecks,
            UnitSightDirectionsChecks = unitsSightDirectionsCheks,
            UnitsPredatorsChecks = unitsPreyChecks,
            UnitsPredatorsPreyObstackleChecks = unitsPreyObtacleChecks,
            UnitsSightDirections = unitsSightDirections,

            ObstacleDistance = obstacleDistance,
            ObstacleMask = obstacleMask,
            PredatorPreyDistance = preyDistance,
            PredatorMask = preyMask,
            SphereCastRadius = sphereCastRadius
        };

        JobHandle secheduleHandle = secheduleRaysJob.Schedule(totalUnitAmought, 15, default);
        JobHandle sightSchedulingHandle = SpherecastCommand.ScheduleBatch(unitsSightDirectionsCheks, unitsObstacleSightResults, 100, secheduleHandle);
        JobHandle checkObstacleHandle = SpherecastCommand.ScheduleBatch(_obstacleChecks, _obstacleResults, 100, sightSchedulingHandle);
        JobHandle checkPredatorsHandle = SpherecastCommand.ScheduleBatch(unitsPreyChecks, unitsPreyResults, 100, checkObstacleHandle);
        JobHandle checkPreysObstacleHandle = SpherecastCommand.ScheduleBatch(unitsPreyObtacleChecks, unitsPreyObtacleResults, 100, checkPredatorsHandle);

        var moveJob = new MoveIdependentFlock
        {
            UnitsForwardDirections = _unitsForwardDirections,
            UnitsCurrentVelocities = _unitsCurrentVelocities,
            UnitsPositions = _unitsPositions,
            UnitsCurrentWaypoints = _unitsCurrentWaypoints,
            UnitsHungerTimer = _unitsHungerTimer,

            ObstacleResults = _obstacleResults,
            UnitsObstacleSightResults = unitsObstacleSightResults,
            UnitsPreyResults = unitsPreyResults,
            UnitsPreyObtacleResults = unitsPreyObtacleResults,
            UnitSightDirections = unitsSightDirections,
            FlockWaypoints = _flockWaypoints,

            AvoidanceDistance = avoidanceDistance,
            PatrolWaypointDistance = patrolWaypointDistance,
            ObstacleDistance = obstacleDistance,
            PreyDistance = preyDistance,
            BoundsDistance = boundsDistance,

            AvoidanceWeight = avoidanceWeight,
            PatrolWaypointWeight = patrolWaypointWeight,
            PreyPersuitWeight = preyPersuitWeight,
            ObstacleAvoidanceWeight = obstacleAvoidanceWeight,
            BoundsWeight = boundsWeight,

            FovAngle = flockUnitPrefab.FOVAngle,
            MaxSteerForce = maxSteerForce,
            MinSpeed = minSpeed,
            MaxSpeed = maxSpeed,
            FlockPosition = transform.position,
            DeltaTime = Time.deltaTime,
        };

        moveJob.Schedule(totalUnitAmought, 15, checkPreysObstacleHandle).Complete();

        for (int i = 0; i < totalUnitAmought; i++)
        {
            SFlockUnit currentUnit = AllUnits[i];
            currentUnit.MyTransform.forward = moveJob.UnitsForwardDirections[i];
            currentUnit.MyTransform.position = moveJob.UnitsPositions[i];
            currentUnit.CurrentVelocity = moveJob.UnitsCurrentVelocities[i];
            currentUnit.CurrrentHunger = _unitsHungerTimer[i];

            if (currentUnit.CurrrentHunger <= 0 && Physics.SphereCast(currentUnit.MyTransform.position,
                sphereCastRadius * 0.5f, currentUnit.MyTransform.forward, out RaycastHit hit, currentUnit.KillBoxDistance, preyMask))
            {
                var killedUnit = hit.transform.GetComponent<SFlockUnit>();
                if (killedUnit)
                {
                    killedUnit.RemoveUnit();
                    _unitsHungerTimer[i] = currentUnit.HungerThreshold;
                }
            }
        }

        unitsSightDirectionsCheks.Dispose();
        unitsObstacleSightResults.Dispose();
        unitsSightDirections.Dispose();
        unitsPreyChecks.Dispose();
        unitsPreyResults.Dispose();
        unitsPreyObtacleChecks.Dispose();
        unitsPreyObtacleResults.Dispose();
        unitsFakeChecks.Dispose();
    }

    private void GenerateUnits()
    {
        AllUnits = new List<SFlockUnit>(flockSize);
        _unitsForwardDirections = new NativeArray<float3>(flockSize, Allocator.Persistent);
        _unitsCurrentVelocities = new NativeArray<float3>(flockSize, Allocator.Persistent);
        _unitsPositions = new NativeArray<float3>(flockSize, Allocator.Persistent);
        _unitsRotaions = new NativeArray<Quaternion>(flockSize, Allocator.Persistent);
        _unitsStates = new NativeArray<UnitStates>(flockSize, Allocator.Persistent);
        _unitsHungerTimer = new NativeArray<float>(flockSize, Allocator.Persistent);

        for (int i = 0; i < flockSize; i++)
        {
            Vector3 randomVector = Vector3.Scale(UnityEngine.Random.insideUnitSphere, spawnBounds);
            SFlockUnit newUnit = Instantiate(flockUnitPrefab, transform.position + randomVector,
                Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0), transform);

            AllUnits.Add(newUnit);
            newUnit.Initialize(maxSpeed, preyDistance);

            _unitsForwardDirections[i] = newUnit.MyTransform.forward;
            _unitsCurrentVelocities[i] = newUnit.CurrentVelocity;
            _unitsPositions[i] = newUnit.MyTransform.position;
            _unitsStates[i] = newUnit.UnitState;
            _unitsHungerTimer[i] = newUnit.HungerThreshold;
        }

        _sightDirections = new NativeArray<float3>(AllUnits[0].Directions.Length, Allocator.Persistent);
        if (_sightDirections.Length > 0)
        {
            for (int i = 0; i < AllUnits[0].Directions.Length; i++)
            {
                _sightDirections[i] = AllUnits[0].Directions[i];
            }
        }

        _obstacleChecks = new NativeArray<SpherecastCommand>(flockSize, Allocator.Persistent);
        _obstacleResults = new NativeArray<RaycastHit>(flockSize, Allocator.Persistent);

        _unitsCurrentWaypoints = new NativeArray<int>(flockSize, Allocator.Persistent);
        for (int i = 0; i < _unitsCurrentWaypoints.Length; i++)
        {
            _unitsCurrentWaypoints[i] = UnityEngine.Random.Range(0, _flockWaypoints.Length);
        }
    }

    private void OnDestroy()
    {
        _unitsForwardDirections.Dispose();
        _unitsCurrentVelocities.Dispose();
        _unitsPositions.Dispose();
        _unitsRotaions.Dispose();
        _unitsCurrentWaypoints.Dispose();
        _unitsStates.Dispose();
        _sightDirections.Dispose();
        _obstacleChecks.Dispose();
        _obstacleResults.Dispose();
        _flockWaypoints.Dispose();
        _unitsHungerTimer.Dispose();
    }
}

[BurstCompile]
public struct MoveIdependentFlock : IJobParallelFor
{
    [NativeDisableParallelForRestriction]
    public NativeArray<float3> UnitsCurrentVelocities;
    [NativeDisableParallelForRestriction]
    public NativeArray<float3> UnitsForwardDirections;
    [NativeDisableParallelForRestriction]
    public NativeArray<float3> UnitsPositions;
    [NativeDisableParallelForRestriction]
    public NativeArray<int> UnitsCurrentWaypoints;

    [ReadOnly]
    public NativeArray<RaycastHit> ObstacleResults;
    [ReadOnly]
    public NativeArray<RaycastHit> UnitsObstacleSightResults;
    [ReadOnly]
    public NativeArray<RaycastHit> UnitsPreyResults;
    [ReadOnly]
    public NativeArray<RaycastHit> UnitsPreyObtacleResults;
    [ReadOnly]
    public NativeArray<float3> UnitSightDirections;
    [ReadOnly]
    public NativeArray<float> UnitsHungerTimer;
    [ReadOnly]
    public NativeArray<float3> FlockWaypoints;

    public float AvoidanceDistance;
    public float PatrolWaypointDistance;
    public float ObstacleDistance;
    public float PreyDistance;
    public float BoundsDistance;

    public float AvoidanceWeight;
    public float PatrolWaypointWeight;
    public float ObstacleAvoidanceWeight;
    public float PreyPersuitWeight;
    public float BoundsWeight;

    public float3 FlockPosition;
    public float MaxSteerForce;
    public float FovAngle;
    public float MinSpeed;
    public float MaxSpeed;
    public float DeltaTime;

    public void Execute(int index)
    {
        float3 avoidanceVector = float3.zero;
        int AvoidanceNeighbours = 0;
        float3 currentUnitPosition = UnitsPositions[index];

        for (int i = 0; i < UnitsPositions.Length; i++)
        {
            float3 currentNeighbourPosition = UnitsPositions[i];

            if (!currentUnitPosition.Equals(currentNeighbourPosition))
            {
                float3 offset = currentNeighbourPosition - currentUnitPosition;
                float currentDistanceToNeighbourSqr = offset.x * offset.x + offset.y * offset.y + offset.z * offset.z;

                if (currentDistanceToNeighbourSqr <= AvoidanceDistance * AvoidanceDistance)
                {
                    if (IsInFov(index, currentNeighbourPosition))
                    {
                        AvoidanceNeighbours++;
                        float dist = math.max(currentDistanceToNeighbourSqr, 0.000001f);
                        avoidanceVector -= (currentNeighbourPosition - currentUnitPosition) / dist;
                    }
                }

            }
        }

        float3 offsetToCenter = FlockPosition - UnitsPositions[index];
        bool isInBounds = offsetToCenter.x * offsetToCenter.x + offsetToCenter.y * offsetToCenter.y
            + offsetToCenter.z * offsetToCenter.z < BoundsDistance * BoundsDistance * 0.9f;
        float3 boundsVector = (!isInBounds ? SteerTowards(offsetToCenter, index) : float3.zero)
            * BoundsWeight;

        var obstacleAvoidanceVector = float3.zero;
        var distancetoObstacle = ObstacleResults[index].distance;
        if (distancetoObstacle > 0)
        {
            float distancePercentage = 1 - distancetoObstacle / ObstacleDistance;
            obstacleAvoidanceVector = SteerTowards(AvoidObstacleDirection(index), index)
                * ObstacleAvoidanceWeight * distancePercentage;
        }

        float3 preyPersuitVector = float3.zero;
        if (UnitsHungerTimer[index] <= 0)
        {
            float targetDist = HasPreyInSight(index, out float3 persuitDir);
            if (targetDist > 0)
            {
                float distancePercentage = 1 - targetDist / PreyDistance;
                preyPersuitVector = SteerTowards(persuitDir, index) * PreyPersuitWeight * distancePercentage;
            }
        }

        int waypointIndex = UnitsCurrentWaypoints[index];
        float3 waypointVector = SteerTowards(GetWaypointDirection(UnitsPositions[index], ref waypointIndex), index) * PatrolWaypointWeight;

        if (AvoidanceNeighbours > 0)
        {
            avoidanceVector = SteerTowards(avoidanceVector, index) * AvoidanceWeight;
        }

        if (!preyPersuitVector.Equals(float3.zero))
        {
            waypointVector = float3.zero;
            boundsVector = float3.zero;
        }

        float3 acceleration = avoidanceVector + boundsVector + obstacleAvoidanceVector + preyPersuitVector + waypointVector;
        float3 currVel = UnitsCurrentVelocities[index] + (acceleration * DeltaTime);
        float speed = math.length(currVel);
        speed = math.clamp(speed, MinSpeed, MaxSpeed);
        currVel = math.normalize(currVel) * speed;

        UnitsPositions[index] += currVel * DeltaTime;
        UnitsForwardDirections[index] = math.normalize(currVel);
        UnitsCurrentVelocities[index] = currVel;
        UnitsCurrentWaypoints[index] = waypointIndex;
    }

    private float3 GetWaypointDirection(float3 neighboursCenter, ref int waypointIndex)
    {
        float3 dirToWaypoint = FlockWaypoints[waypointIndex] - neighboursCenter;
        float distanceSqr = dirToWaypoint.x * dirToWaypoint.x + dirToWaypoint.y * dirToWaypoint.y + dirToWaypoint.z * dirToWaypoint.z;
        if (distanceSqr < PatrolWaypointDistance * PatrolWaypointDistance)
        {
            waypointIndex = (waypointIndex + 1) % FlockWaypoints.Length;
        }

        return FlockWaypoints[waypointIndex] - neighboursCenter;
    }

    private float3 AvoidObstacleDirection(int index)
    {
        float maxDistance = int.MinValue;
        float closestAngle = float.MaxValue;
        float3 avialableDirection = float3.zero;
        float3 farthestContestedDirection = float3.zero;
        int particion = UnitsObstacleSightResults.Length / UnitsPositions.Length;

        for (int i = particion * index; i < particion * (index + 1); i++)
        {
            float3 currentDirection = UnitSightDirections[i];
            float angle = Vector3.Angle(UnitsForwardDirections[index], currentDirection);
            float distanceToHit = UnitsObstacleSightResults[i].distance;
            if (distanceToHit > 0)
            {
                if (distanceToHit > maxDistance)
                {
                    maxDistance = distanceToHit;
                    farthestContestedDirection = currentDirection;
                }
            }
            else if (angle >= 20 && angle < closestAngle)
            {
                closestAngle = angle;
                avialableDirection = currentDirection;
            }
        }

        return !avialableDirection.Equals(float3.zero) ? avialableDirection :
            !farthestContestedDirection.Equals(float3.zero) ? farthestContestedDirection : UnitsForwardDirections[index];
    }

    private float HasPreyInSight(int index, out float3 persuitDir)
    {
        // test with calculating prey future position
        float minDistance = float.MaxValue;
        float3 closestPrey = float3.zero;
        float targetDist = 0;
        int particion = UnitsPreyResults.Length / UnitsPositions.Length;
        persuitDir = UnitsForwardDirections[index];

        for (int i = particion * index; i < particion * (index + 1); i++)
        {
            RaycastHit currentCheck = UnitsPreyResults[i];
            float distanceTotarget = currentCheck.distance;
            float3 targetPosition = currentCheck.point;
            if (IsInFov(index, targetPosition) && UnitsPreyObtacleResults[i].distance == 0 && distanceTotarget > 0
                && distanceTotarget < minDistance)
            {
                minDistance = distanceTotarget;
                closestPrey = targetPosition;
            }
        }

        if (!closestPrey.Equals(float3.zero))
        {
            persuitDir = closestPrey - UnitsPositions[index];
            targetDist = minDistance;
        }

        return targetDist;
    }

    private bool IsInFov(int index, float3 targetPosition)
    {
        return Vector3.Angle(UnitsForwardDirections[index], targetPosition - UnitsPositions[index]) <= FovAngle;
    }

    private float3 SteerTowards(float3 vector, int index)
    {
        float3 v = math.normalize(vector) * MaxSpeed - UnitsCurrentVelocities[index];
        return math.length(v) > MaxSteerForce ? math.normalize(v) * MaxSteerForce : v;
    }
}
