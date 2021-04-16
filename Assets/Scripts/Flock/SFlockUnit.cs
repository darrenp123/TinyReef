using UnityEngine;

public class SFlockUnit : MonoBehaviour, IFood
{
    [Range(0, 180)]
    [SerializeField] private float fovAngle;
    [SerializeField] private int numViewDirections;
    [SerializeField] private float killBoxDistance;
    [SerializeField] private float totalHunger;
    [SerializeField] private float hungerThreshold;
    [SerializeField] private float initStarvingTimer;
    [SerializeField] private float initialLifespan;
    [SerializeField] private string unitType;
    [SerializeField] private string unitName;
    [SerializeField] private int size;

    public UnitEventSigniture OnUnitRemove;
    public UnitEventSigniture OnUnitTraitsValueChanged;


    private float _maxSpeed;
    private Vector3 _currentVelocity;
    private float _currrentHunger;
    private float _sightDistance;
    private float _initialSize;
    private float _lifeSpan;

    public Transform MyTransform { get; set; }
    public int CurrentWaypoint { get; set; }
    public Vector3[] Directions { get; set; }

    public int Size { get => size; set => size = value; }
    public float FOVAngle => fovAngle;
    public int NumViewDirections => numViewDirections;
    public float KillBoxDistance => killBoxDistance;
    public float TotalHunger => totalHunger;
    public float HungerThreshold => hungerThreshold;
    public float InitStarvingTimer => initStarvingTimer;
    public float CurrrentHunger { get => _currrentHunger; set => _currrentHunger = value; }
    public Vector3 CurrentVelocity { get => _currentVelocity; set => _currentVelocity = value; }
    public float MaxSpeed => _maxSpeed;
    public float SightDistance => _sightDistance;
    public string UnitType => unitType;
    public string UnitName { get => unitName; set => unitName = value; }
    public float LifeSpan { get => _lifeSpan; set => _lifeSpan = value; }
    public float InitialLifespan => initialLifespan;
    private float[] fishScale = new float[10] { 1, 3, 5, 7, 9, 11, 13, 15, 17, 20 };

    public delegate void UnitEventSigniture(SFlockUnit unitToRemove);

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
        ScaleFish();
    }

    public void Initialize(float speed, float sightDistance)
    {
        _maxSpeed = speed;
        _sightDistance = sightDistance;
        _currentVelocity = MyTransform.forward * speed;
        _initialSize = Size;
        _lifeSpan = initialLifespan;
    }

    public void SetMaxSpeed(int deltaValue)
    {
        _maxSpeed += deltaValue;
        OnUnitTraitsValueChanged?.Invoke(this);
    }

    public void SetSightDistance(int deltaValue)
    {
        _sightDistance += deltaValue;
        OnUnitTraitsValueChanged?.Invoke(this);
    }

    public void ScaleFish()
    {
        /*
        Debug.Log(Size);
        float sizeChange = (Size - _initialSize) / 10f;
        Debug.Log(sizeChange);
        Vector3 scaleChange = new Vector3(sizeChange, sizeChange, sizeChange);
        Debug.Log(Vector3.one + scaleChange);
        MyTransform.localScale = Vector3.one + scaleChange;
        */
        MyTransform.localScale = new Vector3(fishScale[Size - 1], fishScale[Size - 1], fishScale[Size - 1]);
    }

    private void RemoveUnit()
    {
        OnUnitRemove?.Invoke(this);
    }

    public void Consume()
    {
        RemoveUnit();
    }
}
