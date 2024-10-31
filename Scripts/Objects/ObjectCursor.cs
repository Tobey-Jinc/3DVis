using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;
using Vertex;
using TMPro;

public enum TransformMode
{
    None,
    Position,
    Rotation,
    Scale,
    TextSize,
    Volume,
    Brightness
}

/// <summary>
/// Stores a transform mode and its controls
/// </summary>
public struct TransformModeAndControls
{
    public TransformMode transformMode;
    public string controls;

    public TransformModeAndControls(TransformMode transformMode, string controls)
    {
        this.transformMode = transformMode;
        this.controls = controls;
    }
}

public class ObjectCursor : MonoBehaviour
{
    public static ObjectCursor Instance;

    [SerializeField] private ModelCache modelCache;
    [SerializeField] private RadialMenu radialMenu;
    [SerializeField] private KeyboardInput keyboardInput;
    [SerializeField] private Transform wand;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private Transform cursor;
    [SerializeField] private SpriteRenderer cursorRenderer;
    [SerializeField] private Material foundModelMaterial;
    [SerializeField] private Material didNotFindModelMaterial;
    [SerializeField] private Material quickPlaceMaterial;
    [SerializeField] private RectTransform controls;
    [SerializeField] private TMP_Text t_Controls;

    [Header("Selection")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Transform transformIcon;
    [SerializeField] private float transformIconSizeDampening;
    [SerializeField] private SpriteRenderer transformIconSprite;
    [SerializeField] private Sprite positionIcon;
    [SerializeField] private Color positionColor;
    [SerializeField] private Sprite rotationIcon;
    [SerializeField] private Color rotationColor;
    [SerializeField] private Sprite scaleIcon;
    [SerializeField] private Color scaleColor;
    private Transform selectionAnchor;

    private TransformModeAndControls[] transformModes;
    private int transformModeIndex = 0;

    private string paintControls = "Create <sprite=0>    Copy <sprite=3>    Paste <sprite=2>";
    private string selectionControls = "Select <sprite=0>";

    private bool editMode = true;

    public TransformMode CursorTransformMode { get; private set; } = TransformMode.Position;
    public Transform SelectedObject { get; private set; } = null;
    public bool Active { get; private set; } = false;
    public bool EditMode { get => editMode; }

    public delegate void SelectEvent(Transform selection, Vector3 selectionPoint);
    public event SelectEvent OnSelect;

    public delegate void CopyEvent(Transform selection);
    public event CopyEvent OnCopy;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (editMode)
        {
            if (!radialMenu.InMenu && SelectedObject != null)
            {
                // Make tranform icon face the camera
                transformIcon.LookAt(cameraTransform);
                transformIcon.position = selectionAnchor.position;
                transformIcon.gameObject.SetActive(true);

                // Scale the icon
                float distanceFromCamera = Vector3.Distance(transformIcon.position, cameraTransform.position);
                float transformIconScale = distanceFromCamera / transformIconSizeDampening;
                transformIconScale = Mathf.Clamp(transformIconScale, 0, 0.5f);
                transformIcon.localScale = Vector3.one * transformIconScale;

                // Set the icons colour and sprite
                switch (CursorTransformMode)
                {
                    case TransformMode.None:
                        transformIconSprite.color = Color.black;
                        break;

                    case TransformMode.Position:
                        transformIconSprite.sprite = positionIcon;
                        transformIconSprite.color = positionColor;
                        break;

                    case TransformMode.Rotation:
                        transformIconSprite.sprite = rotationIcon;
                        transformIconSprite.color = rotationColor;
                        break;

                    case TransformMode.Scale:
                        transformIconSprite.sprite = scaleIcon;
                        transformIconSprite.color = scaleColor;
                        break;

                    case TransformMode.TextSize:
                        transformIconSprite.sprite = scaleIcon;
                        transformIconSprite.color = scaleColor;
                        break;

                    case TransformMode.Volume:
                        transformIconSprite.sprite = null;
                        break;

                    case TransformMode.Brightness:
                        transformIconSprite.sprite = scaleIcon;
                        transformIconSprite.color = scaleColor;
                        break;
                }

                // Show the controls for the current transform mode
                if (!CurrentOptions.options.hideControls)
                {
                    t_Controls.SetText(transformModes[transformModeIndex].controls);
                    controls.localScale = Vector3.one;
                }
                else
                {
                    controls.localScale = Vector3.zero;
                }
            }
            else if (!radialMenu.InMenu && Active && !CurrentOptions.options.hideControls)
            {
                // Show the controls for paint (quick place) mode
                if (getReal3D.Input.GetButton(Inputs.leftShoulder))
                {
                    t_Controls.SetText(paintControls);
                }
                else
                {
                    t_Controls.SetText(selectionControls);
                }

                controls.localScale = Vector3.one;
            }
            else
            {
                transformIcon.gameObject.SetActive(false);

                controls.localScale = Vector3.zero;
            }
        }

        // Toggle edit mode
        // If edit mode is off, you can't select objects, use quick place, and certain models will be hidden like audio speakers
        if (SelectedObject == null && getReal3D.Input.GetButtonDown(Inputs.y))
        {
            bool leftShoulder = getReal3D.Input.GetButton(Inputs.leftShoulder);
            bool rightShoulder = getReal3D.Input.GetButton(Inputs.rightShoulder);

            if (!editMode)
            {
                ToggleEditMode();
            }
            else if (!leftShoulder && !rightShoulder)
            {
                ToggleEditMode();
            }
        }
    }

