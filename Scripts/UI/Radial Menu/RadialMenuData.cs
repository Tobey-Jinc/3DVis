using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores all the data for a radial menu
/// </summary>
public class RadialMenuData
{
    public string title;
    public RadialQuadrantData[] quadrants;
    public Menu previousMenu; // The menu to return to

    public RadialMenuData(string title, RadialQuadrantData[] quadrants, Menu previousMenu)
    {
        this.title = title;
        this.quadrants = quadrants;
        this.previousMenu = previousMenu;
    }
}
