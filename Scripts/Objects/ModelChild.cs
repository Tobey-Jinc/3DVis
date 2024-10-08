using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelChild : MonoBehaviour
{
    [SerializeField] private ModelParent parent;

    public ModelParent Parent { get => parent; set => parent = value; }

    void Start()
    {
        ObjectCursor.Instance.OnSelect += OnSelect;
    }

    private void OnSelect(Transform selection, Vector3 selectionPoint)
    {
        if (selection == transform)
        {
            parent.Select(selectionPoint);
        }
    }

    private void OnDestroy()
    {
        ObjectCursor.Instance.OnSelect -= OnSelect;
    }
}