using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

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
    public float renderDistance = 5000f;
    public bool invertCameraControls = false;
    public bool hideControls = false;
}

public static class CurrentOptions
{
    public static Options options = new Options();
}

public class OptionsController : MonoBehaviour
{
    [SerializeField] private new Camera camera;

    private void Start()
    {
        LoadOptions();
    }

    private void LoadOptions()
    {
        string path = "\\\\CAVE-HEADNODE\\data\\3dvis\\options.json";

        if (File.Exists(path))
        {
            try
            {
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

    private void ApplyOptions()
    {
        camera.farClipPlane = CurrentOptions.options.renderDistance;
        QualitySettings.SetQualityLevel(CurrentOptions.options.graphicsQuality);
    }
}
