using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelChild : MonoBehaviour
{
    [SerializeField] private ModelParent parent;

    public ModelParent Parent { get => parent; set => parent = value; }

    void Start()
    {
        // Subscribe to events
        ObjectCursor.Instance.OnSelect += Cursor_OnSelect;
        ObjectCursor.Instance.OnCopy += Cursor_OnCopy;
    }

    /// <summary>
    /// Executed when this child is selected. Selects the parent
    /// </summary>
    private void Cursor_OnSelect(Transform selection, Vector3 selectionPoint)
    {
        if (selection == transform)
        {
            parent.Select(selectionPoint);
        }
    }

    /// <summary>
    /// Copies the parent
    /// </summary>
    private void Cursor_OnCopy(Transform selection)
    {
        if (selection == transform)
        {
            ModelCache.Instance.Copy(parent.gameObject);
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        ObjectCursor.Instance.OnSelect -= Cursor_OnSelect;
        ObjectCursor.Instance.OnCopy -= Cursor_OnCopy;
    }
}