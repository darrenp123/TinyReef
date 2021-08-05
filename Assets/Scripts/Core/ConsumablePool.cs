// the main purpose of this script is to reuse special effects: when a special effect is instantiated it is
// saved here after use, so the next time there is a need for the same effect we can just used the ones that are
// saved here instead, this helps with performance  
// we never need to create any effect the pooling system creates effects if there none available 

using System.Collections.Generic;
using UnityEngine;

// every type of special effect needs a specific tag, so we now what effect to have
public enum ItemPool
{
    BUBBLES_BURST,
    FOAM_BURST,
}

public class ConsumablePool : MonoBehaviour
{
    private Dictionary<ItemPool, Queue<GameObject>> comsumablePool = new Dictionary<ItemPool, Queue<GameObject>>();

    public GameObject GetItemFromPool(ItemPool ItemClass, object objClass)
    {
        if (!comsumablePool.ContainsKey(ItemClass))
        {
            comsumablePool[ItemClass] = new Queue<GameObject>();
        }

        if (comsumablePool[ItemClass].Count == 0)
        {
            CreateItem(ItemClass, objClass);
        }

        return comsumablePool[ItemClass].Dequeue();
    }

    public void ReturnToPool(ItemPool ItemClass, GameObject objToreturn)
    {
        objToreturn.transform.parent = null;
        AddToQueue(ItemClass, objToreturn);
    }

    private void CreateItem(ItemPool ItemClass, object objClass)
    {
        GameObject newItem = Instantiate(objClass as Component).gameObject;
        AddToQueue(ItemClass, newItem);
    }

    private void AddToQueue(ItemPool ItemClass, GameObject newItem)
    {
        newItem.SetActive(false);
        comsumablePool[ItemClass].Enqueue(newItem);
    }
}
