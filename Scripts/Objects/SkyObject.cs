using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Vertex;

public class SkyObject : MonoBehaviour
{
    [SerializeField] private Light sun;
    [SerializeField] private Material[] skyboxes;
    [SerializeField] private Color[] colors;
    [SerializeField] private Quaternion resetRotation;
    [SerializeField] private MeshRenderer editorRenderer;

    private ObjectCursor cursor;

    private TransformModeAndControls[] transformModes;

    private int skyboxIndex;
    private int colorIndex;

    public Light Sun { get => sun; }
    public int SkyboxIndex { get => skyboxIndex; }
    public int ColorIndex { get => colorIndex; }

    void Start()
    {
        cursor = ObjectCursor.Instance;

        cursor.OnSelect += Cursor_OnSelect;

        transformModes = new TransformModeAndControls[] {
            new(TransformMode.Rotation, $"{Data.switchControlNoDelete}Swap Sky <sprite=3>    Rotate <sprite=6>    Reset <sprite=5>"),
            new(TransformMode.Brightness, $"{Data.switchControlNoDelete}Swap Sky <sprite=3>    Brightness <sprite=8>    Shadows <sprite=5>"),
            new(TransformMode.Position, $"{Data.switchControlNoDelete}Swap Sky <sprite=3>    Move <sprite=6>    Up / Down <sprite=9>    Colour <sprite=5>"),
        };
    }

    void Update()
    {
        editorRenderer.enabled = cursor.EditMode;

        if (cursor.SelectedObject == transform)
        {
            switch (cursor.CursorTransformMode)
            {
                case TransformMode.Rotation:
                    cursor.Rotate(sun.transform, resetRotation, true);

                    break;

                case TransformMode.Brightness:
                    float brightnessInput = getReal3D.Input.GetAxis(Inputs.leftStickY);

                    float brightnessSpeed = 12 * CurrentOptions.options.scaleSpeed;

                    sun.intensity += brightnessInput * brightnessSpeed * getReal3D.Cluster.deltaTime;

                    if (getReal3D.Input.GetButtonDown(Inputs.rightShoulder))
                    {
                        if (sun.shadows == LightShadows.None)
                        {
                            sun.shadows = LightShadows.Hard;
                        }
                        else
                        {
                            sun.shadows = LightShadows.None;
                        }
                    }

                    break;

                case TransformMode.Position:
                    cursor.Position(transform);

                    if (getReal3D.Input.GetButtonDown(Inputs.rightShoulder))
                    {
                        colorIndex++;
                        if (colorIndex >= colors.Length)
                        {
                            colorIndex = 0;
                        }

                        SetColor(colorIndex);
                    }

                    break;
            }

            if (getReal3D.Input.GetButtonDown(Inputs.y))
            {
                skyboxIndex++;
                if (skyboxIndex >= skyboxes.Length)
                {
                    skyboxIndex = 0;
                }

                SetSkybox(skyboxIndex);
            }
        }
    }

    private void Cursor_OnSelect(Transform selection, Vector3 selectionPoint)
    {
        if (selection == transform)
        {
            cursor.SelectObject(transform, transformModes, transform);
        }
    }

    private void SetSkybox(int index)
    {
        RenderSettings.skybox = skyboxes[index];
        DynamicGI.UpdateEnvironment();
    }

    private void SetColor(int index)
    {
        Color color = colors[index];

        sun.color = color;
    }

    public void Setup(SDSky sky)
    {
        transform.position = sky.position;
        transform.rotation = sky.rotation;

        skyboxIndex = sky.skyboxIndex;
        SetSkybox(skyboxIndex);

        colorIndex = sky.colorIndex;
        SetColor(colorIndex);

        sun.intensity = sky.intensity;
        sun.shadows = sky.shadows;
    }
}
