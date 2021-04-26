/*  
 *  AUTHOR: Jon Munro 
 *  CREATED: 02/03/2021 
 */

using UnityEngine;

public class DontDestroy : MonoBehaviour
{
    void Awake()
    {
        DontDestroy[] objs = FindObjectsOfType<DontDestroy>();
        if (objs.Length > 2)
        {
            Destroy(gameObject);
        }
        
        DontDestroyOnLoad(gameObject);
    }
}
