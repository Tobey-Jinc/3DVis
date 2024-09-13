using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GLTFast;

public class ModelParent : MonoBehaviour
{
    [SerializeField] private GltfAsset asset;

    private int previousChildCount = 0;

    private void LateUpdate()
    {
        if (previousChildCount != transform.hierarchyCount)
        {
            Debug.Log((previousChildCount, transform.hierarchyCount));
            PrepareChildren(transform);
            previousChildCount = transform.hierarchyCount;
        }
    }

    public void Setup(string path)
    {
        asset.Load(path);
    }

    private void PrepareChildren(Transform parent)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);

            if (child.GetComponent<MeshRenderer>() != null)
            {
                ModelChild modelChild = child.gameObject.AddComponent<ModelChild>();
                modelChild.Parent = this;

                child.gameObject.layer = 6;

                child.gameObject.AddComponent<MeshCollider>();
            }

            if (child.childCount > 0)
            {
                PrepareChildren(child);
            }
        }
    }

    public void Select()
    {
        //gizmoManager.Select(this);
    }
}
