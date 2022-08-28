using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class recorder2
{

    private static List<string> tempdata = new List<string>();
    public static List<string> masterlist = new List<string>();
    public static List<string> namelist = new List<string>();

    public static Dictionary<string, List<string>> alldata = new Dictionary<string, List<string>>();

    public static int counter = 0;

    private static string listname = "";

    public static bool record(GameObject[] allObjects)
    {
        bool result = false;

        foreach (GameObject go in allObjects)
            if (go.activeInHierarchy)
            {
                //get name of objects
                string name = go.name;
                //if name is not already on list of names then add
                if (!namelist.Contains(name))
                {
                    namelist.Add(name);
                }
                int fishsize = go.GetComponent<SFlockUnit>().size;
               
                //generate list key
                listname = counter + "mins";
                //if list key does not exist on list of times then add
                if (!masterlist.Contains(listname))
                {
                    masterlist.Add(listname);
                }
                //add single fish name and data to temp list
                string tempsize = fishsize.ToString();
                string paddedsize = tempsize.PadLeft(4,'0');
                tempdata.Add(name + paddedsize);
            }

        counter++;
       
        //add all fish data (parsed name, size) to dictionary via key
        alldata.Add(listname, tempdata);


        //foreach (var pair in alldata) 
        //{
        //    Debug.Log(pair.Key + string.Join(", ", pair.Value));
        //}



        tempdata = new List<string>();
        result = true;
        Debug.Log("ping");
        return result;
    }


}
