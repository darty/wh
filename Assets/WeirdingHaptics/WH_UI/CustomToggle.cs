using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomToggle : MonoBehaviour
{
    private void HandleValueChanged()
    {
        Toggle toggle = GetComponent<Toggle>();
        Image outline = transform.GetComponentInChildren<Image>();
        if (toggle.isOn)
        {
            outline.enabled = true;
        }
        else
        {
            outline.enabled = false;
        }
    }

    public void UpdateValue()
    {
        HandleValueChanged();
    }
}
