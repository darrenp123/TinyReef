using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;

public struct PreyInfo
{
    public int size;
    public float3 impactPoint;
    public float distance;
}

[BurstCompile]
public struct MoveFlockJob : IJobParallelFor
{
    public NativeArray<float3> UnitsCurrentVelocities;
    // Not needed, normalized velocity is enough 
    [NativeDisableParallelForRestriction]
    public NativeArray<float3> UnitsForwardDirections;
    [NativeDisableParallelForRestriction]
    public NativeArray<float3> UnitsPositions;
    [NativeDisableParallelForRestriction]
    public NativeArray<int> UnitsCurrentWaypoints;

    [ReadOnly]
    public NativeArray<float> UnitsScales;
    [ReadOnly]
    public NativeArray<int> UnitsSizes;
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
    public NativeArray<PreyInfo> UnitsPreyInfo;
    [ReadOnly]
    public NativeArray<RaycastHit> UnitsPredatorPreyObtacleResults;
    [ReadOnly]
    public NativeArray<float3> UnitSightDirections;
    [ReadOnly]
    public NativeArray<float> UnitsCurrentHunger;
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
    public float HungerThreshold;
    public float SphereCastRadius;
    public NativeArray<Unity.Mathematics.Random> RandomRef;

    // for testing between 
    public int TestIndex;

