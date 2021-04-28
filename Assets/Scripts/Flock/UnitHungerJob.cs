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
    [ReadOnly]
    public NativeArray<float> UnitsScales;
    [ReadOnly]
    public NativeArray<LayerMask> UnitsPreyMask;
    [ReadOnly]
    public NativeArray<float> UnitsHungerThreshold;

    public NativeArray<float> UnitsCurrentHunger;
    public NativeArray<float> UnitsStarvingTimer;
    public NativeArray<float> UnitsLifeSpan;
    public NativeArray<float> UnitsMatingHurge;

   [WriteOnly]
    public NativeArray<SpherecastCommand> UnitsEatChecks;

    public float SphereCastRadius;
    public float KillBoxDistance;
    public float TotalHunger;
    public float InitSarvingTimer;
    public float DeltaTime;

    public void Execute(int index)
    {
        UnitsCurrentHunger[index] -= DeltaTime;
        UnitsLifeSpan[index] -= DeltaTime;
        UnitsMatingHurge[index] -= DeltaTime;

        if (UnitsCurrentHunger[index] > UnitsHungerThreshold[index])
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
                UnitPositions[index], SphereCastRadius * UnitsScales[index] * 0.5f,
                UnitForwardDirections[index], KillBoxDistance * UnitsScales[index], UnitsPreyMask[index]);
        }
    }
}
