using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;

[BurstCompile]
public struct MoveFlockJob : IJobParallelFor
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
    public NativeArray<float> UnitsMaxSpeed;
    [ReadOnly]
    public NativeArray<float> UnitsSightDistance;
    [ReadOnly]
    public NativeArray<RaycastHit> UnitsObstacleResults;
    [ReadOnly]
    public NativeArray<RaycastHit> UnitsObstacleSightResults;
    [ReadOnly]
    public NativeArray<RaycastHit> UnitsPredatorsResults;
    [ReadOnly]
    public NativeArray<RaycastHit> UnitsPreyResults;
    [ReadOnly]
    public NativeArray<RaycastHit> UnitsPredatorPreyObtacleResults;
    [ReadOnly]
    public NativeArray<float3> UnitSightDirections;
    [ReadOnly]
    public NativeArray<float> UnitsHungerTimer;
    [ReadOnly]
    public NativeArray<float3> FlockWaypoints;

    public float CohesionDistance;
    public float AvoidanceDistance;
    public float AligementDistance;
    public float PatrolWaypointDistance;
    public float ObstacleDistance;
    public float BoundsDistance;

    public float CohesionWeight;
    public float AvoidanceWeight;
    public float AligementWeight;
    public float PatrolWaypointWeight;
    public float ObstacleAvoidanceWeight;
    public float PredatorFleeWeight;
    public float PreyPersuitWeight;
    public float BoundsWeight;

    public float3 FlockPosition;
    public float MaxSteerForce;
    public float FovAngle;
    public float MinSpeed;
    public float DeltaTime;

    // for testing
    public int TestIndex;

    public void Execute(int index)
    {
        float3 cohesionVector = float3.zero;
        int cohesionNeighbours = 0;
        float3 avoidanceVector = float3.zero;
        int AvoidanceNeighbours = 0;
        float3 alignmnentVector = float3.zero;
        int AlignmentNeighbours = 0;
        float3 currentUnitPosition = UnitsPositions[index];
        NativeHashMap<int, int> neighbourWaypointFrquency = new NativeHashMap<int, int>(1, Allocator.Temp)
        {
            { UnitsCurrentWaypoints[index], 1 }
        };

        for (int i = 0; i < UnitsPositions.Length; i++)
        {
            float3 currentNeighbourPosition = UnitsPositions[i];

            if (!currentUnitPosition.Equals(currentNeighbourPosition))
            {
                float3 offset = currentNeighbourPosition - currentUnitPosition;
                float currentDistanceToNeighbourSqr = offset.x * offset.x + offset.y * offset.y + offset.z * offset.z;

                if (currentDistanceToNeighbourSqr <= CohesionDistance * CohesionDistance)
                {
                    if (IsInFov(index, currentNeighbourPosition))
                    {
                        cohesionNeighbours++;
                        cohesionVector += currentNeighbourPosition;

                        int neighbourCurrentWaypoint = UnitsCurrentWaypoints[i];
                        if (neighbourWaypointFrquency.ContainsKey(neighbourCurrentWaypoint))
                        {
                            int frequency = neighbourWaypointFrquency[neighbourCurrentWaypoint];
                            neighbourWaypointFrquency[neighbourCurrentWaypoint] = ++frequency;
                        }
                        else
                        {
                            neighbourWaypointFrquency.Add(neighbourCurrentWaypoint, 1);
                        }
                    }
                }
                if (currentDistanceToNeighbourSqr <= AvoidanceDistance * AvoidanceDistance)
                {
                    if (IsInFov(index, currentNeighbourPosition))
                    {
                        AvoidanceNeighbours++;
                        float dist = math.max(currentDistanceToNeighbourSqr, 0.000001f);
                        avoidanceVector -= (currentNeighbourPosition - currentUnitPosition) / dist;
                    }
                }
                if (currentDistanceToNeighbourSqr <= AligementDistance * AligementDistance)
                {
                    if (IsInFov(index, currentNeighbourPosition))
                    {
                        AlignmentNeighbours++;
                        alignmnentVector += UnitsForwardDirections[i];
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
        var distancetoObstacle = UnitsObstacleResults[index].distance;
        if (distancetoObstacle > 0)
        {
            float distancePercentage = 1 - distancetoObstacle / ObstacleDistance;
            obstacleAvoidanceVector = SteerTowards(AvoidObstacleDirection(index), index)
                * ObstacleAvoidanceWeight * distancePercentage;
        }

        float3 predatorAvoidanceVector = float3.zero;
        float predatorDist = HasPredatorInSight(index, out float3 fleeDir);
        if (predatorDist > 0)
        {
            float distancePercentage = 1 - predatorDist / UnitsSightDistance[index];
            predatorAvoidanceVector = SteerTowards(fleeDir, index) * PredatorFleeWeight * distancePercentage;
        }

        float3 preyPersuitVector = float3.zero;
        if (UnitsHungerTimer[index] <= 0)
        {
            float targetDist = HasPreyInSight(index, out float3 persuitDir);
            if (targetDist > 0)
            {
                float distancePercentage = 1 - targetDist / UnitsSightDistance[index];
                preyPersuitVector = SteerTowards(persuitDir, index) * PreyPersuitWeight * distancePercentage;
            }
        }

        int waypointIndex = UnitsCurrentWaypoints[index];
        float3 center = (cohesionVector + UnitsPositions[index]) / (cohesionNeighbours + 1);
        if (cohesionNeighbours > 0)
        {
            if (neighbourWaypointFrquency.Count() > 1)
            {
                waypointIndex = NeighboursFrequentWaypoint(neighbourWaypointFrquency);
            }

            cohesionVector /= cohesionNeighbours;
            cohesionVector -= UnitsPositions[index];
            cohesionVector = SteerTowards(cohesionVector, index) * CohesionWeight;
        }

        float3 waypointVector = SteerTowards(GetWaypointDirection(center, ref waypointIndex), index) * PatrolWaypointWeight;

        if (AvoidanceNeighbours > 0)
        {
            avoidanceVector = SteerTowards(avoidanceVector, index) * AvoidanceWeight;
        }

        if (AlignmentNeighbours > 0)
        {
            alignmnentVector = SteerTowards(alignmnentVector, index) * AligementWeight;
        }

        if (!predatorAvoidanceVector.Equals(float3.zero))
        {
            preyPersuitVector = float3.zero;
            boundsVector = float3.zero;
            //waypointVector = float3.zero;
        }

        if (!preyPersuitVector.Equals(float3.zero))
        {
            waypointVector = float3.zero;
            boundsVector = float3.zero;
            cohesionVector = float3.zero;
            alignmnentVector = float3.zero;
        }

        float3 acceleration = cohesionVector + avoidanceVector + alignmnentVector + boundsVector
            + obstacleAvoidanceVector + predatorAvoidanceVector + preyPersuitVector + waypointVector;

        float3 currVel = UnitsCurrentVelocities[index] + (acceleration * DeltaTime);
        float speed = math.length(currVel);
        speed = math.clamp(speed, MinSpeed, UnitsMaxSpeed[index]);
        currVel = math.normalize(currVel) * speed;

        UnitsPositions[index] += currVel * DeltaTime;
        UnitsForwardDirections[index] = math.normalize(currVel);
        UnitsCurrentVelocities[index] = currVel;
        UnitsCurrentWaypoints[index] = waypointIndex;

        neighbourWaypointFrquency.Dispose();
    }

    private float3 GetWaypointDirection(in float3 neighboursCenter, ref int waypointIndex)
    {
        float3 dirToWaypoint = FlockWaypoints[waypointIndex] - neighboursCenter;
        float distanceSqr = dirToWaypoint.x * dirToWaypoint.x + dirToWaypoint.y * dirToWaypoint.y + dirToWaypoint.z * dirToWaypoint.z;
        if (distanceSqr < PatrolWaypointDistance * PatrolWaypointDistance)
        {
            waypointIndex = (waypointIndex + 1) % FlockWaypoints.Length;
        }

        return FlockWaypoints[waypointIndex] - neighboursCenter;
    }

    private int NeighboursFrequentWaypoint(in NativeHashMap<int, int> neighbourWaypointFrquency)
    {
        int waypointIndex = 0;
        int minCount = 0;

        foreach (var waypointFrquency in neighbourWaypointFrquency)
        {
            if (minCount <= waypointFrquency.Value)
            {
                if (minCount == waypointFrquency.Value && waypointIndex > waypointFrquency.Key)
                    continue;

                minCount = waypointFrquency.Value;
                waypointIndex = waypointFrquency.Key;
            }
        }

        return waypointIndex;
    }

    private float3 AvoidObstacleDirection(in int index)
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
            (!farthestContestedDirection.Equals(float3.zero) ? farthestContestedDirection : UnitsForwardDirections[index]);
    }

    private float HasPredatorInSight(in int index, out float3 fleeDir)
    {
        // test with calculating predator future position
        float minDistance = float.MaxValue;
        float3 closestPredator = float3.zero;
        float targetDist = 0;
        int particion = UnitsPredatorsResults.Length / UnitsPositions.Length;
        fleeDir = UnitsForwardDirections[index];

        for (int i = particion * index; i < particion * (index + 1); i++)
        {
            RaycastHit currentCheck = UnitsPredatorsResults[i];
            float distanceTotarget = currentCheck.distance;
            float3 targetPosition = currentCheck.point;
            if (IsInFov(index, targetPosition) && UnitsPredatorPreyObtacleResults[i].distance == 0 && distanceTotarget > 0
                && distanceTotarget < minDistance)
            {
                minDistance = distanceTotarget;
                closestPredator = targetPosition;
            }
        }

        if (!closestPredator.Equals(float3.zero))
        {
            fleeDir = UnitsPositions[index] - closestPredator;
            targetDist = minDistance;
        }

        return targetDist;
    }

    private float HasPreyInSight(in int index, out float3 persuitDir)
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
            if (IsInFov(index, targetPosition) && UnitsPredatorPreyObtacleResults[i].distance == 0 && distanceTotarget > 0
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

    private bool IsInFov(in int index, in float3 targetPosition)
    {
        return Vector3.Angle(UnitsForwardDirections[index], targetPosition - UnitsPositions[index]) <= FovAngle;
    }

    private float3 SteerTowards(in float3 vector, in int index)
    {
        float3 v = (math.normalize(vector) * UnitsMaxSpeed[index]) - UnitsCurrentVelocities[index];
        return math.length(v) > MaxSteerForce ? math.normalize(v) * MaxSteerForce : v;
    }

    private bool IsTestIndex(int index) => TestIndex == index;
}