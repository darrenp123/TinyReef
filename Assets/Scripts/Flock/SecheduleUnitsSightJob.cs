using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct SecheduleUnitsSightJob : IJobParallelFor
{
    [ReadOnly]
    public NativeArray<float3> UnitPositions;
    [ReadOnly]
    public NativeArray<float3> UnitForwardDirections;
    [ReadOnly]
    public NativeArray<quaternion> UnitRotaions;
    [ReadOnly]
    public NativeArray<float> UnitsScales;
    [ReadOnly]
    public NativeArray<float3> SightDirections;
    [ReadOnly]
    public NativeArray<float> UnitsCurrentHunger;
    [ReadOnly]
    public NativeArray<LayerMask> UnitsPredatorMask;
    [ReadOnly]
    public NativeArray<LayerMask> UnitsPreyMask;

    [WriteOnly]
    public NativeArray<SpherecastCommand> ObstacleChecks;
    [NativeDisableParallelForRestriction]
    [WriteOnly]
    public NativeArray<SpherecastCommand> UnitsPredatorsChecks;
    [NativeDisableParallelForRestriction]
    [WriteOnly]
    public NativeArray<SpherecastCommand> UnitsPreysChecks;
    [NativeDisableParallelForRestriction]
    [WriteOnly]
    public NativeArray<float3> UnitsSightDirections;
   
    public float ObstacleDistance;
    public LayerMask ObstacleMask;
    public float PredatorPreyDistance;
    public float SphereCastRadius;
    public float HungerThreshold;

    public void Execute(int index)
    {
        float3 currentUnitPosition = UnitPositions[index];
        float currentUnitScale = UnitsScales[index];
        float sphereRadius = SphereCastRadius * currentUnitScale;

        ObstacleChecks[index] = new SpherecastCommand(currentUnitPosition, sphereRadius,
            UnitForwardDirections[index], ObstacleDistance * currentUnitScale * 0.85f, ObstacleMask);

        SpherecastCommand emptyCommand = new SpherecastCommand();

        int IndexStart = SightDirections.Length * index;
        for (int i = 0; i < SightDirections.Length; i++)
        {
            float3 dir = math.rotate(UnitRotaions[index], SightDirections[i]);
            // float3 dir = UnitRotaions[index] * SightDirections[i];
            UnitsSightDirections[i + IndexStart] = dir;

            UnitsPredatorsChecks[i + IndexStart] = new SpherecastCommand(
                currentUnitPosition, sphereRadius, dir, PredatorPreyDistance, UnitsPredatorMask[index]);

            UnitsPreysChecks[i + IndexStart] = UnitsCurrentHunger[index] <= HungerThreshold ? new SpherecastCommand(
        currentUnitPosition, sphereRadius, dir, PredatorPreyDistance, UnitsPreyMask[index]) : emptyCommand;
        }
    }
}

[BurstCompile]
public struct SecundarySensorCheksJob : IJobParallelFor
{
    [ReadOnly]
    public NativeArray<float3> UnitPositions;
    [ReadOnly]
    public NativeArray<float> UnitsScales;
    [ReadOnly]
    public NativeArray<float3> UnitsSightDirections;
    [ReadOnly]
    public NativeArray<RaycastHit> UnitsObstacleResults;
    [ReadOnly]
    public NativeArray<RaycastHit> UnitsPredatorsResults;
    [ReadOnly]
    public NativeArray<RaycastHit> UnitsPreyResults;

    [NativeDisableParallelForRestriction]
    [WriteOnly]
    public NativeArray<SpherecastCommand> UnitsSightDirectionsCheks;
    [NativeDisableParallelForRestriction]
    [WriteOnly]
    public NativeArray<SpherecastCommand> UnitsPredatorPreysObtacleChecks;

    public float ObstacleDistance;
    public float SphereCastRadius;
    public float PredatorPreyDistance;
    public LayerMask ObstacleMask;

    public void Execute(int index)
    {
        float3 currentUnitPosition = UnitPositions[index];
        float currentUnitScale = UnitsScales[index];
        float sphereRadius = SphereCastRadius * currentUnitScale;

        int particion = UnitsSightDirections.Length / UnitsObstacleResults.Length;
        bool hasObstacle = UnitsObstacleResults[index].distance > 0;
        SpherecastCommand emptyCommand = new SpherecastCommand();

        for (int i = particion * index; i < particion * (index + 1); i++)
        {
            UnitsSightDirectionsCheks[i] = hasObstacle ? new SpherecastCommand(currentUnitPosition, sphereRadius, 
                UnitsSightDirections[i], ObstacleDistance * currentUnitScale, ObstacleMask) : emptyCommand;

            UnitsPredatorPreysObtacleChecks[i] = (UnitsPredatorsResults[i].distance > 0 || UnitsPreyResults[i].distance > 0) ?
                new SpherecastCommand(currentUnitPosition, sphereRadius, UnitsSightDirections[i], PredatorPreyDistance, ObstacleMask)
                : emptyCommand;
        }
    }
}

// used as an idea to increase performance, but it lead nowhere
[BurstCompile]
public struct PreyPredetorCheksJob : IJobParallelFor
{
    [ReadOnly]
    public NativeArray<float3> UnitPositions;
    [ReadOnly]
    public NativeArray<float3> UnitForwardDirections;
    [ReadOnly]
    public NativeArray<RaycastHit> UnitsPredatorsResults;
    [ReadOnly]
    public NativeArray<RaycastHit> UnitsPreyResults;

    [NativeDisableParallelForRestriction]
    [WriteOnly]
    public NativeArray<SpherecastCommand> UnitsPredatorsObstackleChecks;
    [NativeDisableParallelForRestriction]
    [WriteOnly]
    public NativeArray<SpherecastCommand> UnitsPreyObstackleChecks;

    public float SphereCastRadius;
    public float PredatorPreyDistance;
    public LayerMask ObstacleMask;

    public void Execute(int index)
    {
        float3 unitPosition = UnitPositions[index];
        UnitsPredatorsObstackleChecks[index] = new SpherecastCommand();
        UnitsPreyObstackleChecks[index] = new SpherecastCommand();

        RaycastHit predatorHit = UnitsPredatorsResults[index];
        if (predatorHit.distance > 0)
        {
            UnitsPredatorsObstackleChecks[index] = new SpherecastCommand(
               unitPosition, SphereCastRadius, math.normalizesafe((float3)predatorHit.point - unitPosition),
               PredatorPreyDistance, ObstacleMask);
        }

        RaycastHit preyHit = UnitsPreyResults[index];
        if (preyHit.distance > 0)
        {
            UnitsPreyObstackleChecks[index] = new SpherecastCommand(
               unitPosition, SphereCastRadius, math.normalizesafe((float3)preyHit.point - unitPosition),
               PredatorPreyDistance, ObstacleMask);
        }
    }
}
