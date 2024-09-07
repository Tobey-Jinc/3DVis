using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class RadialQuadrantData
{
    public string label;
    public Sprite icon;
    public UnityAction action;

    public RadialQuadrantData(string label, Sprite icon, UnityAction action)
    {
        this.label = label;
        this.icon = icon;
        this.action = action;
    }
}
