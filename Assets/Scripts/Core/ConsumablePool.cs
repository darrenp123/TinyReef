using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
