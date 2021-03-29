using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;

[BurstCompile]
public struct UnitHungerJob : IJobParallelFor
{
    [NativeDisableParallelForRestriction]
    public NativeArray<float> UnitsCurrentHunger;
    [NativeDisableParallelForRestriction]
    public NativeArray<float> UnitsStarvingTimer;
    [NativeDisableParallelForRestriction]
    public NativeArray<float> UnitsLifeSpan;

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
        }
       else
        {
            if (UnitsCurrentHunger[index] < 0)
            {
                UnitsCurrentHunger[index] = 0;
            }

            UnitsStarvingTimer[index] -= DeltaTime;
        }
    }
}
