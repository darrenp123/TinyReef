using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class parser : MonoBehaviour
{

    private static string m_path = "Assets/Stats/";

    private static List<string> alldata = new List<string>();
    private static List<float> outputdata = new List<float>();
    public List<float> oneminrequest = new List<float>();

    static public string requesttype = "";
    static public float requesttime = 0f;

    public bool writeyn = false;

    static public string tempname = "";
    static public float temptime = 0f;
    static public float tempnum = 0f;
    static public float lasttime = 0f;
    static public bool firstvalueset = false;

    static public float totalforaverage;
    static public float counter;
    static public float averagevalue;

    // Start is called before the first frame update
    void Start()
    {
        //test values
        requesttype = "Cod(Clone) ";
        requesttime = 1f;
    }

    static public void request(string type, float time)
    {
        requesttype = type;
        requesttime = time;

        readdata(time);
    }

    static public void readdata(float t)
    {
        string fullPath = m_path + "gameobjectname.txt";
        StreamReader reader = new StreamReader(fullPath);
        string alldataline = "";
        while ((alldataline = reader.ReadLine()) != null)
        {//going through the text file line by line and adding it to a list of vectors.
            //alldata.Add(alldataline);
            //alldataline = "";
            onemindata(alldataline);
            alldataline = "";
        }

        foreach(var x in outputdata)
        {
           Debug.Log(x.ToString());
        }
    }


    static public void onemindata(string oneminstring)
    {
        string[] vals = oneminstring.Split(',');
        tempname = vals[0];
        temptime = float.Parse(vals[1]);
        if (firstvalueset == false)
        {
            lasttime = temptime;
            firstvalueset = true;
        }
        tempnum = float.Parse(vals[2]);
        if (tempname == requesttype)
        {
            if (temptime == lasttime)
            {
                totalforaverage = totalforaverage + tempnum;
                counter++;
            }
            else
            {
                averagevalue = totalforaverage / counter;
                outputdata.Add(averagevalue);
                lasttime = float.Parse(vals[2]);
            }
        }
    }


    // Update is called once per frame
    void Update()
    {

    }
}
