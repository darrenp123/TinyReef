using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;

// this job main responsibility is to calculate the movement of each fish of the flock that instantiates this job
// to achieve the basic flock movement behavior the main ideas from boids algorithm were used 

// helper struct to better have information on a prey
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

    // for testing  
    public int TestIndex;

    public void Execute(int index)
    {
        int cohesionNeighbours = 0;
        float3 cohesionVector = float3.zero;
        float3 avoidanceVector = float3.zero;
        float3 alignmnentVector = float3.zero;
        float3 currentUnitPosition = UnitsPositions[index];

        // a fish choses were to go based on the majority of its neighbors destination, this data structure keeps
        // track of that by having each destination related with the number of neighbors going there
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

                // because fish with different sizes can flock with each other, some calculations must be made 
                // to ensure that the different size fish flock well
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

                    // destination frequency in neighbors
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
                    // small calculations for the avoidance vector, in this case the closest a neighboring fish
                    // is the stronger the avoidance force from that fish will be
                    float dist = math.max(currentDistanceToNeighbourSqr, 0.000001f);
                    avoidanceVector += (currentUnitPosition - currentNeighbourPosition) / dist;
                }
                if (currentDistanceToNeighbourSqr <= scaledAligement * scaledAligement)
                {
                    alignmnentVector += UnitsForwardDirections[i];
                }
            }
        }

        // if a fish leaves a certain bounds volume, this vector tries steer it towards the center of the SFlock obj
        float3 offsetToCenter = FlockPosition - UnitsPositions[index];
        bool isInBounds = offsetToCenter.x * offsetToCenter.x + offsetToCenter.y * offsetToCenter.y
            + offsetToCenter.z * offsetToCenter.z < BoundsDistance * BoundsDistance * 0.9f;
        float3 boundsVector = (!isInBounds ? SteerTowards(offsetToCenter, index) : float3.zero)
            * BoundsWeight;

        // calculate the direction vector to take to avoid an obstacle
        float3 obstacleAvoidanceVector = float3.zero;
        float distancetoObstacle = UnitsObstacleResults[index].distance;
        if (distancetoObstacle > 0)
        {
            // set the wight of the obstacle avoidance vector based on the fish size
            if (UnitsSizes[index] <= 3)
                ObstacleAvoidanceWeight = 30;
            else if (UnitsSizes[index] <= 6)
                ObstacleAvoidanceWeight = 20;
            else
                ObstacleAvoidanceWeight = 10;
            // use distance from object percentage, so the turning force strength is dependent on that distance 
            float distancePercentage = 1 - distancetoObstacle / (ObstacleDistance * UnitsScales[index]);
            obstacleAvoidanceVector = SteerTowards(AvoidObstacleDirection(index), index)
                * ObstacleAvoidanceWeight * distancePercentage;
        }

        // calculate the predator avoidance vector if a fish spots a predator, it gives a vector in the
        // opposite direction of the fish to predator
        float3 predatorAvoidanceVector = float3.zero;
        float3 fleeDir = HasPredatorInSight(index, out float predatorDist);
        if (!fleeDir.Equals(float3.zero))
        {
            // force also based on distance, in these case the distance to the predator 
            float distancePercentage = 1 - predatorDist / UnitsSightDistance[index];
            predatorAvoidanceVector = SteerTowards(fleeDir, index) * PredatorFleeWeight * distancePercentage;
        }

        // very similar to the predator avoidance vector, but the direction is the other way around, also it
        // first checks to see if the fish is hungry 
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
                // check for the most common destination within those neighbors
                waypointIndex = NeighboursFrequentWaypoint(neighbourWaypointFrquency);
            }
            // calculate cohesion vector, it tells the fish to go to the center of the flock
            cohesionVector /= cohesionNeighbours;
            cohesionVector -= UnitsPositions[index];
            cohesionVector = SteerTowards(cohesionVector, index) * CohesionWeight;
        }

        // calculate direction vector to the current waypoit
        float3 waypointVector = SteerTowards(GetWaypointDirection(index, center, ref waypointIndex), index) * PatrolWaypointWeight;

        // calculate the avoidance and alignment direction vector
        if (!avoidanceVector.Equals(float3.zero))
        {
            avoidanceVector = SteerTowards(avoidanceVector, index) * AvoidanceWeight;
        }

        if (!alignmnentVector.Equals(float3.zero))
        {
            alignmnentVector = SteerTowards(alignmnentVector, index) * AligementWeight;
        }

        // in case there is a predator, ignore the prey and bounds vector
        if (!predatorAvoidanceVector.Equals(float3.zero))
        {
            preyPersuitVector = float3.zero;
            boundsVector = float3.zero;
            //waypointVector = float3.zero;
        }

        // in it finds a prey, ignore the waypoint, bounds, cohesion and alignment vector
        if (!preyPersuitVector.Equals(float3.zero))
        {
            waypointVector = float3.zero;
            boundsVector = float3.zero;
            cohesionVector = float3.zero;
            alignmnentVector = float3.zero;
        }

        // if there is an object in the way ignore the predator vector
        if (!obstacleAvoidanceVector.Equals(float3.zero))
        {
            predatorAvoidanceVector = float3.zero;
        }

        // some all previously calculated direction vectors to add to the current velocity vector
        float3 acceleration = cohesionVector + avoidanceVector + alignmnentVector + boundsVector + obstacleAvoidanceVector
            + predatorAvoidanceVector + preyPersuitVector + waypointVector;

        float3 currVel = UnitsCurrentVelocities[index] + (acceleration * DeltaTime);
        float speed = math.length(currVel);
        speed = math.clamp(speed, MinSpeed, UnitsMaxSpeed[index]);
        currVel = math.normalize(currVel) * speed;

        // set variables with the calculated values, so we set them later on the actual fish
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

        // scale necessary distance to waypoint based on size 
        if (UnitsSizes[index] <= 3)
            patrolDistanceScaled = PatrolWaypointDistance;
        else if (UnitsSizes[index] <= 7)
            patrolDistanceScaled = PatrolWaypointDistance * 2.5f;
        else
            patrolDistanceScaled = PatrolWaypointDistance * 4;

        // get random waypoint
        if (distanceSqr < patrolDistanceScaled * patrolDistanceScaled)
        {
            Unity.Mathematics.Random random = RandomRef[index];
            waypointIndex = random.NextInt(0, FlockWaypoints.Length);
            RandomRef[index] = random;
            // uncomment to get predictable waypoints
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
        // in jobs we cant have an array of arrays, so we have just one array with all the information
        // and we use some math to access that array at the right element for that fish
        int particion = UnitsObstacleSightResults.Length / UnitsPositions.Length;

        //calculate what is the obstacle avoidance path that as the minimum deviation from the fish current
        //trajectory. Also, if all directions have obstacles in front, get the one that is
        //furthest away from the fish 
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
        // can also experiment with calculating predator future position
        float3 fleeDir = float3.zero;
        int particion = UnitsPredatorsResults.Length / UnitsPositions.Length;
        predatorDist = float.MaxValue;

        // gets all the predators that are in sight and gets a flee direction from them
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
        // can also experiment with calculating prey future position
        float minDistance = float.MaxValue;
        int maxSize = -1;
        float3 closestPrey = float3.zero;
        float targetDist = 0;
        int particion = UnitsPreyInfo.Length / UnitsPositions.Length;
        persuitDir = UnitsForwardDirections[index];

        // get the closest biggest prey in sight
        for (int i = particion * index; i < particion * (index + 1); i++)
        {
            PreyInfo currentCheck = UnitsPreyInfo[i];
            float distanceTotarget = currentCheck.distance;
            int preySize = currentCheck.size;
            float3 targetPosition = currentCheck.impactPoint;

            // should check and test for the prey size
            if (IsInFov(index, targetPosition) && UnitsPredatorPreyObtacleResults[i].distance == 0 && distanceTotarget > 0
                && distanceTotarget < minDistance && preySize >= maxSize)
            {
                minDistance = distanceTotarget;
                closestPrey = targetPosition;
                maxSize = preySize;
            }
        }

        // calculate the direction to the chosen prey
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