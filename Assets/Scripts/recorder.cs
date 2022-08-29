using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Analytics;

public static class recorder
{

    public static string m_path = "Assets/Stats/";

    private static List<string> tempdata = new List<string>();

    public static bool record(GameObject[] allObjects)
    {
        //string filePath = m_path + SceneManager.GetActiveScene().name + "gameobjectname.txt";
        bool result = false;

        foreach (GameObject go in allObjects)
            if (go.activeInHierarchy)
            {
                string name = go.name;
                int fishsize = go.GetComponent<SFlockUnit>().size;
                string lineToAdd = name + " , " + Time.time + " , " + fishsize;
                using (StreamWriter sw = File.AppendText(m_path + "gameobjectname.txt")) //This line will try to open the file and if it doesn't exist, if will make it!
                {
                    sw.WriteLine(lineToAdd);
                    sw.Close();
                }
            }
        result = true;
        return result;
    }

    public static List<string> averagingfunction(){

        var returnlist = new List<string>();


        return returnlist;
    }
}
