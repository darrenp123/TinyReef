using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlockUinit : MonoBehaviour
{
    [SerializeField] private float FOVAngle;
    [SerializeField] private float smoothDamp;
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private Vector3[] checkObstacleDrections;

    public Transform MyTransform { get; set; }

    private List<FlockUinit> _cohesionNeighbours = new List<FlockUinit>();
    private List<FlockUinit> _avoidanceNeighbours = new List<FlockUinit>();
    private List<FlockUinit> _alignmentNeighbours = new List<FlockUinit>();
    private Flock _assignedFlock;
    private Vector3 _currentVelocity = Vector3.zero;
    private float _speed;

    public float GetFOVAngle => FOVAngle;
    public float SmoothDamp => smoothDamp;
    public Flock AssignedFlock { set => _assignedFlock = value; }
    public Vector3 CurrentVelocity { get => _currentVelocity; set => _currentVelocity = value; }
    public float Speed { get => _speed; set => _speed = value; }

    private void Awake()
    {
        MyTransform = transform;
    }
    
    public void MoveUnit()
    {
        FindNeighbours();
        CalculateSpeed();

        Vector3 cohesionVector = CalculateCohesionVector() * _assignedFlock.CohesionWeight;
        Vector3 avoidanceVector = CalculateAvoidanceVector() * _assignedFlock.AvoidanceWeight;
        Vector3 alignmentVector = CalculateAlignmentVector() * _assignedFlock.AlignmentWeight;
        Vector3 obstacleAvoidanceVector = CalculateObstacleAvoidanceVector() * _assignedFlock.ObstacleAvoidanceWeight;
        Vector3 boundsVector = CalculateBoundsVector() * _assignedFlock.BoundsWeight;

        Vector3 moveVector = cohesionVector + avoidanceVector + alignmentVector /*+ boundsVector + obstacleAvoidanceVector*/;
        moveVector = Vector3.SmoothDamp(MyTransform.forward, moveVector, ref _currentVelocity, smoothDamp);
        MyTransform.forward = moveVector.normalized * _speed;
        MyTransform.position += MyTransform.forward * Time.deltaTime;
    }

    private void FindNeighbours()
    {
        _cohesionNeighbours.Clear();
        _avoidanceNeighbours.Clear();
        _alignmentNeighbours.Clear();
        foreach (FlockUinit currentUnit in _assignedFlock.AllUnits)
        {
            if (currentUnit != this)
            {
                float currentNeighbourDistanceSqr =
                    Vector3.SqrMagnitude(currentUnit.MyTransform.position - MyTransform.position);

                if (currentNeighbourDistanceSqr <= _assignedFlock.CohesionDistance * _assignedFlock.CohesionDistance)
                {
                    _cohesionNeighbours.Add(currentUnit);
                }
                if (currentNeighbourDistanceSqr <= _assignedFlock.AvoidanceDistance * _assignedFlock.AvoidanceDistance)
                {
                    _avoidanceNeighbours.Add(currentUnit);
                }
                if (currentNeighbourDistanceSqr <= _assignedFlock.AlignmentDistance * _assignedFlock.AlignmentDistance)
                {
                    _alignmentNeighbours.Add(currentUnit);
                }
            }
        }
    }

    private void CalculateSpeed()
    {
        if (_cohesionNeighbours.Count == 0) return;

        _speed = 0;
        foreach (FlockUinit neighbor in _cohesionNeighbours)
        {
            _speed += neighbor.Speed;
        }

        _speed /= _cohesionNeighbours.Count;
        _speed = Mathf.Clamp(_speed, _assignedFlock.MinSpeed, _assignedFlock.MaxSpeed);
    }

    private Vector3 CalculateCohesionVector()
    {
        var cohesionVector = Vector3.zero;
        int neighboursInFOV = 0;

        foreach (FlockUinit neighbor in _cohesionNeighbours)
        {
            if (IsInFOV(neighbor.MyTransform.position))
            {
                ++neighboursInFOV;
                cohesionVector += neighbor.MyTransform.position;
            }
        }

        if (neighboursInFOV == 0)
            return cohesionVector;

        cohesionVector /= neighboursInFOV;
        cohesionVector -= MyTransform.position;
        return cohesionVector.normalized;
    }

    private Vector3 CalculateAvoidanceVector()
    {
        var avoidanceVector = Vector3.zero;
        int neighboursInFOV = 0;

        foreach (FlockUinit neighbor in _avoidanceNeighbours)
        {
            if (IsInFOV(neighbor.MyTransform.position))
            {
                ++neighboursInFOV;
                avoidanceVector += MyTransform.position - neighbor.MyTransform.position;
            }
        }

        if (neighboursInFOV == 0)
            return avoidanceVector;

        avoidanceVector /= neighboursInFOV;
        return avoidanceVector.normalized;
    }

    private Vector3 CalculateAlignmentVector()
    {
        var alignmnentVector = MyTransform.forward;
        int neighboursInFOV = 0;

        foreach (FlockUinit neighbor in _alignmentNeighbours)
        {
            if (IsInFOV(neighbor.MyTransform.position))
            {
                ++neighboursInFOV;
                alignmnentVector += neighbor.MyTransform.forward;
            }
        }

        if (neighboursInFOV == 0)
            return alignmnentVector;

        alignmnentVector /= neighboursInFOV;
        return alignmnentVector.normalized;
    }

    private Vector3 CalculateBoundsVector()
    {
        Vector3 offsetToCenter = _assignedFlock.transform.position - MyTransform.position;
        return offsetToCenter.magnitude >= _assignedFlock.BoundsDistance * 0.9f ? offsetToCenter.normalized : Vector3.zero;
    }

    private Vector3 CalculateObstacleAvoidanceVector()
    {
        Vector3 obstacleAvoidanceVector = Vector3.zero;
        if (Physics.Raycast(MyTransform.position, MyTransform.forward, out _, _assignedFlock.ObstacleAvoidanceDistance, obstacleMask))
        {
            obstacleAvoidanceVector = AvoidObstacleDirection();
        }
        
        return obstacleAvoidanceVector;
    }

    private Vector3 AvoidObstacleDirection()
    {
        float maxDistance = int.MinValue;
        Vector3 selectedDirection = Vector3.zero;

        foreach (Vector3 directionToCheck in checkObstacleDrections)
        {
            Vector3 checkDirection = MyTransform.TransformDirection(directionToCheck.normalized);
            if (Physics.Raycast(MyTransform.position, checkDirection, out RaycastHit hit, _assignedFlock.ObstacleAvoidanceDistance, obstacleMask))
            {
                float currentDistance = (hit.point - MyTransform.position).sqrMagnitude;
                if ((hit.point - MyTransform.position).sqrMagnitude > maxDistance)
                {
                    maxDistance = currentDistance;
                    selectedDirection = checkDirection;
                }
            }
            else
            {
                selectedDirection = checkDirection;
                break;
            }
        }

        return selectedDirection.normalized;
    }

    private bool IsInFOV(Vector3 position)
    {
        return Vector3.Angle(MyTransform.forward, position - MyTransform.position) <= FOVAngle;
    }
}
