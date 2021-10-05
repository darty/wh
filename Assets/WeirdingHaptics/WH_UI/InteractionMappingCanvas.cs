using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractionMappingCanvas : MonoBehaviour
{
    public enum InteractionType
    {
        Grab,
        Touch
    }

    public Image arrow;

    public Image icon;
    public Sprite grab;
    public Sprite touch;

    public void Initialize(InteractionType type)
    {
        if (type == InteractionType.Grab)
        {
            icon.sprite = grab;
        }
        else
        {
            icon.sprite = touch;
            arrow.rectTransform.Rotate(180f, 0f, 0f);
        }   
    }

    public void RotateArrow(float angle)
    {
        arrow.rectTransform.Rotate(0f, 0f, angle);
    }
}
