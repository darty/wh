using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Polygoat.Haptics;

public class MappingLabel : MonoBehaviour
{
    public Sprite instantaneous;
    public Sprite continuousPassive;
    public Sprite continuousActive;
    public Sprite positional;

    private Image image;


    private void OnEnable()
    {
        image = GetComponent<Image>();
    }

    public void SetType(HapticType type)
    {
        if(type == HapticType.Instantaneous)
        {
            image.sprite = instantaneous;
        }
        else if (type == HapticType.ContinuousPassive)
        {
            image.sprite = continuousPassive;
        }
        else if (type == HapticType.ContinuousActive)
        {
            image.sprite = continuousActive;
        }
        else if (type == HapticType.Positional)
        {
            image.sprite = positional;
        }
    }
}
