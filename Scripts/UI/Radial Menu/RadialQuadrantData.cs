using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Define a quadrant in the radial menu
/// </summary>
public class RadialQuadrantData
{
    public string label;
    public Sprite icon;
    public UnityAction action; // The action to run when the quadrant is selected and pressed

    public RadialQuadrantData(string label, Sprite icon, UnityAction action)
    {
        this.label = label;
        this.icon = icon;
        this.action = action;
    }
}
