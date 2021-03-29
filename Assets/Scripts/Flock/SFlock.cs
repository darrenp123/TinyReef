using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

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
    private NativeList<float> _unitsCurrentHunger;
    private NativeList<float> _unitsStarvingTimer;
    private NativeList<float> _unitsLifeSpan;

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

    private MoveFlockJob _moveJob;
    private SecheduleUnitsSightJob _secheduleRaysJob;
    private UnitHungerJob _hungerJob;

    private int _totalUnitAmought;
    private int _unitBatchCount;
    private int _sightBatchCount;

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

        InitialGenerateUnits();

        _sightDirections = new NativeArray<float3>(AllUnits[0].Directions.Length, Allocator.Persistent);
        for (int i = 0; i < AllUnits[0].Directions.Length; i++)
        {
            _sightDirections[i] = AllUnits[0].Directions[i];
        }

         _secheduleRaysJob = new SecheduleUnitsSightJob
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

        _moveJob = new MoveFlockJob
        {
            UnitsForwardDirections = _unitsForwardDirections,
            UnitsCurrentVelocities = _unitsCurrentVelocities,
            UnitsPositions = _unitsPositions,
            UnitsCurrentWaypoints = _unitsCurrentWaypoints,
            UnitsCurrentHunger = _unitsCurrentHunger,

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
            HungerThreshold = AllUnits.Count > 0 ? AllUnits[0].HungerThreshold : 0,

            TestIndex = unitIndex
        };


        _hungerJob = new UnitHungerJob
        {
            UnitsCurrentHunger = _unitsCurrentHunger,
            UnitsStarvingTimer = _unitsStarvingTimer,
            UnitsLifeSpan = _unitsLifeSpan,

            TotalHunger = AllUnits.Count > 0 ? AllUnits[0].TotalHunger : 0,
            HungerThreshold = AllUnits.Count > 0 ? AllUnits[0].HungerThreshold : 0,
            InitSarvingTimer = AllUnits.Count > 0 ? AllUnits[0].InitStarvingTimer : 0,
            DeltaTime = Time.deltaTime
        };

        // For debugging.
        var debuger = GetComponent<FlockDebuger>();
        if (debuger) debuger.InitDebugger(AllUnits.ToArray(), obstacleDistance, sphereCastRadius);
    }

    private void Update()
    {
        if (_totalUnitAmought <= 0) return;

        float deltaTime = Time.deltaTime;
        for (int i = 0; i < _totalUnitAmought; i++)
        {
            Transform currentUnitTransform = AllUnits[i].MyTransform;
            _unitsPositions[i] = currentUnitTransform.position;
            _unitsForwardDirections[i] = currentUnitTransform.forward;
            _unitsRotaions[i] = currentUnitTransform.rotation;
        }

        _moveJob.DeltaTime = deltaTime;
        _hungerJob.DeltaTime = deltaTime;

        NativeArray<JobHandle> dependencies = new NativeArray<JobHandle>(6, Allocator.Temp);
        JobHandle secheduleHandle = _secheduleRaysJob.Schedule(_totalUnitAmought, _unitBatchCount);
        dependencies[0] = SpherecastCommand.ScheduleBatch(_unitsSightDirectionsCheks, _unitsObstacleSightResults, _sightBatchCount, secheduleHandle);
        dependencies[1] = SpherecastCommand.ScheduleBatch(_unitsObstacleChecks, _unitsObstacleResults, _unitBatchCount, secheduleHandle);
        dependencies[2] = SpherecastCommand.ScheduleBatch(_unitsPredatorsChecks, _unitsPredatorsResults, _sightBatchCount, secheduleHandle);
        dependencies[3] = SpherecastCommand.ScheduleBatch(_unitsPreyChecks, _unitsPreyResults, _sightBatchCount, secheduleHandle);
        dependencies[4] = SpherecastCommand.ScheduleBatch(_unitsPredatorPreysObtacleChecks, _unitsPredatorPreysObtacleResults, _sightBatchCount, secheduleHandle);
        dependencies[5] = _hungerJob.Schedule(_totalUnitAmought, _unitBatchCount, default);

        JobHandle dependency = JobHandle.CombineDependencies(dependencies);
        _moveJob.Schedule(_totalUnitAmought, _unitBatchCount, dependency).Complete();

        for (int i = 0; i < _totalUnitAmought; i++)
        {
            SFlockUnit currentUnit = AllUnits[i];
            currentUnit.MyTransform.forward = _moveJob.UnitsForwardDirections[i];
            currentUnit.MyTransform.position = _moveJob.UnitsPositions[i];
            currentUnit.CurrentVelocity = _moveJob.UnitsCurrentVelocities[i];
            currentUnit.CurrrentHunger = _hungerJob.UnitsCurrentHunger[i];
            currentUnit.LifeSpan = _hungerJob.UnitsLifeSpan[i];

            if (currentUnit.CurrrentHunger <= currentUnit.HungerThreshold && Physics.SphereCast(currentUnit.MyTransform.position,
           sphereCastRadius * 0.5f, currentUnit.MyTransform.forward, out RaycastHit hit, currentUnit.KillBoxDistance, preyMask))
            {
                var killedUnit = hit.transform.GetComponent<IFood>();
                if (killedUnit != null)
                {
                    killedUnit.Consume();
                    _unitsCurrentHunger[i] += currentUnit.TotalHunger / 3;
                    Debug.Log("Fish consumed! ");
                }
            }

            // should make removal safer by using a temp array with the indexes of units to be removed
            if (currentUnit.LifeSpan <= 0 || _unitsStarvingTimer[i] <= 0)
            {
                print("Unit died: " + (currentUnit.LifeSpan <= 0) + " or starved: " + (_unitsStarvingTimer[i] <= 0));
                RemoveUnit(currentUnit);
                break;
            }
        }

        if (_totalUnitAmought >= 2 && UnityEngine.Random.Range(0, 800) <= 1)
        {
            SpawnNewUnit();
        }

        dependencies.Dispose();
    }

    private void InitialGenerateUnits()
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
        _unitsCurrentHunger = new NativeList<float>(flockSize, Allocator.Persistent);
        _unitsStarvingTimer = new NativeList<float>(flockSize, Allocator.Persistent);
        _unitsLifeSpan = new NativeList<float>(flockSize, Allocator.Persistent);

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

        for (int i = 0; i < flockSize; i++)
        {
            CreateUnit();
        }

        RefreshBatches();
    }

    private void SpawnNewUnit()
    {
        CreateUnit();
        AllocateNewValues();
        RefreshBatches();
        Debug.Log("Unit made: " + _totalUnitAmought);
    }

    private void CreateUnit()
    {
        SpherecastCommand emptyCommand = new SpherecastCommand();
        RaycastHit emptyRay = new RaycastHit();

        Vector3 randomVector = Vector3.Scale(UnityEngine.Random.insideUnitSphere, spawnBounds);
        SFlockUnit newUnit = Instantiate(flockUnitPrefab, transform.position + randomVector,
            Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0), transform);
        newUnit.Initialize(initMaxSpeed, initPredatorPreyDistance);
        newUnit.OnUnitRemove = RemoveUnit;
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
        _unitsCurrentHunger.Add(newUnit.TotalHunger);
        _unitsStarvingTimer.Add(newUnit.InitStarvingTimer);
        _unitsLifeSpan.Add(newUnit.LifeSpan);

        for (int j = 0; j < flockUnitPrefab.NumViewDirections; j++)
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

    private void RemoveUnit(SFlockUnit unitToRemove)
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
        _unitsCurrentHunger.RemoveAt(index);
        _unitsStarvingTimer.RemoveAt(index);
        _unitsLifeSpan.RemoveAt(index);

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

        AllocateNewValues();
        RefreshBatches();

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

    private void AllocateNewValues()
    {
        _secheduleRaysJob.UnitPositions = _unitsPositions;
        _secheduleRaysJob.UnitForwardDirections = _unitsForwardDirections;
        _secheduleRaysJob.UnitRotaions = _unitsRotaions;
        _secheduleRaysJob.ObstacleChecks = _unitsObstacleChecks;
        _secheduleRaysJob.UnitSightDirectionsChecks = _unitsSightDirectionsCheks;
        _secheduleRaysJob.UnitsPredatorsChecks = _unitsPredatorsChecks;
        _secheduleRaysJob.UnitsPreysChecks = _unitsPreyChecks;
        _secheduleRaysJob.UnitsPredatorsPreyObstackleChecks = _unitsPredatorPreysObtacleChecks;
        _secheduleRaysJob.UnitsSightDirections = _unitsSightDirections;

        _moveJob.UnitsForwardDirections = _unitsForwardDirections;
        _moveJob.UnitsCurrentVelocities = _unitsCurrentVelocities;
        _moveJob.UnitsPositions = _unitsPositions;
        _moveJob.UnitsCurrentWaypoints = _unitsCurrentWaypoints;
        _moveJob.UnitsCurrentHunger = _unitsCurrentHunger;
        _moveJob.UnitsMaxSpeed = _unitsMaxSpeed;
        _moveJob.UnitsSightDistance = _unitsSightDistance;
        _moveJob.UnitsObstacleResults = _unitsObstacleResults;
        _moveJob.UnitsObstacleSightResults = _unitsObstacleSightResults;
        _moveJob.UnitsPredatorsResults = _unitsPredatorsResults;
        _moveJob.UnitsPreyResults = _unitsPreyResults;
        _moveJob.UnitsPredatorPreyObtacleResults = _unitsPredatorPreysObtacleResults;
        _moveJob.UnitSightDirections = _unitsSightDirections;

        _hungerJob.UnitsCurrentHunger = _unitsCurrentHunger;
        _hungerJob.UnitsStarvingTimer = _unitsStarvingTimer;
        _hungerJob.UnitsLifeSpan = _unitsLifeSpan;
    }

    private void RefreshBatches()
    {
        _totalUnitAmought = AllUnits.Count;
        _unitBatchCount = _totalUnitAmought > 10 ? _totalUnitAmought / 10 : 1;
        _sightBatchCount = _sightDirections.Length / (_totalUnitAmought <= 0 ? 1 : _totalUnitAmought);

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
        _unitsCurrentHunger.Dispose();
        _unitsStarvingTimer.Dispose();
        _unitsLifeSpan.Dispose();
    }
}
