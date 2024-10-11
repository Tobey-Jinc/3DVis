using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vertex;

public class SkyObject : MonoBehaviour
{
    [SerializeField] private Light lightSource;
    [SerializeField] private Skybox[] skyboxes;

    private ObjectCursor cursor;

    private TransformModeAndControls[] transformModes;

    private int skyboxIndex;

    void Start()
    {
        cursor = ObjectCursor.Instance;

        cursor.OnSelect += Cursor_OnSelect;

        transformModes = new TransformModeAndControls[] {
            new(TransformMode.Rotation, $"{Data.switchControl}Set Text <sprite=3>    Move <sprite=6>    Up / Down <sprite=9>    Colour <sprite=5>"),
            new(TransformMode.Rotation, $"{Data.switchControl}Set Text <sprite=3>    Rotate <sprite=6>    Reset <sprite=5>"),
            new(TransformMode.TextSize, $"{Data.switchControl}Set Text <sprite=3>    Font Size <sprite=8>    Width <sprite=9>    Alignment <sprite=5>")
        };
    }

    // Update is called once per frame
    void Update()
    {
        
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

    }
}
