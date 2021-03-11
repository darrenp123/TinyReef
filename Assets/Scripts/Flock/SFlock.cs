using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using System.Collections.Generic;

public class SFlock : MonoBehaviour
{
    [Header("Spawn Setup")]
    [SerializeField] private SFlockUnit flockUnitPrefab;
    [SerializeField] private int flockSize;
    [SerializeField] private Vector3 spawnBounds;
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private LayerMask predatorMask;
    [SerializeField] private LayerMask preyMask;
    [Range(0, 1)]
    [SerializeField] private float sphereCastRadius;
    [SerializeField] private FlockWaypoints flockWaypoints;

    [Header("Speed Setup")]
    [Range(0, 10)]
    [SerializeField] private float minSpeed;
    [Range(0, 10)]
    [SerializeField] private float initMaxSpeed;
    [Range(0, 10)]
    [SerializeField] private float maxSteerForce;

    [Header("Detection Distances")]
    [Range(0, 10)]
    [SerializeField] private float cohesionDistance;
    [Range(0, 10)]
    [SerializeField] private float avoidanceDistance;
    [Range(0, 10)]
    [SerializeField] private float aligementDistance;
    [Range(0, 10)]
    [SerializeField] private float patrolWaypointDistance;
    [Range(0, 10)]
    [SerializeField] private float obstacleDistance;
    [Range(0, 10)]
    [SerializeField] private float initPredatorPreyDistance;
    [Range(0, 100)]
    [SerializeField] private float boundsDistance;

    [Header("Behaviour Weights")]
    [Range(0, 10)]
    [SerializeField] private float cohesionWeight;
    [Range(0, 10)]
    [SerializeField] private float avoidanceWeight;
    [Range(0, 10)]
    [SerializeField] private float aligementWeight;
    [Range(0, 10)]
    [SerializeField] private float patrolWaypointWeight;
    [Range(0, 100)]
    [SerializeField] private float obstacleAvoidanceWeight;
    [Range(0, 10)]
    [SerializeField] private float predatorFleeWeight;
    [Range(0, 10)]
    [SerializeField] private float preyPersuitWeight;
    [Range(0, 10)]
    [SerializeField] private float boundsWeight;

    public List<SFlockUnit> AllUnits { get; set; }

    private NativeList<float3> _unitsForwardDirections;
    private NativeList<float3> _unitsCurrentVelocities;
    private NativeList<float3> _unitsPositions;
    private NativeList<Quaternion> _unitsRotaions;
    private NativeList<int> _unitsCurrentWaypoints;
    private NativeList<float> _unitsMaxSpeed;
    private NativeList<float> _unitsSightDistance;
    private NativeList<float> _unitsHungerTimer;

    private NativeList<SpherecastCommand> _unitsObstacleChecks;
    private NativeList<RaycastHit> _unitsObstacleResults;

    private NativeArray<float3> _sightDirections;
    private NativeArray<float3> _flockWaypoints;

    private NativeList<SpherecastCommand> _unitsSightDirectionsCheks;
    private NativeList<RaycastHit> _unitsObstacleSightResults;
    private NativeList<SpherecastCommand> _unitsPredatorsChecks;
    private NativeList<RaycastHit> _unitsPredatorsResults;
    private NativeList<SpherecastCommand> _unitsPreyChecks;
    private NativeList<RaycastHit> _unitsPreyResults;
    private NativeList<SpherecastCommand> _unitsPredatorPreysObtacleChecks;
    private NativeList<RaycastHit> _unitsPredatorPreysObtacleResults;
    private NativeList<float3> _unitsSightDirections;

    // used for debugging and testing
    [Header("Debugging and Testing")]
    public int unitIndex;

    private void Start()
    {
        Vector3[] waypoints = flockWaypoints.GetWaypoints();
        _flockWaypoints = new NativeArray<float3>(waypoints.Length, Allocator.Persistent);
        for (int i = 0; i < waypoints.Length; i++)
        {
            _flockWaypoints[i] = waypoints[i];
        }

        GenerateUnits();

        _sightDirections = new NativeArray<float3>(AllUnits[0].Directions.Length, Allocator.Persistent);
        for (int i = 0; i < AllUnits[0].Directions.Length; i++)
        {
            _sightDirections[i] = AllUnits[0].Directions[i];
        }

        // For debugging.
        var debuger = GetComponent<FlockDebuger>();
        if (debuger) debuger.InitDebugger(AllUnits.ToArray(), obstacleDistance, sphereCastRadius);
    }

