using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

/// <summary>
/// Defines all options
/// </summary>
[System.Serializable]
public class Options
{
    public float cameraSensitivity = 0.5f;
    public float movementSpeed = 0.5f;
    public float positionSpeed = 0.5f;
    public float rotationSpeed = 0.5f;
    public float scaleSpeed = 0.5f;
    public float wandSmoothing = 20f;
    public int graphicsQuality = 5;
    public bool invertCameraControls = false;
    public bool hideControls = false;
}

/// <summary>
/// Stores current options to be accessible across the whole app
/// </summary>
public static class CurrentOptions
{
    public static Options options = new Options();
}

public class OptionsController : MonoBehaviour
{
    private void Start()
    {
        LoadOptions();
    }

    /// <summary>
    /// Tries to load options from the network folder.
    /// If no options file exists, options will just use their default values.
    /// No option file will be created here.
    /// </summary>
    public void LoadOptions()
    {
        string path = "\\\\CAVE-HEADNODE\\data\\3dvis\\options.json";

        if (File.Exists(path))
        {
            try
            {
                // Read options
                string json = File.ReadAllText(path);
                CurrentOptions.options = JsonUtility.FromJson<Options>(json);

                Debug.Log("Successfully read options file");
            }
            catch
            {
                Debug.Log("Could not read options file");
            }
        }
        else
        {
            Debug.Log("Options file does not exist");
        }

        ApplyOptions();
    }

    /// <summary>
    /// Applies options that only need to be applied once
    /// </summary>
    private void ApplyOptions()
    {
        // Set the graphics quality
        QualitySettings.SetQualityLevel(CurrentOptions.options.graphicsQuality);
    }
}
