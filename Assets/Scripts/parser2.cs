using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class parser2 
{

    public static List<string> tempname = new List<string>();
    public static List<string> temptime = new List<string>();

    public static Dictionary<string, List<string>> tempdir= new Dictionary<string, List<string>>();
    public static Dictionary<string, List<string>> dataperfish = new Dictionary<string, List<string>>();

    public static Dictionary<string, List<int>> fishavgsize = new Dictionary<string, List<int>>();
    public static List<int> fishavglist = new List<int>();
    public static Dictionary<string, List<int>> fishpop = new Dictionary<string, List<int>>();
    public static List<int> fishpoplist = new List<int>();

    public static int nooftimecaps = 0;
    public static int currenttimecap = 0;
    public static int stepcounter =0;

    public static List<string> calledtime = new List<string>();

    public static string searchforthisfish;

    public static int currenttimeloop = 0;

    public static int totalfishsize;
    public static int numberofsamefish;
    public static int avgfishsize;

    public static bool getnames()
    {
        bool result1 = false;

        tempname = recorder2.namelist;

        result1 = true;
        return result1;
    }

    public static bool gettimes()
    {
        bool result2 = false;

        temptime = recorder2.masterlist;

        result2 = true;
        return result2;
    }

    public static bool getalldata()
    {
        bool result2 = false;

        tempdir = recorder2.alldata;

        result2 = true;
        return result2;
    }

    public static bool gettimestep()
    {
        bool result3 = false;

        stepcounter = recorder2.counter;

        result3 = true;
        return result3;
    }

    public static bool setlistoftimes()
    {
        bool result4 = false;

        nooftimecaps = temptime.Count;
        nooftimecaps = nooftimecaps - 1;

        result4 = true;
        return result4;
    }

    public static bool setfishnametimes()
    {
        bool result5 = false;

        // set the name of the fish we're looking for
        foreach (var name in tempname)
        {
            //grab the number of possible time lists
            foreach (var time in temptime)
            {
                //call the timelist
                if (currenttimecap < stepcounter)
                {
                    calledtime = tempdir[currenttimecap + "mins"];

                    //split the time list 
                    foreach (var entry in calledtime)
                    {
                        string[] vals = entry.Split(',');

                        //grab each value in time list
                        foreach (var value in vals)
                        {
                            // store string length
                            int len = value.Length;
                            len = len - 4;
                            //remove size from entry
                            string actualname = value.Substring(0, value.Length - 4);
                            // if name matches then store
                            if (actualname == name)
                            {
                                int actualsize = int.Parse(value.Substring(value.Length - 4));
                                totalfishsize = totalfishsize + actualsize;
                                numberofsamefish++;
                            }
                        }
                        //after going through all values in entry then work out average and contract name. Store. 

                    }
                    avgfishsize = totalfishsize / numberofsamefish;
                    string storedname = name.Substring(0, name.Length - 7);
                    Debug.Log(currenttimecap + "mins");
                    Debug.Log(storedname + " name");
                    Debug.Log(avgfishsize + " avg");
                    Debug.Log(numberofsamefish + "pop");
                    storedname = "";
                    totalfishsize = 0;
                    avgfishsize = 0;
                    numberofsamefish = 0;
                }
                currenttimecap++;
            }
            //increase round number
            
            //
        }


        //calledtime = tempdir[currenttimecap + "mins"];
        //foreach (var x in calledtime)
        //{
        //    Debug.Log(x.ToString());
        //}
        currenttimecap = 0;
        result5 = true;
        return result5;
    }

    public static bool sorting()
    {
        bool result4 = false;

        if(currenttimecap < stepcounter)
        {
            foreach (var name in tempname)
            {
                calledtime = tempdir[currenttimecap + "mins"];
                
                foreach (var entry in calledtime)
                {
                    string actualname = entry.Substring(0, entry.Length - 4);
                    if (name == actualname)
                    {
                        int actualsize = int.Parse(entry.Substring(entry.Length - 4));
                        totalfishsize = totalfishsize + actualsize;
                        numberofsamefish++;
                    }
                }
                avgfishsize = totalfishsize / numberofsamefish;
                string storedname = name.Substring(0, name.Length - 7);
                Debug.Log(currenttimecap + "mins");
                Debug.Log(storedname + " name");
                Debug.Log(avgfishsize + " avg");
                Debug.Log(numberofsamefish + "pop");
                storedname = "";
                avgfishsize = 0;
                totalfishsize = 0;
                numberofsamefish = 0;
            }
            currenttimecap++;
        }

        result4 = true;
        return result4;
    }
}  

