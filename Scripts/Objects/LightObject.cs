using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using Vertex;

public class LightObject : MonoBehaviour
{
    [SerializeField] private Light lightSource;
    [SerializeField] private Transform rangeVisualizer;
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private Color[] colors;
    [SerializeField] private MeshRenderer editorRenderer;

    private ObjectCursor cursor;
    private KeyboardInput keyboardInput;

    private TransformModeAndControls[] transformModes;

    [HideInInspector][SerializeField] private int colorIndex;

    public Light LightSource { get => lightSource; }
    public int ColorIndex { get => colorIndex; }

    void Start()
    {
        cursor = ObjectCursor.Instance;
        keyboardInput = KeyboardInput.Instance;

        // Subscribe to events
        cursor.OnSelect += Cursor_OnSelect;
        cursor.OnCopy += Cursor_OnCopy;

        // Define transform modes
        transformModes = new TransformModeAndControls[] {
            new(TransformMode.Position, $"{Data.switchControl}Colour <sprite=3>    Move <sprite=6>    Up / Down <sprite=9>"),
            new(TransformMode.Scale, $"{Data.switchControl}Colour <sprite=3>    Range <sprite=8>"),
            new(TransformMode.Brightness, $"{Data.switchControl}Colour <sprite=3>    Brightness <sprite=8>    Shadows <sprite=5>")
        };
    }

    /// <summary>
    /// Set up the light from a Scene Description Light
    /// </summary>
    /// <param name="sdLight">The SDLight to derive from</param>
    public void Setup(SDLight sdLight)
    {
        transform.position = sdLight.position;

        SetColor(sdLight.colorIndex);
        colorIndex = sdLight.colorIndex;

        lightSource.range = sdLight.range;
        lightSource.intensity = sdLight.intensity;
        lightSource.shadows = sdLight.shadows;
    }

    /// <summary>
    /// Executed when this object is selected. Selects the object
    /// </summary>
    private void Cursor_OnSelect(Transform selection, Vector3 selectionPoint)
    {
        if (selection == transform)
        {
            cursor.SelectObject(transform, transformModes, transform);
        }
    }

    /// <summary>
    /// Copies this object
    /// </summary>
    private void Cursor_OnCopy(Transform selection)
    {
        if (selection == transform)
        {
            ModelCache.Instance.Copy(gameObject);
        }
    }

    void Update()
    {
        // Hide if not in edit mode
        editorRenderer.enabled = cursor.EditMode;

        if (cursor.SelectedObject == transform && !keyboardInput.InMenu)
        {
            // Show range visualizer
            rangeVisualizer.gameObject.SetActive(cursor.CursorTransformMode == TransformMode.Scale);

            // Handle transform modes
            switch (cursor.CursorTransformMode)
            {
                case TransformMode.Position:
                    cursor.Position(transform);

                    break;

                case TransformMode.Scale:
                    // Change light range
                    float scaleInput = getReal3D.Input.GetAxis(Inputs.leftStickY);

                    float scaleSpeed = 12 * CurrentOptions.options.scaleSpeed;

                    lightSource.range += scaleInput * scaleSpeed * getReal3D.Cluster.deltaTime;

                    // Visualize light reach
                    rangeVisualizer.localScale = Vector3.one * lightSource.range * 2;

                    break;

                case TransformMode.Brightness:
                    // Change brightness
                    float brightnessInput = getReal3D.Input.GetAxis(Inputs.leftStickY);

                    float brightnessSpeed = 12 * CurrentOptions.options.scaleSpeed;

                    lightSource.intensity += brightnessInput * brightnessSpeed * getReal3D.Cluster.deltaTime;
                    
                    // Toggle shadows
                    if (getReal3D.Input.GetButtonDown(Inputs.rightShoulder))
                    {
                        if (lightSource.shadows == LightShadows.None)
                        {
                            lightSource.shadows = LightShadows.Hard;
                        }
                        else
                        {
                            lightSource.shadows = LightShadows.None;
                        }
                    }

                    break;
            }

            // Change light colour
            if (getReal3D.Input.GetButtonDown(Inputs.y))
            {
                colorIndex++;
                if (colorIndex >= colors.Length)
                {
                    colorIndex = 0;
                }

                SetColor(colorIndex);
            }
        }
        else
        {
            rangeVisualizer.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Set the colour to the given index
    /// </summary>
    /// <param name="index">The colours index</param>
    private void SetColor(int index)
    {
        Color color = colors[index];

        Material material = meshRenderer.material;
        material.color = color;
        meshRenderer.material = material;

        lightSource.color = color;
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        cursor.OnSelect -= Cursor_OnSelect;
        cursor.OnCopy -= Cursor_OnCopy;
    }
}
