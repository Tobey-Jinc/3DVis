using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vertex;

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

public class ObjectCursor : MonoBehaviour
{
    public static ObjectCursor Instance;

    [SerializeField] private RadialMenu radialMenu;
    [SerializeField] private KeyboardInput keyboardInput;
    [SerializeField] private Transform wand;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private Transform cursor;
    [SerializeField] private Material foundModelMaterial;
    [SerializeField] private Material didNotModelMaterial;

    [Header("Selection")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Transform transformIcon;
    [SerializeField] private SpriteRenderer transformIconSprite;
    [SerializeField] private Sprite positionIcon;
    [SerializeField] private Color positionColor;
    [SerializeField] private Sprite rotationIcon;
    [SerializeField] private Color rotationColor;
    [SerializeField] private Sprite scaleIcon;
    [SerializeField] private Color scaleColor;
    private Transform selectionAnchor;

    private TransformMode[] transformModes;
    private int transformModeIndex = 0;

    public TransformMode CursorTransformMode { get; private set; } = TransformMode.Position;
    public Transform SelectedObject { get; private set; } = null;
    public bool Active { get; private set; } = false;

    public delegate void SelectEvent(Transform selection, Vector3 selectionPoint);
    public event SelectEvent OnSelect;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (SelectedObject != null)
        {
            transformIcon.LookAt(cameraTransform);
            transformIcon.position = selectionAnchor.position;
            transformIcon.gameObject.SetActive(true);

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
                    transformIconSprite.sprite = scaleIcon;
                    transformIconSprite.color = scaleColor;
                    break;

                case TransformMode.Brightness:
                    transformIconSprite.sprite = scaleIcon;
                    transformIconSprite.color = scaleColor;
                    break;
            }
        }
        else
        {
            transformIcon.gameObject.SetActive(false);
        }
    }

    void LateUpdate()
    {
        if (!radialMenu.InMenu && !keyboardInput.InMenu && SelectedObject == null && getReal3D.Input.GetButton(Inputs.rightShoulder))
        {
            Active = true;

            lineRenderer.enabled = true;
            lineRenderer.SetPosition(0, wand.position);

            if (Physics.Raycast(wand.position, wand.forward, out RaycastHit hit, Mathf.Infinity, 1 << LayerMask.NameToLayer(Layer.model), QueryTriggerInteraction.Collide))
            {
                cursor.position = hit.point + (hit.normal * 0.1f);
                cursor.forward = -hit.normal;
                cursor.gameObject.SetActive(true);

                lineRenderer.SetPosition(1, hit.point + (hit.normal * 0.1f));
                lineRenderer.material = foundModelMaterial;

                if (getReal3D.Input.GetButtonDown(Inputs.a))
                {
                    OnSelect?.Invoke(hit.transform, hit.point);
                }
            }
            else
            {
                cursor.gameObject.SetActive(false);

                lineRenderer.SetPosition(1, wand.position + (wand.forward * 1000));
                lineRenderer.material = didNotModelMaterial;
            }
        }
        else
        {
            Active = false;

            lineRenderer.enabled = false;
            cursor.gameObject.SetActive(false);

            if (SelectedObject != null)
            {
                if (getReal3D.Input.GetButtonDown(Inputs.a))
                {
                    transformModeIndex++;
                    if (transformModeIndex >= transformModes.Length)
                    {
                        transformModeIndex = 0;
                    }

                    CursorTransformMode = transformModes[transformModeIndex];
                }
                else if (getReal3D.Input.GetButtonDown(Inputs.x))
                {
                    if (CursorTransformMode == TransformMode.None)
                    {
                        CursorTransformMode = transformModes[transformModeIndex];
                    }
                    else
                    {
                        CursorTransformMode = TransformMode.None;
                    }
                }
                else if (getReal3D.Input.GetButtonDown(Inputs.b))
                {
                    SelectedObject = null;
                }
                else if (getReal3D.Input.GetButtonDown(Inputs.leftShoulder))
                {
                    Destroy(SelectedObject.gameObject);
                    SelectedObject = null;
                }
            }
        }
    }

    public void SelectObject(Transform selection, TransformMode[] transformModes, Transform selectionAnchor)
    {
        this.transformModes = transformModes;
        transformModeIndex = 0;

        CursorTransformMode = transformModes[transformModeIndex];

        this.selectionAnchor = selectionAnchor;

        SelectedObject = selection;
    }

    public void DeselectObject()
    {
        SelectedObject = null;
    }

    public void Position(Transform transform)
    {
        Vector2 movementInput = new Vector2(getReal3D.Input.GetAxis(Inputs.leftStickY), getReal3D.Input.GetAxis(Inputs.leftStickX));
        float upDownInput = getReal3D.Input.GetAxis(Inputs.rightStickY);

        transform.Translate((wand.right * movementInput.y + wand.forward * movementInput.x + Vector3.up * upDownInput) * 5 * getReal3D.Cluster.deltaTime, Space.World);
    }

    public void Rotate(Transform transform)
    {
        float rotateX = getReal3D.Input.GetAxis(Inputs.leftStickY);
        float rotateY = getReal3D.Input.GetAxis(Inputs.leftStickX);

        transform.Rotate(new Vector3(0, -rotateY, 0) * 50 * getReal3D.Cluster.deltaTime, Space.World);

        transform.RotateAround(transform.position, wand.right, rotateX * 50 * getReal3D.Cluster.deltaTime);

        if (getReal3D.Input.GetButtonDown(Inputs.rightShoulder))
        {
            transform.rotation = Quaternion.identity;
        }
    }
}
