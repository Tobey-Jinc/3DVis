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

    private TransformMode[] transformModes;

    private int colorIndex;

    public Light LightSource { get => lightSource; }
    public int ColorIndex { get => colorIndex; }

    void Start()
    {
        cursor = ObjectCursor.Instance;
        keyboardInput = KeyboardInput.Instance;

        cursor.OnSelect += Cursor_OnSelect;

        transformModes = new[] { TransformMode.Position, TransformMode.Scale, TransformMode.Brightness };
    }

    private void Cursor_OnSelect(Transform selection, Vector3 selectionPoint)
    {
        cursor.SelectObject(transform, transformModes, transform);
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

                    lightSource.range += scaleInput * 6 * getReal3D.Cluster.deltaTime;

                    rangeVisualizer.localScale = Vector3.one * lightSource.range * 2;

                    break;

                case TransformMode.Brightness:
                    float brightnessInput = getReal3D.Input.GetAxis(Inputs.leftStickY);

                    lightSource.intensity += brightnessInput * 6 * getReal3D.Cluster.deltaTime;

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
}
