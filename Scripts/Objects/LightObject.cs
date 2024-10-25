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

    private ObjectCursor cursor;
    private KeyboardInput keyboardInput;

    private TransformModeAndControls[] transformModes;

    private int colorIndex;

    public Light LightSource { get => lightSource; }
    public int ColorIndex { get => colorIndex; }

    void Start()
    {
        cursor = ObjectCursor.Instance;
        keyboardInput = KeyboardInput.Instance;

        cursor.OnSelect += Cursor_OnSelect;
        cursor.OnCopy += Cursor_OnCopy;

        transformModes = new TransformModeAndControls[] {
            new(TransformMode.Position, $"{Data.switchControl}Colour <sprite=3>    Move <sprite=6>    Up / Down <sprite=9>"),
            new(TransformMode.Scale, $"{Data.switchControl}Colour <sprite=3>    Range <sprite=8>"),
            new(TransformMode.Brightness, $"{Data.switchControl}Colour <sprite=3>    Brightness <sprite=8>    Shadows <sprite=5>")
        };
    }

    public void Setup(SDLight sdLight)
    {
        transform.position = sdLight.position;

        SetColor(sdLight.colorIndex);
        colorIndex = sdLight.colorIndex;

        lightSource.range = sdLight.range;
        lightSource.intensity = sdLight.intensity;
        lightSource.shadows = sdLight.shadows;
    }

    private void Cursor_OnSelect(Transform selection, Vector3 selectionPoint)
    {
        if (selection == transform)
        {
            cursor.SelectObject(transform, transformModes, transform);
        }
    }

    private void Cursor_OnCopy(Transform selection)
    {
        if (selection == transform)
        {
            ModelCache.Instance.Copy(transform);
        }
    }

    void Update()
    {
        if (cursor.SelectedObject == transform && !keyboardInput.InMenu)
        {
            rangeVisualizer.gameObject.SetActive(cursor.CursorTransformMode == TransformMode.Scale);

            switch (cursor.CursorTransformMode)
            {
                case TransformMode.Position:
                    cursor.Position(transform);

                    break;

                case TransformMode.Scale:
                    float scaleInput = getReal3D.Input.GetAxis(Inputs.leftStickY);

                    float scaleSpeed = 12 * CurrentOptions.options.scaleSpeed;

                    lightSource.range += scaleInput * scaleSpeed * getReal3D.Cluster.deltaTime;

                    rangeVisualizer.localScale = Vector3.one * lightSource.range * 2;

                    break;

                case TransformMode.Brightness:
                    float brightnessInput = getReal3D.Input.GetAxis(Inputs.leftStickY);

                    float brightnessSpeed = 12 * CurrentOptions.options.scaleSpeed;

                    lightSource.intensity += brightnessInput * brightnessSpeed * getReal3D.Cluster.deltaTime;

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
        cursor.OnSelect -= Cursor_OnSelect;
        cursor.OnCopy -= Cursor_OnCopy;
    }
}
