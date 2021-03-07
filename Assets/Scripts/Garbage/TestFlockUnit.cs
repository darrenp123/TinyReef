using UnityEngine;

public class TestFlockUnit : MonoBehaviour
{
    [SerializeField] private float fovAngle;
    [SerializeField] private float smoothDamp;

    public Vector3 CurrentVelocity { get; set; }
    public TestFlock AssignedFlock { get; set; }
    public float Speed { get; set; }
    public Transform MyTransform { get; set; }

    public float SmoothDamp => smoothDamp;
    public float FOVAngle => fovAngle;

    public Vector3[] Directions;

    private void Awake()
    {
        MyTransform = transform;

        //if(Directions == null)
        //{
        //    Directions = new Vector3[numViewDirections];

        //    float goldenRatio = (1 + Mathf.Sqrt(5)) / 2;
        //    float angleIncrement = Mathf.PI * 2 * goldenRatio;

        //    for (int i = 0; i < numViewDirections; i++)
        //    {
        //        float t = (float)i / numViewDirections;
        //        float inclination = Mathf.Acos(1 - 2 * t);
        //        float azimuth = angleIncrement * i;

        //        float x = Mathf.Sin(inclination) * Mathf.Cos(azimuth);
        //        float y = Mathf.Sin(inclination) * Mathf.Sin(azimuth);
        //        float z = Mathf.Cos(inclination);
        //        Directions[i] = new Vector3(x, y, z);
        //    }
        //}
    }
}
