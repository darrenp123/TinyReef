/*  
 *  AUTHOR: Jon Munro 
 *  CREATED: 02/03/2021 
 */

using UnityEngine;

public class DontDestroy : MonoBehaviour
{
    void Awake()
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag(gameObject.tag);
        if (objs.Length > 1)
        {
            Destroy(gameObject);
        }
        
        DontDestroyOnLoad(gameObject);
    }
}