    void LateUpdate()
    {
        if (editMode)
        {
            bool leftShoulder = getReal3D.Input.GetButton(Inputs.leftShoulder);
            bool rightShoulder = getReal3D.Input.GetButton(Inputs.rightShoulder);

            if (!radialMenu.InMenu && !keyboardInput.InMenu && SelectedObject == null && (leftShoulder || rightShoulder))
            {
                Active = true;

                lineRenderer.enabled = true;
                lineRenderer.SetPosition(0, wand.position);

                if (leftShoulder) // Quick place mode
                {
                    lineRenderer.material = quickPlaceMaterial;
                    cursorRenderer.material = quickPlaceMaterial;

                    // Make cursor conform to geometry
                    if (Physics.Raycast(wand.position, wand.forward, out RaycastHit hit, 15))
                    {
                        ShowCursorAtHit(hit);

                        // Copy hovered object
                        if (getReal3D.Input.GetButtonDown(Inputs.y))
                        {
                            OnCopy?.Invoke(hit.transform);
                        }
                    }
                    else
                    {
                        Vector3 placementPosition = wand.position + (wand.forward * 15);

                        cursor.forward = wand.forward;
                        cursor.position = placementPosition;

                        lineRenderer.SetPosition(1, placementPosition);
                    }

                    // Paste
                    if (getReal3D.Input.GetButtonDown(Inputs.x))
                    {
                        modelCache.Paste(GetCursorPosition());
                    }

                    cursor.gameObject.SetActive(true);
                }
                else // Select mode
                {
                    // Make cursor conform to geometry
                    if (Physics.Raycast(wand.position, wand.forward, out RaycastHit hit, Mathf.Infinity, 1 << LayerMask.NameToLayer(Layer.model), QueryTriggerInteraction.Collide))
                    {
                        ShowCursorAtHit(hit);
                        cursorRenderer.material = foundModelMaterial;
                        cursor.gameObject.SetActive(true);

                        lineRenderer.material = foundModelMaterial;

                        // Try select hovered object
                        if (getReal3D.Input.GetButtonDown(Inputs.a))
                        {
                            OnSelect?.Invoke(hit.transform, hit.point);
                        }
                    }
                    else
                    {
                        cursor.gameObject.SetActive(false);

                        lineRenderer.SetPosition(1, wand.position + (wand.forward * 1000));
                        lineRenderer.material = didNotFindModelMaterial;
                    }
                }
            }
            else // Object is selected
            {
                Active = false;

                lineRenderer.enabled = false;
                cursor.gameObject.SetActive(false);

                if (!radialMenu.InMenu && SelectedObject != null)
                {
                    // Switch transform mode
                    if (getReal3D.Input.GetButtonDown(Inputs.a))
                    {
                        transformModeIndex++;
                        if (transformModeIndex >= transformModes.Length)
                        {
                            transformModeIndex = 0;
                        }

                        CursorTransformMode = transformModes[transformModeIndex].transformMode;
                    }
                    else if (getReal3D.Input.GetButtonDown(Inputs.x)) // Toggle transform
                    {
                        if (CursorTransformMode == TransformMode.None)
                        {
                            CursorTransformMode = transformModes[transformModeIndex].transformMode;
                        }
                        else
                        {
                            CursorTransformMode = TransformMode.None;
                        }
                    }
                    else if (getReal3D.Input.GetButtonDown(Inputs.b)) // Deselect
                    {
                        SelectedObject = null;
                    }
                    else if (getReal3D.Input.GetButtonDown(Inputs.leftShoulder) && SelectedObject.parent == SceneDescriptionManager.Scene) // Delete
                    {
                        Destroy(SelectedObject.gameObject);
                        SelectedObject = null;
                    }
                }
            }
        }
        else
        {
            lineRenderer.enabled = false;
            cursor.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Places the cursor at the given hit
    /// </summary>
    /// <param name="hit">The hit to place at</param>
    private void ShowCursorAtHit(RaycastHit hit)
    {
        cursor.position = hit.point + (hit.normal * 0.1f); // Position
        cursor.forward = -hit.normal; // Rotation

        // Draw line to point
        lineRenderer.SetPosition(1, hit.point + (hit.normal * 0.1f));
    }

    /// <summary>
    /// Selects the given object
    /// </summary>
    /// <param name="selection">Object to select</param>
    /// <param name="transformModes">Objects' transform modes</param>
    /// <param name="selectionAnchor">Objects' selection anchor</param>
    public void SelectObject(Transform selection, TransformModeAndControls[] transformModes, Transform selectionAnchor)
    {
        this.transformModes = transformModes;
        transformModeIndex = 0;

        CursorTransformMode = transformModes[transformModeIndex].transformMode;

        this.selectionAnchor = selectionAnchor;

        SelectedObject = selection;
    }

    /// <summary>
    /// Deselects the object
    /// </summary>
    public void DeselectObject()
    {
        SelectedObject = null;
    }

    /// <summary>
    /// Generic position transform control
    /// </summary>
    /// <param name="transform">The object to move</param>
    public void Position(Transform transform)
    {
        // Get inputs
        Vector2 movementInput = new Vector2(getReal3D.Input.GetAxis(Inputs.leftStickY), getReal3D.Input.GetAxis(Inputs.leftStickX));
        float upDownInput = getReal3D.Input.GetAxis(Inputs.rightStickY);

        // Determine speed
        float speed = 10 * CurrentOptions.options.positionSpeed;

        // Apply movement
        transform.Translate((wand.right * movementInput.y + wand.forward * movementInput.x + Vector3.up * upDownInput) * speed * getReal3D.Cluster.deltaTime, Space.World);
    }

    /// <summary>
    /// Generic rotation transform control
    /// </summary>
    /// <param name="transform">The object to rotate</param>
    /// <param name="resetRotation">The default rotation</param>
    /// <param name="inverted">Should input be inverted?</param>
    public void Rotate(Transform transform, Quaternion resetRotation, bool inverted = false)
    {
        // Get input
        float rotateX = getReal3D.Input.GetAxis(Inputs.leftStickY);
        float rotateY = getReal3D.Input.GetAxis(Inputs.leftStickX);

        // Invert input if necessary
        if (inverted)
        {
            rotateX *= -1;
            rotateY *= -1;
        }

        // Determine speed
        float speed = 100 * CurrentOptions.options.rotationSpeed;

        // Rotate y axis
        transform.Rotate(new Vector3(0, -rotateY, 0) * speed * getReal3D.Cluster.deltaTime, Space.World);

        // Rotate x axis relative to the wand
        transform.RotateAround(transform.position, wand.right, rotateX * speed * getReal3D.Cluster.deltaTime);

        // Reset the rotation
        if (getReal3D.Input.GetButtonDown(Inputs.rightShoulder))
        {
            transform.rotation = resetRotation;
        }
    }

    /// <summary>
    /// Gets the cursors current position
    /// </summary>
    /// <returns>The cursors position</returns>
    public Vector3 GetCursorPosition()
    {
        return cursor.position;
    }

    /// <summary>
    /// Toggles edit mode
    /// </summary>
    public void ToggleEditMode()
    {
        editMode = !editMode;

        SelectedObject = null;
    }
}
