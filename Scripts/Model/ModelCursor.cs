using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vertex;

public enum TransformMode
{
    None,
    Position,
    Rotation,
    Scale
}

public class ModelCursor : MonoBehaviour
{
    public static ModelCursor Instance;

    [SerializeField] private RadialMenu radialMenu;
    [SerializeField] private Transform wand;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private Transform cursor;
    [SerializeField] private Material foundModelMaterial;
    [SerializeField] private Material didNotModelMaterial;

    public TransformMode TransformMode { get; private set; } = TransformMode.Position;
    public Transform SelectedObject { get; private set; } = null;
    public bool Active { get; private set; } = false;

    public delegate void SelectEvent(Transform selection, Vector3 selectionPoint);
    public event SelectEvent OnSelect;

    private void Awake()
    {
        Instance = this;
    }

    void LateUpdate()
    {
        if (!radialMenu.InMenu && SelectedObject == null && getReal3D.Input.GetButton(Inputs.rightShoulder))
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
                    SetTransformMode(TransformMode.Position);
                }
                else if (getReal3D.Input.GetButtonDown(Inputs.x))
                {
                    SetTransformMode(TransformMode.Rotation);
                }
                else if (getReal3D.Input.GetButtonDown(Inputs.y))
                {
                    SetTransformMode(TransformMode.Scale);
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

    private void SetTransformMode(TransformMode desiredMode)
    {
        if (TransformMode != desiredMode)
        {
            TransformMode = desiredMode;
        }
        else
        {
            TransformMode = TransformMode.None;
        }
    }

    public void SelectObject(Transform selection)
    {
        SelectedObject = selection;
    }

    public void DeselectObject()
    {
        SelectedObject = null;
    }
}
