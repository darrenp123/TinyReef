/*  
 *  AUTHOR: Jon Munro 
 *  CREATED: 29/03/2021 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ContentsButton : MonoBehaviour
{
    Animator animator;
    [SerializeField]
    bool areExposed;
    // Start is called before the first frame update
    void Start()
    {
        areExposed = false;
        animator = gameObject.GetComponent<Animator>();
    }

    public void ToggleChildren()
    {
        if (areExposed == true)
        {
            areExposed = false;
            HideChildren();
        }
        else if (areExposed == false)
        {
            areExposed = true;
            ShowChildren();
        }
    }

    void ShowChildren()
    {
        animator.enabled = true;
        animator.Play("Base Layer.ButtonExpandAnim",0,0);
        Invoke("DisableAnimator", 0.2f);
    }

    void HideChildren()
    {
        animator.enabled = true;
        animator.Play("Base Layer.ButtonCollapseAnim", 0,0);
        Invoke("DisableAnimator", 0.2f);
    }

    void DisableAnimator()
    {
        animator.enabled = false;
    }
}
