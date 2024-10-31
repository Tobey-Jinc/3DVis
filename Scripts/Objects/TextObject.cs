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
    [SerializeField] private Color[] colors;

    private ObjectCursor cursor;
    private KeyboardInput keyboardInput;

    private TransformModeAndControls[] transformModes;

    [HideInInspector] [SerializeField] private int colorIndex;

    public TMP_Text Text { get => t_Text; }
    public RectTransform RectTransform { get => rectTransform; }
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
            new(TransformMode.Position, $"{Data.switchControl}Set Text <sprite=3>    Move <sprite=6>    Up / Down <sprite=9>    Colour <sprite=5>"),
            new(TransformMode.Rotation, $"{Data.switchControl}Set Text <sprite=3>    Rotate <sprite=6>    Reset <sprite=5>"),
            new(TransformMode.TextSize, $"{Data.switchControl}Set Text <sprite=3>    Font Size <sprite=8>    Width <sprite=9>    Alignment <sprite=5>")
        };

        t_Text.alignment = TextAlignmentOptions.Left;
    }

    /// <summary>
    /// Setup with the given Scene Description Text
    /// </summary>
    /// <param name="sdText">The text data to set up from</param>
    public void Setup(SDText sdText)
    {
        t_Text.SetText(sdText.text);

        transform.position = sdText.position;
        transform.rotation = sdText.rotation;

        SetColor(sdText.colorIndex);
        colorIndex = sdText.colorIndex;

        t_Text.fontSize = sdText.fontSize;

        rectTransform.sizeDelta = new Vector2(sdText.width, rectTransform.sizeDelta.y);
    }

    /// <summary>
    /// Executed when this object is selected. Selects the object
    /// </summary>
    private void Cursor_OnSelect(Transform selection, Vector3 selectionPoint)
    {
        if (selection == boxCollider.transform)
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
        if (cursor.SelectedObject == transform && !keyboardInput.InMenu)
        {
            // Handle transform modes
            switch (cursor.CursorTransformMode)
            {
                case TransformMode.Position:
                    cursor.Position(transform);

                    // Change colour
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

                case TransformMode.Rotation:
                    cursor.Rotate(transform, Quaternion.identity);

                    break;

                case TransformMode.TextSize:
                    // Change font size
                    float fontSizeInput = getReal3D.Input.GetAxis(Inputs.leftStickY);
                    
                    float fontSizeSpeed = 12 * CurrentOptions.options.scaleSpeed;

                    t_Text.fontSize += fontSizeSpeed * fontSizeInput * getReal3D.Cluster.deltaTime;
                    t_Text.fontSize = Mathf.Max(t_Text.fontSize, 0.1f);

                    // Change container width
                    float widthSpeed = 12 * CurrentOptions.options.scaleSpeed;

                    float widthInput = widthSpeed * getReal3D.Input.GetAxis(Inputs.rightStickY) * getReal3D.Cluster.deltaTime;
                    float width = Mathf.Max(rectTransform.sizeDelta.x + widthInput, 0.1f);
                    rectTransform.sizeDelta = new Vector2(width, rectTransform.sizeDelta.y);

                    // Change text alignment
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

            // Set text
            if (getReal3D.Input.GetButtonDown(Inputs.y))
            {
                keyboardInput.Open("Set Text", (string text) => { t_Text.SetText(text); }, startText: t_Text.text);
            }
        }

        UpdateCollider();
    }

    /// <summary>
    /// Make the collider the size of the text
    /// </summary>
    private void UpdateCollider()
    {
        boxCollider.size = t_Text.bounds.size;
        boxCollider.center = t_Text.bounds.center;
    }

    /// <summary>
    /// Sets the text colour
    /// </summary>
    /// <param name="index">Colour index</param>
    private void SetColor(int index)
    {
        Color color = colors[index];

        t_Text.color = colors[index];   
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        cursor.OnSelect -= Cursor_OnSelect;
        cursor.OnCopy -= Cursor_OnCopy;
    }
}
