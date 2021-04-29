using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] private float matingUrge;
    [SerializeField] private int size;
    [SerializeField] private float[] fishScale = new float[10] { 1, 2.5f, 3.3f, 5.5f, 7.3f, 9.5f, 11.8f, 15.2f, 18, 21 };
    [SerializeField] private float seepIncumbrance;
    [SerializeField] private float sightIncumbrance;
    [SerializeField] private float sizeIncumbrance;
    [SerializeField] private ParticleSystem consumeEffectPrefab;

    public UnitEventSigniture OnUnitRemove;
    public UnitEventSigniture OnUnitTraitsValueChanged;

    private float _maxSpeed;
    private Vector3 _currentVelocity;
    private float _currrentHunger;
    private float _sightDistance;
    private float _lifeSpan;
    private float _scaledHungerThreshold;
    private ConsumablePool _consumablePool;
    private Material _fishMat;
    private Dictionary<int, string> _sizeToLayerDic;
    private LayerMask _predatorMask;
    private LayerMask _preyMask;
    private List<string> _predatorLayerNames;
    private List<string> _preyLayerNames;
    private float _currentMatingUrge;
    private float _matDefaultWigleSpeed;

    public Transform MyTransform { get; set; }
    public Vector3[] Directions { get; set; }

    public int Size { get => size; set => size = value; }
    public float FOVAngle => fovAngle;
    public int NumViewDirections => numViewDirections;
    public float KillBoxDistance => killBoxDistance;
    public float TotalHunger => totalHunger;
    public float CurrentHungerThreshold => _scaledHungerThreshold;
    public float InitStarvingTimer => initStarvingTimer;
    public float CurrrentHunger { get => _currrentHunger; set => _currrentHunger = value; }
    public Vector3 CurrentVelocity { get => _currentVelocity; set => _currentVelocity = value; }
    public float MaxSpeed => _maxSpeed;
    public float SightDistance => _sightDistance;
    public string UnitType => unitType;
    public string UnitName { get => unitName; set => unitName = value; }
    public float LifeSpan { get => _lifeSpan; set => _lifeSpan = value; }
    public float InitialLifespan => initialLifespan;
    public float CurrentMatingUrge => _currentMatingUrge;
    public LayerMask PredatorMask => _predatorMask;
    public LayerMask PreyMask => _preyMask;

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

        _sizeToLayerDic = new Dictionary<int, string>(10)
        {
            {1, "Sise_1" },
            {2, "Sise_2" },
            {3, "Sise_3" },
            {4, "Sise_4" },
            {5, "Sise_5" },
            {6, "Sise_6" },
            {7, "Sise_7" },
            {8, "Sise_8" },
            {9, "Sise_9" },
            {10, "Sise_10" },
        };

        _predatorLayerNames = new List<string>(10);
        _preyLayerNames = new List<string>(10);
    }

    public void Initialize(float speed, float sightDistance)
    {
        _maxSpeed = speed;
        _sightDistance = sightDistance;
        _currentVelocity = MyTransform.forward * speed;
        _lifeSpan = initialLifespan;

        _consumablePool = FindObjectOfType<ConsumablePool>();
        _fishMat = GetComponentInChildren<MeshRenderer>().material;
        _matDefaultWigleSpeed = _fishMat.GetFloat("_TimeScale");

        SetScale(size);
    }

    public void SetLifeSapan(float newLifeSapwn)
    {
        initialLifespan = newLifeSapwn;
    }

    // this two functions might need to run on the Initialize method
    public void SetMaxSpeed(float newSpeed)
    {
        _maxSpeed = newSpeed;
        UpdateHungerThreshold();
        OnUnitTraitsValueChanged?.Invoke(this);
    }

    public void SetSightDistance(float newSightDistance)
    {
        _sightDistance = newSightDistance;
        UpdateHungerThreshold();
        OnUnitTraitsValueChanged?.Invoke(this);
    }

    public void SetScale(int newSise)
    {
        // Testing
        //size = Random.Range(0, 10);
        //float fishSize = fishScale[size];
        _predatorLayerNames.Clear();
        _preyLayerNames.Clear();

        size = newSise;

        float fishSize = fishScale[Size - 1];
        MyTransform.localScale = new Vector3(fishSize, fishSize, fishSize);

        if (_fishMat)
        {
            float divisor;
            if (size <= 3)
                divisor = 1;
            else if (size <= 7)
                divisor = 1.5f;
            else
                divisor = 2;

            _fishMat.SetFloat("_TimeScale", _matDefaultWigleSpeed / divisor);
        }

        int predatorMinSize = size + 1;
        int preyMinSize = size - 1;
        foreach (KeyValuePair<int, string> sizeToLayer in _sizeToLayerDic)
        {
            if (predatorMinSize < sizeToLayer.Key /*&& sizeToLayer.Key - size < 9*/)
            {
                _predatorLayerNames.Add(sizeToLayer.Value);
            }

            if (preyMinSize > sizeToLayer.Key && size - sizeToLayer.Key < 8)
            {
                _preyLayerNames.Add(sizeToLayer.Value);
            }
        }

        if (size <= 3)
        {
            _preyLayerNames.Add("Coral");
        }

        gameObject.layer = LayerMask.NameToLayer(_sizeToLayerDic[size]);
        _predatorMask = LayerMask.GetMask(_predatorLayerNames.ToArray());
        _preyMask = LayerMask.GetMask(_preyLayerNames.ToArray());

        _currentMatingUrge = matingUrge * (1 + (size / 10));

        UpdateHungerThreshold();
        OnUnitTraitsValueChanged?.Invoke(this);
    }

    public void Consume()
    {
        OnUnitRemove?.Invoke(this);

        if (_consumablePool)
            _consumablePool.StartCoroutine(ManageConsumableEffect());
    }

    public float CalculateUnitFitness()
    {
        return _sightDistance + _maxSpeed + size + initialLifespan;
    }

    private void UpdateHungerThreshold()
    {
        _scaledHungerThreshold = hungerThreshold + (size * sizeIncumbrance) + (size * seepIncumbrance) + (size * sightIncumbrance);
    }

    public string GetFoodName() => unitName;

    private IEnumerator ManageConsumableEffect()
    {
        if (!_consumablePool) yield break;

        ParticleSystem consumeEffect = _consumablePool.GetItemFromPool(ItemPool.FOAM_BURST, consumeEffectPrefab)
            .GetComponent<ParticleSystem>();

        consumeEffect.transform.position = transform.position;
        consumeEffect.gameObject.SetActive(true);
        consumeEffect.Play();

        yield return new WaitWhile(() => consumeEffect.isPlaying);

        _consumablePool.ReturnToPool(ItemPool.FOAM_BURST, consumeEffect.gameObject);
    }
}
