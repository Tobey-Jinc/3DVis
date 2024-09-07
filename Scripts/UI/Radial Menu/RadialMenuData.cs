using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadialMenuData
{
    public string title;
    public RadialQuadrantData[] quadrants;

    public RadialMenuData(string title, RadialQuadrantData[] quadrants)
    {
        this.title = title;
        this.quadrants = quadrants;
    }
}
