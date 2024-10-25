using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vertex
{
    public class Paths
    {
        public static string GetModelFolder()
        {
            return Application.persistentDataPath + "/models/";
        }

        public static string GetSceneFolder()
        {
            return Application.persistentDataPath + "/scenes/";
        }
    }

    /// <summary>
    /// Defines the colour palette for the 3DVis app
    /// </summary>
    public class Palette
    {
        public static Color32 white = new Color32(255, 255, 255, 255);
        public static Color32 black = new Color32(0, 0, 0, 255);
        public static Color32 darkGrey = new Color32(15, 17, 17, 255);
        public static Color32 red = new Color32(229, 55, 58, 255);
        public static Color32 blue = new Color32(55, 88, 164, 255);
    }

    /// <summary>
    /// Cached layer strings
    /// </summary>
    public class Layer
    {
        public static string model = "Model";
    }

    /// <summary>
    /// Cached input strings to minimize garbage collection
    /// </summary>
    public class Inputs
    {
        public static string a = "B";
        public static string b = "X";
        public static string x = "A";
        public static string y = "Y";

        public static string leftShoulder = "LeftShoulder";
        public static string rightShoulder = "RightShoulder";

        public static string leftTrigger = "LeftTrigger";
        public static string rightTrigger = "RightTrigger";

        public static string dpadUp = "DpadUp";
        public static string dpadDown = "DpadDown";
        public static string dpadLeft = "DpadLeft";
        public static string dpadRight = "DpadRight";

        public static string dpadUpDown = "UpDown";
        public static string dpadLeftRight = "LeftRight";

        public static string leftStickX = "Yaw";
        public static string leftStickY = "Forward";

        public static string rightStickX = "Strafe";
        public static string rightStickY = "Pitch";

        public static int Composite(string positiveInput, string negativeInput, bool hold = true)
        {
            bool positive, negative;

            if (hold)
            {
                positive = getReal3D.Input.GetButton(positiveInput);
                negative = getReal3D.Input.GetButton(negativeInput);
            }
            else
            {
                positive = getReal3D.Input.GetButtonDown(positiveInput);
                negative = getReal3D.Input.GetButtonDown(negativeInput);
            }

            if (positive && !negative)
            {
                return 1;
            }
            else if (negative && !positive)
            {
                return -1;
            }

            return 0;
        }

        public static int AxisToInt(string axis, float deadZone = 0.2f)
        {
            float input = getReal3D.Input.GetAxis(axis);

            if (input > deadZone)
            {
                return 1;
            }
            else if (input < -deadZone)
            {
                return -1;
            }

            return 0;
        }
    }

    public class Data
    {
        public static float menuScaleSpeed = 30f;

        public const string allCategory = "All";

        public const string switchControl = "Switch <sprite=0>    Delete <sprite=4>    ";

        public const string switchControlNoDelete = "Switch <sprite=0>    ";
    }
}