    private void Update()
    {
        // might change to private variables to avoid running every frame.
        int totalUnitAmought = AllUnits.Count;
        int unitBatchCount = totalUnitAmought / 10;
        int sightBatchCount = totalUnitAmought / _sightDirections.Length;

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

            ObstacleChecks = _unitsObstacleChecks,
            UnitSightDirectionsChecks = _unitsSightDirectionsCheks,
            UnitsPredatorsChecks = _unitsPredatorsChecks,
            UnitsPreysChecks = _unitsPreyChecks,
            UnitsPredatorsPreyObstackleChecks = _unitsPredatorPreysObtacleChecks,
            UnitsSightDirections = _unitsSightDirections,

            ObstacleDistance = obstacleDistance,
            ObstacleMask = obstacleMask,
            PredatorPreyDistance = initPredatorPreyDistance,
            PredatorMask = predatorMask,
            PreyMask = preyMask,
            SphereCastRadius = sphereCastRadius
        };

        JobHandle secheduleHandle = secheduleRaysJob.Schedule(totalUnitAmought, unitBatchCount, default);
        JobHandle sightSchedulingHandle = SpherecastCommand.ScheduleBatch(_unitsSightDirectionsCheks, _unitsObstacleSightResults, sightBatchCount, secheduleHandle);
        JobHandle checkObstacleHandle = SpherecastCommand.ScheduleBatch(_unitsObstacleChecks, _unitsObstacleResults, unitBatchCount, sightSchedulingHandle);
        JobHandle checkPredatorsHandle = SpherecastCommand.ScheduleBatch(_unitsPredatorsChecks, _unitsPredatorsResults, sightBatchCount, checkObstacleHandle);
        JobHandle checkPreyHandle = SpherecastCommand.ScheduleBatch(_unitsPreyChecks, _unitsPreyResults, sightBatchCount, checkPredatorsHandle);
        JobHandle checkPredatorPreyObstacleHandle = SpherecastCommand.ScheduleBatch(_unitsPredatorPreysObtacleChecks, _unitsPredatorPreysObtacleResults, sightBatchCount, checkPreyHandle);

        MoveFlockJob moveJob = new MoveFlockJob
        {
            UnitsForwardDirections = _unitsForwardDirections,
            UnitsCurrentVelocities = _unitsCurrentVelocities,
            UnitsPositions = _unitsPositions,
            UnitsCurrentWaypoints = _unitsCurrentWaypoints,
            UnitsHungerTimer = _unitsHungerTimer,

            UnitsMaxSpeed = _unitsMaxSpeed,
            UnitsSightDistance = _unitsSightDistance,
            UnitsObstacleResults = _unitsObstacleResults,
            UnitsObstacleSightResults = _unitsObstacleSightResults,
            UnitsPredatorsResults = _unitsPredatorsResults,
            UnitsPreyResults = _unitsPreyResults,
            UnitsPredatorPreyObtacleResults = _unitsPredatorPreysObtacleResults,
            UnitSightDirections = _unitsSightDirections,
            FlockWaypoints = _flockWaypoints,

            CohesionDistance = cohesionDistance,
            AvoidanceDistance = avoidanceDistance,
            AligementDistance = aligementDistance,
            PatrolWaypointDistance = patrolWaypointDistance,
            ObstacleDistance = obstacleDistance,
            BoundsDistance = boundsDistance,

            CohesionWeight = cohesionWeight,
            AvoidanceWeight = avoidanceWeight,
            AligementWeight = aligementWeight,
            PatrolWaypointWeight = patrolWaypointWeight,
            ObstacleAvoidanceWeight = obstacleAvoidanceWeight,
            PredatorFleeWeight = predatorFleeWeight,
            PreyPersuitWeight = preyPersuitWeight,
            BoundsWeight = boundsWeight,

            FovAngle = flockUnitPrefab.FOVAngle,
            MaxSteerForce = maxSteerForce,
            MinSpeed = minSpeed,
            FlockPosition = transform.position,
            DeltaTime = Time.deltaTime,

            TestIndex = unitIndex
        };

        moveJob.Schedule(totalUnitAmought, unitBatchCount, checkPredatorPreyObstacleHandle).Complete();

