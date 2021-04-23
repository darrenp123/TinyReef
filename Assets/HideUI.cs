using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HideUI : MonoBehaviour
{
    [SerializeField]
    bool isHidden = false;
    [SerializeField]
    TMP_Text buttonText;

    [SerializeField]
    GameObject[] UIElements;

    public void HideElement(GameObject elementToHide)
    {
        UIElement elementScript = elementToHide.GetComponent<UIElement>();
        for (int i = 0; i < UIElements.Length; i++)
        {
            if(elementScript.ElementID == i)
            {
                if(elementScript.isHidden == true)
                {
                    elementScript.isHidden = false;
                    Animator animator = UIElements[i].GetComponent<Animator>();
                    animator.enabled = true;
                    animator.ResetTrigger("HideUI");
                    animator.SetTrigger("ShowUI");
                    elementScript.buttonText.text = "Hide";
                }
                else if (elementScript.isHidden == false)
                {
                    elementScript.isHidden = true;
                    Animator animator = UIElements[i].GetComponent<Animator>();
                    animator.enabled = true;
                    animator.ResetTrigger("ShowUI");
                    animator.SetTrigger("HideUI");
                    elementScript.buttonText.text = "Show";
                }
            }
        }
    }

    public void HideAll()
    {
        if(isHidden)
        {
            isHidden = false;
            for (int i = 0; i < UIElements.Length; i++)
            {
                Animator animator = UIElements[i].GetComponent<Animator>();
                animator.enabled = true;
                animator.ResetTrigger("HideUI");
                animator.SetTrigger("ShowUI");
                UIElement elementScript = UIElements[i].GetComponent<UIElement>();
                if (elementScript.buttonText != null)
                {
                    elementScript.isHidden = false;
                    elementScript.buttonText.text = "Hide";
                }
                buttonText.text = "Hide   UI";
            }
        }
        else if(!isHidden)
        {
            isHidden = true;
            for (int i = 0; i < UIElements.Length; i++)
            {
                Animator animator = UIElements[i].GetComponent<Animator>();
                animator.enabled = true;
                animator.ResetTrigger("ShowUI");
                animator.SetTrigger("HideUI");
                UIElement elementScript = UIElements[i].GetComponent<UIElement>();
                if (elementScript.buttonText != null)
                {
                    elementScript.isHidden = true;
                    elementScript.buttonText.text = "Show";
                }
                buttonText.text = "Show UI";

            }
        }
    }
}
