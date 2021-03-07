using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleFlockUnit : MonoBehaviour
{
    [SerializeField] private float _FOVAngle;
    public float FOVAngle { get { return _FOVAngle; } }

    [SerializeField] private float _smoothDamp;
    public float smoothDamp { get { return _smoothDamp; } }

    private Vector3 _currentVelocity;
    public Vector3 currentVelocity { get; set; }

    private TestFlock assignedFlock;
    public float speed { get; set; }

    public Transform myTransform { get; set; }

    private void Awake()
    {
        myTransform = transform;
    }

    public void AssignFlock(TestFlock flock)
    {
        assignedFlock = flock;
    }

    public void InitializeSpeed(float speed)
    {
        this.speed = speed;
    }
}