using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class SFlock : MonoBehaviour
{
    [SerializeField] private string flockName;
    [Header("Spawn Setup")]
    [SerializeField] private SFlockUnit flockUnitPrefab;
    [SerializeField] private int flockSize;
    [SerializeField] private Vector3 spawnBounds;
    [SerializeField] private LayerMask obstacleMask;
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

    [Header("spawning percentages")]
    [Range(0, 1)]
    [SerializeField] private float spwanPercent;
    [Range(0, 1)]
    [SerializeField] private float spwanMutatePercent;

    public List<SFlockUnit> AllUnits { get; set; }

    public string FlockName => flockName;

    private NativeList<float3> _unitsForwardDirections;
    private NativeList<float3> _unitsCurrentVelocities;
    private NativeList<float3> _unitsPositions;
    private NativeList<quaternion> _unitsRotaions;
    private NativeList<float> _unitsScales;
    private NativeList<int> _unitsCurrentWaypoints;
    private NativeList<float> _unitsMaxSpeed;
    private NativeList<float> _unitsSightDistance;
    private NativeList<float> _unitsCurrentHunger;
    private NativeList<float> _unitsStarvingTimer;
    private NativeList<float> _unitsLifeSpan;
    private NativeList<int> _unitsSizes;
    private NativeList<LayerMask> _unitsPredatorMasks;
    private NativeList<LayerMask> _unitsPreyMasks;

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
    private NativeList<SpherecastCommand> _unitsEatChecks;
    private NativeList<RaycastHit> _unitsEatResults;
    private NativeList<PreyInfo> _unitsPreyInfo;
    private NativeList<float> _unitsHungerThreshold;
    private NativeList<float> _unitsMatingHurge;

    private MoveFlockJob _moveJob;
    private SecheduleUnitsSightJob _secheduleRaysJob;
    private SecundarySensorCheksJob _secundarySensorCheksJob;
    private UnitHungerJob _hungerJob;

    private NativeList<Unity.Mathematics.Random> _random;

    private float _sphereCastRadius;
    private int _totalUnitAmought;
    private int _unitBatchCount;
    private int _sightBatchCount;
    private float _reactionTimer;
    private ActivityLog _activityLog;

    public OnFlockSizeChangeSignature OnFlockSizeChange;
    public delegate void OnFlockSizeChangeSignature(SFlock flock);

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

        _sphereCastRadius = AllUnits[0].GetComponent<CapsuleCollider>().radius;

        _secheduleRaysJob = new SecheduleUnitsSightJob
        {
            UnitPositions = _unitsPositions,
            UnitForwardDirections = _unitsForwardDirections,
            UnitRotaions = _unitsRotaions,
            UnitsScales = _unitsScales,
            SightDirections = _sightDirections,
            UnitsCurrentHunger = _unitsCurrentHunger,

            ObstacleChecks = _unitsObstacleChecks,
            UnitsPredatorsChecks = _unitsPredatorsChecks,
            UnitsPreysChecks = _unitsPreyChecks,
            UnitsSightDirections = _unitsSightDirections,

            ObstacleDistance = obstacleDistance,
            ObstacleMask = obstacleMask,
            PredatorPreyDistance = initPredatorPreyDistance,
            UnitsPredatorMask = _unitsPredatorMasks,
            UnitsPreyMask = _unitsPreyMasks,
            SphereCastRadius = _sphereCastRadius,
            HungerThreshold = AllUnits.Count > 0 ? AllUnits[0].CurrentHungerThreshold : 0
        };

        _secundarySensorCheksJob = new SecundarySensorCheksJob
        {
            UnitPositions = _unitsPositions,
            UnitsScales = _unitsScales,
            UnitsSightDirections = _unitsSightDirections,
            UnitsObstacleResults = _unitsObstacleResults,
            UnitsPredatorsResults = _unitsPredatorsResults,
            UnitsPreyResults = _unitsPreyResults,

            UnitsSightDirectionsCheks = _unitsSightDirectionsCheks,
            UnitsPredatorPreysObtacleChecks = _unitsPredatorPreysObtacleChecks,

            ObstacleDistance = obstacleDistance,
            ObstacleMask = obstacleMask,
            PredatorPreyDistance = initPredatorPreyDistance,
            SphereCastRadius = _sphereCastRadius
        };

        _moveJob = new MoveFlockJob
        {
            UnitsForwardDirections = _unitsForwardDirections,
            UnitsCurrentVelocities = _unitsCurrentVelocities,
            UnitsPositions = _unitsPositions,
            UnitsCurrentWaypoints = _unitsCurrentWaypoints,
            UnitsScales = _unitsScales,
            UnitsSizes = _unitsSizes,
            UnitsCurrentHunger = _unitsCurrentHunger,

            UnitsMaxSpeed = _unitsMaxSpeed,
            UnitsSightDistance = _unitsSightDistance,
            UnitsObstacleResults = _unitsObstacleResults,
            UnitsObstacleSightResults = _unitsObstacleSightResults,
            UnitsPredatorsResults = _unitsPredatorsResults,
            UnitsPreyInfo = _unitsPreyInfo,
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
            HungerThreshold = AllUnits.Count > 0 ? AllUnits[0].CurrentHungerThreshold : 0,
            SphereCastRadius = _sphereCastRadius,
            RandomRef = _random,
            TestIndex = unitIndex
        };

        _hungerJob = new UnitHungerJob
        {
            UnitPositions = _unitsPositions,
            UnitForwardDirections = _unitsForwardDirections,
            UnitsScales = _unitsScales,
            UnitsPreyMask = _unitsPreyMasks,

            UnitsCurrentHunger = _unitsCurrentHunger,
            UnitsStarvingTimer = _unitsStarvingTimer,
            UnitsLifeSpan = _unitsLifeSpan,
            UnitsEatChecks = _unitsEatChecks,
            UnitsHungerThreshold = _unitsHungerThreshold,
            UnitsMatingHurge = _unitsMatingHurge,

            SphereCastRadius = _sphereCastRadius,
            KillBoxDistance = AllUnits.Count > 0 ? AllUnits[0].KillBoxDistance : 0,
            TotalHunger = AllUnits.Count > 0 ? AllUnits[0].TotalHunger : 0,
            InitSarvingTimer = AllUnits.Count > 0 ? AllUnits[0].InitStarvingTimer : 0,
            DeltaTime = Time.deltaTime
        };

        _activityLog = FindObjectOfType<ActivityLog>();
        StartCoroutine(SpawnUnit());

        // For debugging.
        var debuger = GetComponent<FlockDebuger>();
        if (debuger) debuger.InitDebugger(AllUnits.ToArray(), obstacleDistance, _sphereCastRadius);
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
        _reactionTimer += deltaTime;

        JobHandle secheduleHandle = _secheduleRaysJob.Schedule(_totalUnitAmought, _unitBatchCount);

        NativeArray<JobHandle> dependencies = new NativeArray<JobHandle>(3, Allocator.Temp);
        dependencies[0] = SpherecastCommand.ScheduleBatch(_unitsObstacleChecks, _unitsObstacleResults, _unitBatchCount, secheduleHandle);

        bool canReact = _reactionTimer > 0.15f;
        if (canReact)
        {
            dependencies[1] = SpherecastCommand.ScheduleBatch(_unitsPredatorsChecks, _unitsPredatorsResults, _sightBatchCount, secheduleHandle);
            dependencies[2] = SpherecastCommand.ScheduleBatch(_unitsPreyChecks, _unitsPreyResults, _sightBatchCount, secheduleHandle);

            RaycastHit emptyHit = new RaycastHit();
            PreyInfo emptyInfo = new PreyInfo();
            JobHandle.CombineDependencies(dependencies).Complete();
            for (int i = 0; i < (flockUnitPrefab.NumViewDirections * _totalUnitAmought); i++)
            {
                RaycastHit currentPreyResult = _unitsPreyResults[i];
                _unitsPreyInfo[i] = emptyInfo;
                if (currentPreyResult.transform)
                {
                    var fishPrey = currentPreyResult.transform.GetComponent<SFlockUnit>();
                    if (!AllUnits.Contains(fishPrey))
                    {
                        _unitsPreyInfo[i] = new PreyInfo()
                        {
                            size = fishPrey ? fishPrey.Size : 0,
                            impactPoint = currentPreyResult.point,
                            distance = currentPreyResult.distance
                        };
                    }
                }

                if (_unitsPredatorsResults[i].transform && AllUnits.Contains(_unitsPredatorsResults[i].transform.GetComponent<SFlockUnit>()))
                {
                    _unitsPredatorsResults[i] = emptyHit;
                }
            }
        }

        JobHandle secundarysecheduleHandle = _secundarySensorCheksJob.Schedule(_totalUnitAmought, _unitBatchCount, dependencies[0]);
        dependencies[0] = SpherecastCommand.ScheduleBatch(_unitsSightDirectionsCheks, _unitsObstacleSightResults, _sightBatchCount, secundarysecheduleHandle);
        if (canReact)
        {
            dependencies[1] = SpherecastCommand.ScheduleBatch(_unitsPredatorPreysObtacleChecks, _unitsPredatorPreysObtacleResults, _sightBatchCount, secundarysecheduleHandle);
            _reactionTimer = 0;
        }

        dependencies[2] = _hungerJob.Schedule(_totalUnitAmought, _unitBatchCount, secheduleHandle);
        JobHandle lastBatch = SpherecastCommand.ScheduleBatch(_unitsEatChecks, _unitsEatResults, _unitBatchCount, dependencies[2]);

        _moveJob.Schedule(_totalUnitAmought, _unitBatchCount, JobHandle.CombineDependencies(dependencies)).Complete();
        lastBatch.Complete();

        List<SFlockUnit> UnitsToRemove = new List<SFlockUnit>(_totalUnitAmought);

        for (int i = 0; i < _totalUnitAmought; i++)
        {
            SFlockUnit currentUnit = AllUnits[i];
            currentUnit.MyTransform.forward = _moveJob.UnitsForwardDirections[i];
            currentUnit.MyTransform.position = _moveJob.UnitsPositions[i];
            currentUnit.CurrentVelocity = _moveJob.UnitsCurrentVelocities[i];
            currentUnit.CurrrentHunger = _hungerJob.UnitsCurrentHunger[i];
            currentUnit.LifeSpan = _hungerJob.UnitsLifeSpan[i];
            // might have a current mating urge to be able to show in the UI

            // if (currentUnit.CurrrentHunger <= currentUnit.HungerThreshold && Physics.SphereCast(currentUnit.MyTransform.position,
            //sphereCastRadius * 0.5f, currentUnit.MyTransform.forward, out RaycastHit hit, currentUnit.KillBoxDistance, preyMask))
            Transform foodTransform = _unitsEatResults[i].transform;
            if (foodTransform)
            {
                //var killedUnit = hit.transform.GetComponent<IFood>();
                var killedUnit = foodTransform.GetComponent<IFood>();
                if (killedUnit != null && !AllUnits.Contains(foodTransform.GetComponent<SFlockUnit>()))
                {
                    killedUnit.Consume();
                    _unitsCurrentHunger[i] += currentUnit.TotalHunger / 3;

                    string message = currentUnit.UnitName + " consumed " + killedUnit.GetFoodName();
                    SendMessage(MessageType.DEFAULT, message);
                }
            }

            if (currentUnit.LifeSpan <= 0 || _unitsStarvingTimer[i] <= 0)
            {
                UnitsToRemove.Add(currentUnit);
                if (_activityLog)
                {
                    string message = currentUnit.UnitName + " died of " + ((currentUnit.LifeSpan <= 0) ? "old age." : "starvation");
                    SendMessage(MessageType.DEATH, message);
                }
            }
        }

        foreach (SFlockUnit unitToRemove in UnitsToRemove)
        {
            RemoveUnit(unitToRemove);
        }

        dependencies.Dispose();
    }

    private void InitialGenerateUnits()
    {
        AllUnits = new List<SFlockUnit>(flockSize);
        _unitsForwardDirections = new NativeList<float3>(flockSize, Allocator.Persistent);
        _unitsCurrentVelocities = new NativeList<float3>(flockSize, Allocator.Persistent);
        _unitsPositions = new NativeList<float3>(flockSize, Allocator.Persistent);
        _unitsRotaions = new NativeList<quaternion>(flockSize, Allocator.Persistent);
        _unitsScales = new NativeList<float>(flockSize, Allocator.Persistent);
        _unitsCurrentWaypoints = new NativeList<int>(flockSize, Allocator.Persistent);
        _unitsMaxSpeed = new NativeList<float>(flockSize, Allocator.Persistent);
        _unitsSightDistance = new NativeList<float>(flockSize, Allocator.Persistent);
        _unitsObstacleChecks = new NativeList<SpherecastCommand>(flockSize, Allocator.Persistent);
        _unitsObstacleResults = new NativeList<RaycastHit>(flockSize, Allocator.Persistent);
        _unitsCurrentHunger = new NativeList<float>(flockSize, Allocator.Persistent);
        _unitsStarvingTimer = new NativeList<float>(flockSize, Allocator.Persistent);
        _unitsLifeSpan = new NativeList<float>(flockSize, Allocator.Persistent);
        _unitsSizes = new NativeList<int>(flockSize, Allocator.Persistent);
        _unitsEatChecks = new NativeList<SpherecastCommand>(flockSize, Allocator.Persistent);
        _unitsEatResults = new NativeList<RaycastHit>(flockSize, Allocator.Persistent);
        _unitsPredatorMasks = new NativeList<LayerMask>(flockSize, Allocator.Persistent);
        _unitsPreyMasks = new NativeList<LayerMask>(flockSize, Allocator.Persistent);
        _random = new NativeList<Unity.Mathematics.Random>(flockSize, Allocator.Persistent);
        _unitsHungerThreshold = new NativeList<float>(flockSize, Allocator.Persistent);
        _unitsMatingHurge = new NativeList<float>(flockSize, Allocator.Persistent);

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
        _unitsPreyInfo = new NativeList<PreyInfo>(numberOfSightDirections, Allocator.Persistent);

        for (int i = 0; i < flockSize; i++)
        {
            CreateUnit();
        }

        RefreshBatches();
    }

    private void CreateUnit()
    {
        SpherecastCommand emptyCommand = new SpherecastCommand();
        RaycastHit emptyRay = new RaycastHit();
        PreyInfo emptyInfo = new PreyInfo();

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
        _unitsScales.Add(newUnit.MyTransform.localScale.x);
        _unitsCurrentWaypoints.Add(UnityEngine.Random.Range(0, _flockWaypoints.Length));
        _unitsMaxSpeed.Add(newUnit.MaxSpeed);
        _unitsSightDistance.Add(newUnit.SightDistance);
        _unitsObstacleChecks.Add(emptyCommand);
        _unitsObstacleResults.Add(emptyRay);
        _unitsCurrentHunger.Add(newUnit.TotalHunger);
        _unitsStarvingTimer.Add(newUnit.InitStarvingTimer);
        _unitsLifeSpan.Add(newUnit.LifeSpan);
        _unitsSizes.Add(newUnit.Size);
        _unitsEatChecks.Add(emptyCommand);
        _unitsEatResults.Add(emptyRay);
        _unitsPredatorMasks.Add(newUnit.PredatorMask);
        _unitsPreyMasks.Add(newUnit.PreyMask);
        _random.Add(new Unity.Mathematics.Random((uint)UnityEngine.Random.Range(1, 100000)));
        _unitsHungerThreshold.Add(newUnit.CurrentHungerThreshold);
        _unitsMatingHurge.Add(newUnit.CurrentMatingUrge);

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
            _unitsPreyInfo.Add(emptyInfo);
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
        _unitsScales.RemoveAt(index);
        _unitsCurrentWaypoints.RemoveAt(index);
        _unitsMaxSpeed.RemoveAt(index);
        _unitsSightDistance.RemoveAt(index);
        _unitsObstacleChecks.RemoveAt(index);
        _unitsObstacleResults.RemoveAt(index);
        _unitsCurrentHunger.RemoveAt(index);
        _unitsStarvingTimer.RemoveAt(index);
        _unitsLifeSpan.RemoveAt(index);
        _unitsSizes.RemoveAt(index);
        _unitsEatChecks.RemoveAt(index);
        _unitsEatResults.RemoveAt(index);
        _unitsPredatorMasks.RemoveAt(index);
        _unitsPreyMasks.RemoveAt(index);
        _random.RemoveAt(index);
        _unitsHungerThreshold.RemoveAt(index);
        _unitsMatingHurge.RemoveAt(index);

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
        _unitsPreyInfo.RemoveRangeWithBeginEnd(from, to);

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
        _unitsScales[index] = flockUnit.MyTransform.localScale.x;
        _unitsSizes[index] = flockUnit.Size;
        _unitsPredatorMasks[index] = flockUnit.PredatorMask;
        _unitsPreyMasks[index] = flockUnit.PreyMask;
        _unitsHungerThreshold[index] = flockUnit.CurrentHungerThreshold;
        _unitsMatingHurge[index] = flockUnit.CurrentMatingUrge;
    }

    private void AllocateNewValues()
    {
        _secheduleRaysJob.UnitPositions = _unitsPositions;
        _secheduleRaysJob.UnitForwardDirections = _unitsForwardDirections;
        _secheduleRaysJob.UnitRotaions = _unitsRotaions;
        _secheduleRaysJob.UnitsScales = _unitsScales;
        _secheduleRaysJob.ObstacleChecks = _unitsObstacleChecks;
        _secheduleRaysJob.UnitsPredatorsChecks = _unitsPredatorsChecks;
        _secheduleRaysJob.UnitsPreysChecks = _unitsPreyChecks;
        _secheduleRaysJob.UnitsSightDirections = _unitsSightDirections;
        _secheduleRaysJob.UnitsCurrentHunger = _unitsCurrentHunger;
        _secheduleRaysJob.UnitsPredatorMask = _unitsPredatorMasks;
        _secheduleRaysJob.UnitsPreyMask = _unitsPreyMasks;

        _secundarySensorCheksJob.UnitPositions = _unitsPositions;
        _secundarySensorCheksJob.UnitsScales = _unitsScales;
        _secundarySensorCheksJob.UnitsSightDirections = _unitsSightDirections;
        _secundarySensorCheksJob.UnitsObstacleResults = _unitsObstacleResults;
        _secundarySensorCheksJob.UnitsPredatorsResults = _unitsPredatorsResults;
        _secundarySensorCheksJob.UnitsPreyResults = _unitsPreyResults;
        _secundarySensorCheksJob.UnitsSightDirectionsCheks = _unitsSightDirectionsCheks;
        _secundarySensorCheksJob.UnitsPredatorPreysObtacleChecks = _unitsPredatorPreysObtacleChecks;

        _moveJob.UnitsForwardDirections = _unitsForwardDirections;
        _moveJob.UnitsCurrentVelocities = _unitsCurrentVelocities;
        _moveJob.UnitsPositions = _unitsPositions;
        _moveJob.UnitsScales = _unitsScales;
        _moveJob.UnitsSizes = _unitsSizes;
        _moveJob.UnitsCurrentWaypoints = _unitsCurrentWaypoints;
        _moveJob.UnitsCurrentHunger = _unitsCurrentHunger;
        _moveJob.UnitsMaxSpeed = _unitsMaxSpeed;
        _moveJob.UnitsSightDistance = _unitsSightDistance;
        _moveJob.UnitsObstacleResults = _unitsObstacleResults;
        _moveJob.UnitsObstacleSightResults = _unitsObstacleSightResults;
        _moveJob.UnitsPredatorsResults = _unitsPredatorsResults;
        _moveJob.UnitsPreyInfo = _unitsPreyInfo;
        _moveJob.UnitsPredatorPreyObtacleResults = _unitsPredatorPreysObtacleResults;
        _moveJob.UnitSightDirections = _unitsSightDirections;
        _moveJob.RandomRef = _random;

        _hungerJob.UnitPositions = _unitsPositions;
        _hungerJob.UnitForwardDirections = _unitsForwardDirections;
        _hungerJob.UnitsScales = _unitsScales;
        _hungerJob.UnitsCurrentHunger = _unitsCurrentHunger;
        _hungerJob.UnitsStarvingTimer = _unitsStarvingTimer;
        _hungerJob.UnitsLifeSpan = _unitsLifeSpan;
        _hungerJob.UnitsEatChecks = _unitsEatChecks;
        _hungerJob.UnitsPreyMask = _unitsPreyMasks;
        _hungerJob.UnitsHungerThreshold = _unitsHungerThreshold;
        _hungerJob.UnitsMatingHurge = _unitsMatingHurge;
    }

    private void RefreshBatches()
    {
        _totalUnitAmought = AllUnits.Count;
        _unitBatchCount = _totalUnitAmought > 10 ? _totalUnitAmought / 10 : 1;
        _sightBatchCount = _sightDirections.Length / (_totalUnitAmought <= 0 ? 1 : _totalUnitAmought);
        OnFlockSizeChange?.Invoke(this);
    }

    private void SpawnNewUnit(SFlockUnit parent1, SFlockUnit parent2)
    {
        CreateUnit();

        float newBornSpeed = (parent1.MaxSpeed + parent2.MaxSpeed) / 2;
        float newBornSightDist = (parent1.SightDistance + parent2.SightDistance) / 2;
        int newBornSize = (parent1.Size + parent2.Size) / 2;
        float newBornLifespan = (parent1.InitialLifespan + parent2.InitialLifespan) / 2;

        SFlockUnit child = AllUnits[_totalUnitAmought];

        MutateChild(ref newBornSpeed, ref newBornSightDist, ref newBornSize, ref newBornLifespan);

       //print("new born stats: speed: " + newBornSpeed + ", sight dist: " + newBornSightDist + ", size: " + newBornSize + ", lifeSpan: " + newBornLifespan);

        child.SetMaxSpeed(newBornSpeed);
        child.SetSightDistance(newBornSightDist);
        child.SetScale(newBornSize);
        child.SetLifeSapan(newBornLifespan);

        AllocateNewValues();
        RefreshBatches();
        string message = "A " + flockUnitPrefab.UnitName + " spawned! (current number is " + _totalUnitAmought + ")";
        SendMessage(MessageType.BIRTH, message);
    }

    private IEnumerator SpawnUnit()
    {
        while (true)
        {
            yield return new WaitForSeconds(2);

            float randInt = UnityEngine.Random.value;
            if (_totalUnitAmought >= 2 && randInt <= spwanPercent)
            {
                SFlockUnit parent1 = null;
                SFlockUnit parent2 = null;
                float best1 = 0;
                float best2 = 0;
                int index1 = -1;
                int index2 = -1;
                for (int i = 0; i < _totalUnitAmought; ++i)
                {
                    if (_unitsMatingHurge[i] > 0) continue;

                    SFlockUnit currentUnit = AllUnits[i];
                    float currentEvaluation = currentUnit.CalculateUnitFitness();

                    if (best1 < currentEvaluation)
                    {
                        parent2 = parent1;
                        best2 = best1;
                        index2 = index1;
                        parent1 = currentUnit;
                        best1 = currentEvaluation;
                        index1 = i;
                    }
                    else if (best2 < currentEvaluation)
                    {
                        parent2 = currentUnit;
                        best2 = currentEvaluation;
                        index2 = i;
                    }
                }

                //print("parent 1: " + index1 + ", parent 2: " + index2);
                if (index1 >= 0 && index2 >= 0)
                {
                    _unitsMatingHurge[index1] = AllUnits[index1].CurrentMatingUrge;
                    _unitsMatingHurge[index2] = AllUnits[index2].CurrentMatingUrge;

                    SpawnNewUnit(parent1, parent2);
                }
            }
        }
    }

    private void MutateChild(ref float newBornSpeed, ref float newBornSightDist, ref int newBornSize, ref float newBornLifSpan)
    {
        float mutateChance = UnityEngine.Random.value;
        if (mutateChance > spwanMutatePercent) return;

        int changeDelta = UnityEngine.Random.value >= 0.5f ? 1 : -1;
        int trait = UnityEngine.Random.Range(1, 5);
        //print("mutated: " + changeDelta);
        switch (trait)
        {
            case 1:
                if (newBornSpeed <= 1) return;
                newBornSpeed += changeDelta;
                break;
            case 2:
                if (newBornSightDist <= 1) return;
                newBornSightDist += changeDelta;
                break;

            case 3:
                if (newBornSize <= 1) return;
                newBornSize += changeDelta;
                break;

            case 4:
                if ((newBornLifSpan / 60) <= 1) return;
                newBornLifSpan += (changeDelta * 60);
                break;
        }
    }

    private void SendMessage(MessageType messageType, string message)
    {
        if (_activityLog)
            _activityLog.SendMessage(messageType, message);
    }

    private void OnDestroy()
    {
        _unitsForwardDirections.Dispose();
        _unitsCurrentVelocities.Dispose();
        _unitsPositions.Dispose();
        _unitsRotaions.Dispose();
        _unitsScales.Dispose();
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
        _unitsSizes.Dispose();
        _unitsEatChecks.Dispose();
        _unitsEatResults.Dispose();
        _random.Dispose();
        _unitsPredatorMasks.Dispose();
        _unitsPreyMasks.Dispose();
        _unitsPreyInfo.Dispose();
        _unitsHungerThreshold.Dispose();
        _unitsMatingHurge.Dispose();
    }
}
