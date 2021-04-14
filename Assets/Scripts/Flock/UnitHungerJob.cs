using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using UnityEngine;
using Unity.Mathematics;

[BurstCompile]
public struct UnitHungerJob : IJobParallelFor
{
    [ReadOnly]
    public NativeArray<float3> UnitPositions;
    [ReadOnly]
    public NativeArray<float3> UnitForwardDirections;

    public NativeArray<float> UnitsCurrentHunger;
    public NativeArray<float> UnitsStarvingTimer;
    public NativeArray<float> UnitsLifeSpan;

    [WriteOnly]
    public NativeArray<SpherecastCommand> UnitsEatChecks;

    public float SphereCastRadius;
    public float KillBoxDistance;
    public LayerMask PreyMask;
    public float TotalHunger;
    public float HungerThreshold;
    public float InitSarvingTimer;
    public float DeltaTime;

    public void Execute(int index)
    {
        UnitsCurrentHunger[index] -= DeltaTime;
        UnitsLifeSpan[index] -= DeltaTime;

        if (UnitsCurrentHunger[index] > HungerThreshold)
        {
            UnitsStarvingTimer[index] = InitSarvingTimer;
            UnitsEatChecks[index] = new SpherecastCommand();
        }
       else
        {
            if (UnitsCurrentHunger[index] < 0)
            {
                UnitsCurrentHunger[index] = 0;
            }

            UnitsStarvingTimer[index] -= DeltaTime;

            UnitsEatChecks[index] = new SpherecastCommand(
                UnitPositions[index], SphereCastRadius * 0.5f, UnitForwardDirections[index], KillBoxDistance, PreyMask);
        }
    }
}