    public void Execute(int index)
    {
        int cohesionNeighbours = 0;
        float3 cohesionVector = float3.zero;
        float3 avoidanceVector = float3.zero;
        float3 alignmnentVector = float3.zero;
        float3 currentUnitPosition = UnitsPositions[index];
        NativeHashMap<int, int> neighbourWaypointFrquency = new NativeHashMap<int, int>(1, Allocator.Temp)
        {
            { UnitsCurrentWaypoints[index], 1 }
        };

        float scaledCohesion = CohesionDistance * UnitsScales[index];
        float scaledAligement = AligementDistance * UnitsScales[index];
        float scaledAvoidance;

        for (int i = 0; i < UnitsPositions.Length; ++i)
        {
            float3 currentNeighbourPosition = UnitsPositions[i];

            if (!currentUnitPosition.Equals(currentNeighbourPosition) && IsInFov(index, currentNeighbourPosition))
            {
                float3 offset = currentNeighbourPosition - currentUnitPosition;
                float currentDistanceToNeighbourSqr = offset.x * offset.x + offset.y * offset.y + offset.z * offset.z;

                if (UnitsScales[index] > UnitsScales[i])
                {
                    scaledAvoidance = (AvoidanceDistance * UnitsScales[i]) + (SphereCastRadius * UnitsScales[i]);
                }
                else if (UnitsScales[index] == UnitsScales[i])
                {
                    scaledAvoidance = (AvoidanceDistance * UnitsScales[index]);
                }
                else
                    scaledAvoidance = (AvoidanceDistance * UnitsScales[index]) + (SphereCastRadius * UnitsScales[i]);


                if (scaledCohesion > 9)
                {
                    scaledCohesion = scaledAvoidance + 1;
                }

                if (scaledAligement > 9)
                {
                    scaledAligement = scaledAvoidance + 1;
                }

                if (currentDistanceToNeighbourSqr <= scaledCohesion * scaledCohesion)
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
                if (currentDistanceToNeighbourSqr <= scaledAvoidance * scaledAvoidance)
                {
                    float dist = math.max(currentDistanceToNeighbourSqr, 0.000001f);
                    avoidanceVector += (currentUnitPosition - currentNeighbourPosition) / dist;
                }
                if (currentDistanceToNeighbourSqr <= scaledAligement * scaledAligement)
                {
                    alignmnentVector += UnitsForwardDirections[i];
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
            if (UnitsSizes[index] <= 3)
                ObstacleAvoidanceWeight = 30;
            else if (UnitsSizes[index] <= 6)
                ObstacleAvoidanceWeight = 20;
            else
                ObstacleAvoidanceWeight = 10;

            float distancePercentage = 1 - distancetoObstacle / (ObstacleDistance * UnitsScales[index]);
            obstacleAvoidanceVector = SteerTowards(AvoidObstacleDirection(index), index)
                * ObstacleAvoidanceWeight * distancePercentage;
        }

        float3 predatorAvoidanceVector = float3.zero;
        float3 fleeDir = HasPredatorInSight(index, out float predatorDist);
        if (!fleeDir.Equals(float3.zero))
        {
            float distancePercentage = 1 - predatorDist / UnitsSightDistance[index];
            predatorAvoidanceVector = SteerTowards(fleeDir, index) * PredatorFleeWeight * distancePercentage;
        }

        float3 preyPersuitVector = float3.zero;
        if (UnitsCurrentHunger[index] <= HungerThreshold)
        {
            float preyDist = HasPreyInSight(index, out float3 persuitDir);
            if (preyDist > 0)
            {
                float distancePercentage = 1 - preyDist / UnitsSightDistance[index];
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

        float3 waypointVector = SteerTowards(GetWaypointDirection(index, center, ref waypointIndex), index) * PatrolWaypointWeight;

        if (!avoidanceVector.Equals(float3.zero))
        {
            avoidanceVector = SteerTowards(avoidanceVector, index) * AvoidanceWeight;
        }

        if (!alignmnentVector.Equals(float3.zero))
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

        if (!obstacleAvoidanceVector.Equals(float3.zero))
        {
            predatorAvoidanceVector = float3.zero;
        }

        float3 acceleration = cohesionVector + avoidanceVector + alignmnentVector + boundsVector + obstacleAvoidanceVector
            + predatorAvoidanceVector + preyPersuitVector + waypointVector;

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

    private float3 GetWaypointDirection(int index, in float3 neighboursCenter, ref int waypointIndex)
    {
        float3 dirToWaypoint = FlockWaypoints[waypointIndex] - neighboursCenter;
        float distanceSqr = dirToWaypoint.x * dirToWaypoint.x + dirToWaypoint.y * dirToWaypoint.y + dirToWaypoint.z * dirToWaypoint.z;
        float patrolDistanceScaled;

        if (UnitsSizes[index] <= 3)
            patrolDistanceScaled = PatrolWaypointDistance;
        else if (UnitsSizes[index] <= 7)
            patrolDistanceScaled = PatrolWaypointDistance * 2.5f;
        else
            patrolDistanceScaled = PatrolWaypointDistance * 4;

        if (distanceSqr < patrolDistanceScaled * patrolDistanceScaled)
        {
            Unity.Mathematics.Random random = RandomRef[index];
            waypointIndex = random.NextInt(0, FlockWaypoints.Length);
            RandomRef[index] = random;
            //waypointIndex = (waypointIndex + 1) % FlockWaypoints.Length;
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
            float angle = AngleBetween(UnitsForwardDirections[index], currentDirection);
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


    private float3 HasPredatorInSight(in int index, out float predatorDist)
    {
        // test with calculating predator future position
        float3 fleeDir = float3.zero;
        int particion = UnitsPredatorsResults.Length / UnitsPositions.Length;
        predatorDist = float.MaxValue;

        for (int i = particion * index; i < particion * (index + 1); i++)
        {
            RaycastHit currentCheck = UnitsPredatorsResults[i];
            float distanceTotarget = currentCheck.distance;
            float3 targetPosition = currentCheck.point;
            if (IsInFov(index, targetPosition) && UnitsPredatorPreyObtacleResults[i].distance == 0 && distanceTotarget > 0)
            {
                fleeDir += (UnitsPositions[index] - targetPosition) / distanceTotarget;
                if (distanceTotarget < predatorDist)
                {
                    predatorDist = distanceTotarget;
                }
            }
        }

        return fleeDir;
    }

    private float HasPreyInSight(in int index, out float3 persuitDir)
    {
        // test with calculating prey future position
        float minDistance = float.MaxValue;
        int maxSize = -1;
        float3 closestPrey = float3.zero;
        float targetDist = 0;
        int particion = UnitsPreyInfo.Length / UnitsPositions.Length;
        persuitDir = UnitsForwardDirections[index];

        for (int i = particion * index; i < particion * (index + 1); i++)
        {
            PreyInfo currentCheck = UnitsPreyInfo[i];
            float distanceTotarget = currentCheck.distance;
            int preySize = currentCheck.size;
            float3 targetPosition = currentCheck.impactPoint;

            if (IsInFov(index, targetPosition) && UnitsPredatorPreyObtacleResults[i].distance == 0 && distanceTotarget > 0
                && distanceTotarget < minDistance && preySize >= maxSize)
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
        return AngleBetween(UnitsForwardDirections[index], targetPosition - UnitsPositions[index]) <= FovAngle;
    }

    private float3 SteerTowards(in float3 vector, in int index)
    {
        float3 v = (math.normalize(vector) * UnitsMaxSpeed[index]) - UnitsCurrentVelocities[index];
        return math.length(v) > MaxSteerForce ? math.normalize(v) * MaxSteerForce : v;
    }

    private float AngleBetween(in float3 from, in float3 to)
    {
        float angle = math.acos(math.dot(from, to) / (math.length(from) * math.length(to)));
        return math.degrees(angle);
    }

    // private bool IsTestIndex(int index) => TestIndex == index;
}

// Old has predator calculation 
//private float HasPredatorInSight(in int index, out float3 fleeDir)
//{
//    // test with calculating predator future position
//    float minDistance = float.MaxValue;
//    float3 closestPredator = float3.zero;
//    float targetDist = 0;
//    int particion = UnitsPredatorsResults.Length / UnitsPositions.Length;
//    fleeDir = UnitsForwardDirections[index];

//    for (int i = particion * index; i < particion * (index + 1); i++)
//    {
//        RaycastHit currentCheck = UnitsPredatorsResults[i];
//        float distanceTotarget = currentCheck.distance;
//        float3 targetPosition = currentCheck.point;
//        if (IsInFov(index, targetPosition) && UnitsPredatorPreyObtacleResults[i].distance == 0 && distanceTotarget > 0
//            && distanceTotarget < minDistance)
//        {
//            minDistance = distanceTotarget;
//            closestPredator = targetPosition;
//        }
//    }

//    if (!closestPredator.Equals(float3.zero))
//    {
//        fleeDir = UnitsPositions[index] - closestPredator;
//        targetDist = minDistance;
//    }

//    return targetDist;
//}