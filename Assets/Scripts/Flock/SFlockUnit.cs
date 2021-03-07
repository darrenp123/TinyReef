using UnityEngine;

public class SFlockUnit : MonoBehaviour
{
    [Range(0, 180)]
    [SerializeField] private float fovAngle;
    [SerializeField] private int numViewDirections;
    [SerializeField] private float killBoxDistance;
    [SerializeField] private float hungerThreshold;

    public UnitEventSigniture OnUnitRemove;
    public UnitEventSigniture OnUnitTraitsValueChanged;

    public Transform MyTransform { get; set; }
    public int CurrentWaypoint { get; set; }
    public Vector3[] Directions { get; set; }

    public float FOVAngle => fovAngle;
    public int NumViewDirections => numViewDirections;
    public float KillBoxDistance => killBoxDistance;
    public UnitStates UnitState => _unitSate;
    public float HungerThreshold => hungerThreshold;
    public float CurrrentHunger { get => _currrentHunger; set => _currrentHunger = value; }
    public Vector3 CurrentVelocity { get => _currentVelocity; set => _currentVelocity = value; }
    public float MaxSpeed => _maxSpeed;
    public float SightDistance => _sightDistance;

    public delegate void UnitEventSigniture(SFlockUnit unitToRemove);

    private UnitStates _unitSate;
    private float _maxSpeed;
    private Vector3 _currentVelocity;
    private float _currrentHunger;
    private float _sightDistance;

    private void Awake()
    {
        MyTransform = transform;

        if (Directions == null)
        {
            Directions = new Vector3[numViewDirections];

            float goldenRatio = (1 + Mathf.Sqrt(5)) / 2;
            float angleIncrement = Mathf.PI * 2 * goldenRatio;

            for (int i = 0; i < numViewDirections; i++)
            {
                float t = (float)i / numViewDirections;
                float inclination = Mathf.Acos(1 - 2 * t);
                float azimuth = angleIncrement * i;

                float x = Mathf.Sin(inclination) * Mathf.Cos(azimuth);
                float y = Mathf.Sin(inclination) * Mathf.Sin(azimuth);
                float z = Mathf.Cos(inclination);
                Directions[i] = new Vector3(x, y, z);
            }
        }
    }

    public void Initialize(float speed, float sightDistance)
    {
        _maxSpeed = speed;
        _sightDistance = sightDistance;
        _currentVelocity = MyTransform.forward * speed;
        _unitSate = UnitStates.PATROLING;
    }

    public void SetMaxSpeed()
    {
        _maxSpeed++;
        OnUnitTraitsValueChanged?.Invoke(this);
    }

    public void SetSightDistance()
    {
        _sightDistance++;
        OnUnitTraitsValueChanged?.Invoke(this);
    }

    public void RemoveUnit()
    {
        OnUnitRemove?.Invoke(this);
    }
}
