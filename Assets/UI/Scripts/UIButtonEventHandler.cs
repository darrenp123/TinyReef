/*  
 *  AUTHOR: Jon Munro 
 *  CREATED: 02/03/2021 
 */

using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonEventHandler : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler
{
    UISoundManager UISoundManager;

    private void Awake()
    {
        UISoundManager = GameObject.FindGameObjectWithTag("UISoundManager").GetComponent<UISoundManager>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        UISoundManager.PlaySound("ButtonClicked");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        UISoundManager.PlaySound("ButtonHover");
    }
}
