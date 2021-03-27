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
    public NativeArray<Quaternion> UnitRotaions;
    [ReadOnly]
    public NativeArray<float3> SightDirections;

    [NativeDisableParallelForRestriction]
    [WriteOnly]
    public NativeArray<SpherecastCommand> ObstacleChecks;
    [NativeDisableParallelForRestriction]
    [WriteOnly]
    public NativeArray<SpherecastCommand> UnitSightDirectionsChecks;
    [NativeDisableParallelForRestriction]
    [WriteOnly]
    public NativeArray<SpherecastCommand> UnitsPredatorsChecks;
    [NativeDisableParallelForRestriction]
    [WriteOnly]
    public NativeArray<SpherecastCommand> UnitsPreysChecks;
    [NativeDisableParallelForRestriction]
    [WriteOnly]
    public NativeArray<SpherecastCommand> UnitsPredatorsPreyObstackleChecks;
    [NativeDisableParallelForRestriction]
    [WriteOnly]
    public NativeArray<float3> UnitsSightDirections;

    public float ObstacleDistance;
    public LayerMask ObstacleMask;
    public float PredatorPreyDistance;
    public LayerMask PredatorMask;
    public LayerMask PreyMask;
    public float SphereCastRadius;

    public void Execute(int index)
    {
        float3 currentUnitPosition = UnitPositions[index];

        ObstacleChecks[index] = new SpherecastCommand(
                 currentUnitPosition, SphereCastRadius, UnitForwardDirections[index], ObstacleDistance * 0.85f, ObstacleMask);

        int IndexStart = SightDirections.Length * index;
        for (int i = 0; i < SightDirections.Length; i++)
        {
            float3 dir = UnitRotaions[index] * SightDirections[i];
            UnitsSightDirections[i + IndexStart] = dir;

            UnitSightDirectionsChecks[i + IndexStart] = new SpherecastCommand(
               currentUnitPosition, SphereCastRadius, dir, ObstacleDistance, ObstacleMask);

            UnitsPredatorsChecks[i + IndexStart] = new SpherecastCommand(
                currentUnitPosition, SphereCastRadius, dir, PredatorPreyDistance, PredatorMask);

            UnitsPreysChecks[i + IndexStart] = new SpherecastCommand(
                currentUnitPosition, SphereCastRadius, dir, PredatorPreyDistance, PreyMask);

            UnitsPredatorsPreyObstackleChecks[i + IndexStart] = new SpherecastCommand(
                currentUnitPosition, SphereCastRadius, dir, PredatorPreyDistance, ObstacleMask);
        }
    }
}

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
