using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class timetrigger : MonoBehaviour
{
    public float timemins;
    float timesecs;
    float timenow;
    float nextrecord;

    // Start is called before the first frame update
    void Start()
    {
        timesecs = timemins * 60;
        //nextrecord = Time.time + timesecs;
        nextrecord = Time.time;
        Debug.Log(nextrecord);
        //GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
        //recorder.record(allObjects);
        //Debug.Log("Initial Write");
    }

    void timeTrigger()
    {
        timenow = Time.time;

        if (timenow >= nextrecord)
        {
            //GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
            GameObject[] allObjects = GameObject.FindGameObjectsWithTag("Fish");
            recorder2.record(allObjects);
            nextrecord = timenow + timesecs;
            Debug.Log("Update new line");
            //parser.readdata(1);
            //parser.request("Cod(Clone) ", 1);
            parser2.getnames();
            parser2.gettimes();
            parser2.getalldata();
            parser2.gettimestep();
            //parser2.setlistoftimes();
            //parser2.setfishnametimes();
            parser2.sorting();
            //recorder.record(allObjects);
            allObjects = null;
        }
    }

    void consoleOutput()
    {
        //Debug.Log();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        timeTrigger();
    }
}
