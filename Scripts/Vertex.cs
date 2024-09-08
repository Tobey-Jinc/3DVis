using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vertex
{
    /// <summary>
    /// Defines the colour palette for the 3DVis app
    /// </summary>
    public class Palette
    {
        public static Color32 White = new Color32(255, 255, 255, 255);
        public static Color32 Black = new Color32(0, 0, 0, 255);
        public static Color32 DarkGrey = new Color32(15, 17, 17, 255);
        public static Color32 Red = new Color32(229, 55, 58, 255);
        public static Color32 Blue = new Color32(55, 88, 164, 255);
    }

    /// <summary>
    /// Cached input strings to minimize garbage collection
    /// </summary>
    public class Inputs
    {
        public static string a = "A";
        public static string b = "B";
        public static string x = "X";
        public static string y = "Y";

        public static string leftShoulder = "LeftShoulder";
        public static string rightShoulder = "RightShoulder";

        public static string leftStickX = "Yaw";
        public static string leftStickY = "Forward";

        public static string rightStickX = "Strafe";
        public static string rightStickY = "Pitch";
    }
}
