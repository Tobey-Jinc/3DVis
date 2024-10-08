using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vertex;
using TMPro;

public class TextObject : MonoBehaviour
{
    [SerializeField] private TMP_Text t_Text;
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private BoxCollider boxCollider;
    [SerializeField] private Vector2 padding;

    private ObjectCursor cursor;
    private KeyboardInput keyboardInput;

    private TransformMode[] transformModes;

    public TMP_Text Text { get => t_Text; }
    public RectTransform RectTransform { get => rectTransform; }

    void Start()
    {
        cursor = ObjectCursor.Instance;
        keyboardInput = KeyboardInput.Instance;

        cursor.OnSelect += Cursor_OnSelect;

        transformModes = new[] { TransformMode.Position, TransformMode.Rotation, TransformMode.TextSize };

        t_Text.alignment = TextAlignmentOptions.Left;
    }

    private void Cursor_OnSelect(Transform selection, Vector3 selectionPoint)
    {
        cursor.SelectObject(transform, transformModes, transform);
    }

    void Update()
    {
        if (cursor.SelectedObject == transform && !keyboardInput.InMenu)
        {
            switch (cursor.CursorTransformMode)
            {
                case TransformMode.Position:
                    cursor.Position(transform);

                    break;

                case TransformMode.Rotation:
                    cursor.Rotate(transform);

                    break;

                case TransformMode.TextSize:
                    float fontSizeInput = getReal3D.Input.GetAxis(Inputs.leftStickY);
                    
                    t_Text.fontSize += 6 * fontSizeInput * getReal3D.Cluster.deltaTime;
                    t_Text.fontSize = Mathf.Max(t_Text.fontSize, 0.1f);

                    float widthInput = 6 * getReal3D.Input.GetAxis(Inputs.rightStickX) * getReal3D.Cluster.deltaTime;
                    float width = Mathf.Max(rectTransform.sizeDelta.x + widthInput, 0.1f);
                    rectTransform.sizeDelta = new Vector2(width, rectTransform.sizeDelta.y);

                    if (getReal3D.Input.GetButtonDown(Inputs.rightShoulder))
                    {
                        if (t_Text.alignment == TextAlignmentOptions.Left)
                        {
                            t_Text.alignment = TextAlignmentOptions.Center;
                        }
                        else if (t_Text.alignment == TextAlignmentOptions.Center)
                        {
                            t_Text.alignment = TextAlignmentOptions.Right;
                        }
                        else
                        {
                            t_Text.alignment = TextAlignmentOptions.Left;
                        }
                    }

                    break;
            }

            if (getReal3D.Input.GetButtonDown(Inputs.y))
            {
                keyboardInput.Open("Set Text", (string text) => { t_Text.SetText(text); }, startText: t_Text.text);
            }
        }

        UpdateCollider();
    }

    private void UpdateCollider()
    {
        boxCollider.size = t_Text.bounds.size;
        boxCollider.center = t_Text.bounds.center;
    }
}
