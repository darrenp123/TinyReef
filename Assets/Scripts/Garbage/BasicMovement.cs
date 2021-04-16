using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicMovement : MonoBehaviour
{
    float timeCounter = 0;
    public float speed, width, height;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        timeCounter += Time.deltaTime*speed;

        float x = Mathf.Cos(timeCounter)*width;
        float y = height;
        float z = Mathf.Sin(timeCounter)*width;

        transform.position = new Vector3(x, y, z);
    }
}
