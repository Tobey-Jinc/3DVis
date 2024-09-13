using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelChild : MonoBehaviour
{
    private ModelParent parent;

    public ModelParent Parent { get => parent; set => parent = value; }

    void Start()
    {
        
    }

    private void OnSelect(Transform selection)
    {
        if (selection == transform)
        {
            parent.Select();
        }
    }
}