        for (int i = 0; i < totalUnitAmought; i++)
        {
            SFlockUnit currentUnit = AllUnits[i];
            currentUnit.MyTransform.forward = moveJob.UnitsForwardDirections[i];
            currentUnit.MyTransform.position = moveJob.UnitsPositions[i];
            currentUnit.CurrentVelocity = moveJob.UnitsCurrentVelocities[i];
            currentUnit.CurrrentHunger = _unitsHungerTimer[i];
            // might not be needed.
            currentUnit.CurrentWaypoint = moveJob.UnitsCurrentWaypoints[i];

            if (currentUnit.CurrrentHunger <= 0 && Physics.SphereCast(currentUnit.MyTransform.position,
           sphereCastRadius * 0.5f, currentUnit.MyTransform.forward, out RaycastHit hit, currentUnit.KillBoxDistance, preyMask))
            {
                var killedUnit = hit.transform.GetComponent<SFlockUnit>();
                if (killedUnit)
                {
                    killedUnit.RemoveUnit();
                    _unitsHungerTimer[i] = currentUnit.HungerThreshold;
                    Debug.Log("Fish consumed! ");
                }
            }
        }
    }

    private void GenerateUnits()
    {
        AllUnits = new List<SFlockUnit>(flockSize);
        _unitsForwardDirections = new NativeList<float3>(flockSize, Allocator.Persistent);
        _unitsCurrentVelocities = new NativeList<float3>(flockSize, Allocator.Persistent);
        _unitsPositions = new NativeList<float3>(flockSize, Allocator.Persistent);
        _unitsRotaions = new NativeList<Quaternion>(flockSize, Allocator.Persistent);
        _unitsCurrentWaypoints = new NativeList<int>(flockSize, Allocator.Persistent);
        _unitsMaxSpeed = new NativeList<float>(flockSize, Allocator.Persistent);
        _unitsSightDistance = new NativeList<float>(flockSize, Allocator.Persistent);
        _unitsObstacleChecks = new NativeList<SpherecastCommand>(flockSize, Allocator.Persistent);
        _unitsObstacleResults = new NativeList<RaycastHit>(flockSize, Allocator.Persistent);
        _unitsHungerTimer = new NativeList<float>(flockSize, Allocator.Persistent);

        SpherecastCommand emptyCommand = new SpherecastCommand();
        RaycastHit emptyRay = new RaycastHit();

        for (int i = 0; i < flockSize; i++)
        {
            Vector3 randomVector = Vector3.Scale(UnityEngine.Random.insideUnitSphere, spawnBounds);
            SFlockUnit newUnit = Instantiate(flockUnitPrefab, transform.position + randomVector,
                Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0), transform);
            newUnit.Initialize(initMaxSpeed, initPredatorPreyDistance);
            newUnit.OnUnitRemove = OnUnitRemoved;
            newUnit.OnUnitTraitsValueChanged = OnUnitChangeTraits;

            AllUnits.Add(newUnit);
            _unitsForwardDirections.Add(newUnit.MyTransform.forward);
            _unitsCurrentVelocities.Add(newUnit.CurrentVelocity);
            _unitsPositions.Add(newUnit.MyTransform.position);
            _unitsRotaions.Add(newUnit.MyTransform.rotation);
            _unitsCurrentWaypoints.Add(UnityEngine.Random.Range(0, _flockWaypoints.Length));
            _unitsMaxSpeed.Add(newUnit.MaxSpeed);
            _unitsSightDistance.Add(newUnit.SightDistance);
            _unitsObstacleChecks.Add(emptyCommand);
            _unitsObstacleResults.Add(emptyRay);
            _unitsHungerTimer.Add(newUnit.HungerThreshold);
        }

        int numberOfSightDirections = flockUnitPrefab.NumViewDirections * flockSize;
        _unitsSightDirectionsCheks = new NativeList<SpherecastCommand>(numberOfSightDirections, Allocator.Persistent);
        _unitsObstacleSightResults = new NativeList<RaycastHit>(numberOfSightDirections, Allocator.Persistent);
        _unitsPredatorsChecks = new NativeList<SpherecastCommand>(numberOfSightDirections, Allocator.Persistent);
        _unitsPredatorsResults = new NativeList<RaycastHit>(numberOfSightDirections, Allocator.Persistent);
        _unitsPreyChecks = new NativeList<SpherecastCommand>(numberOfSightDirections, Allocator.Persistent);
        _unitsPreyResults = new NativeList<RaycastHit>(numberOfSightDirections, Allocator.Persistent);
        _unitsPredatorPreysObtacleChecks = new NativeList<SpherecastCommand>(numberOfSightDirections, Allocator.Persistent);
        _unitsPredatorPreysObtacleResults = new NativeList<RaycastHit>(numberOfSightDirections, Allocator.Persistent);
        _unitsSightDirections = new NativeList<float3>(numberOfSightDirections, Allocator.Persistent);

        for (int i = 0; i < numberOfSightDirections; i++)
        {
            _unitsSightDirectionsCheks.Add(emptyCommand);
            _unitsObstacleSightResults.Add(emptyRay);
            _unitsPredatorsChecks.Add(emptyCommand);
            _unitsPredatorsResults.Add(emptyRay);
            _unitsPreyChecks.Add(emptyCommand);
            _unitsPreyResults.Add(emptyRay);
            _unitsPredatorPreysObtacleChecks.Add(emptyCommand);
            _unitsPredatorPreysObtacleResults.Add(emptyRay);
            _unitsSightDirections.Add(float3.zero);
        }
    }

    private void OnUnitRemoved(SFlockUnit unitToRemove)
    {
        int index = AllUnits.IndexOf(unitToRemove);
        if (index < 0) return;

        AllUnits.RemoveAt(index);
        _unitsForwardDirections.RemoveAt(index);
        _unitsCurrentVelocities.RemoveAt(index);
        _unitsPositions.RemoveAt(index);
        _unitsRotaions.RemoveAt(index);
        _unitsCurrentWaypoints.RemoveAt(index);
        _unitsMaxSpeed.RemoveAt(index);
        _unitsSightDistance.RemoveAt(index);
        _unitsObstacleChecks.RemoveAt(index);
        _unitsObstacleResults.RemoveAt(index);

        int from = index * flockUnitPrefab.NumViewDirections;
        int to = (1 + index) * flockUnitPrefab.NumViewDirections;

        _unitsSightDirectionsCheks.RemoveRangeWithBeginEnd(from, to);
        _unitsObstacleSightResults.RemoveRangeWithBeginEnd(from, to);
        _unitsPredatorsChecks.RemoveRangeWithBeginEnd(from, to);
        _unitsPredatorsResults.RemoveRangeWithBeginEnd(from, to);
        _unitsPreyChecks.RemoveRangeWithBeginEnd(from, to);
        _unitsPreyResults.RemoveRangeWithBeginEnd(from, to);
        _unitsPredatorPreysObtacleChecks.RemoveRangeWithBeginEnd(from, to);
        _unitsPredatorPreysObtacleResults.RemoveRangeWithBeginEnd(from, to);
        _unitsSightDirections.RemoveRangeWithBeginEnd(from, to);

        //is there a need for this?
        for (int i = 0; i < AllUnits.Count; i++)
        {
            var unit = AllUnits[i];
            _unitsCurrentVelocities[i] = unit.CurrentVelocity;
            _unitsCurrentWaypoints[i] = unit.CurrentWaypoint;
        }

        Destroy(unitToRemove.gameObject);
    }

    private void OnUnitChangeTraits(SFlockUnit flockUnit)
    {
        int index = AllUnits.IndexOf(flockUnit);
        if (index < 0) return;

        // add all traits that can be editable 
        _unitsMaxSpeed[index] = flockUnit.MaxSpeed;
        _unitsSightDistance[index] = flockUnit.SightDistance;
    }

    private void OnDestroy()
    {
        _unitsForwardDirections.Dispose();
        _unitsCurrentVelocities.Dispose();
        _unitsPositions.Dispose();
        _unitsRotaions.Dispose();
        _unitsCurrentWaypoints.Dispose();
        _sightDirections.Dispose();
        _unitsObstacleChecks.Dispose();
        _unitsObstacleResults.Dispose();
        _flockWaypoints.Dispose();
        _unitsSightDirectionsCheks.Dispose();
        _unitsObstacleSightResults.Dispose();
        _unitsPredatorsChecks.Dispose();
        _unitsPredatorsResults.Dispose();
        _unitsPreyChecks.Dispose();
        _unitsPreyResults.Dispose();
        _unitsPredatorPreysObtacleChecks.Dispose();
        _unitsPredatorPreysObtacleResults.Dispose();
        _unitsSightDirections.Dispose();
        _unitsMaxSpeed.Dispose();
        _unitsSightDistance.Dispose();
        _unitsHungerTimer.Dispose();
    }
}
