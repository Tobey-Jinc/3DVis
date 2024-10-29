using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if (UNITY_EDITOR)
using UnityEditor;

class CreateLibraryModel : EditorWindow
{
    [MenuItem("Scripts/Convert To Library Model")]
    static void ConvertToLibraryModel()
    {
        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("Converted to library model");
        int undoGroupIndex = Undo.GetCurrentGroup();

        Transform parent = Selection.transforms[0];

        // Create parent
        ModelParent modelParent = parent.GetComponent<ModelParent>();
        if (modelParent == null)
        {
            Undo.RegisterCompleteObjectUndo(parent.gameObject, "Create Model Parent");

            parent.gameObject.AddComponent<ModelParent>();

            modelParent = parent.GetComponent<ModelParent>();
        }

        // Prepare children
        PrepareChildren(parent, modelParent);

        Undo.CollapseUndoOperations(undoGroupIndex);
    }

    static void PrepareChildren(Transform parent, ModelParent modelParent)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);

            if (child.GetComponent<MeshRenderer>() != null)
            {
                Undo.RegisterCompleteObjectUndo(child.gameObject, "Prepared model child");

                if (child.gameObject.GetComponent<ModelChild>() == null)
                {
                    child.gameObject.AddComponent<ModelChild>();
                }
                ModelChild modelChild = child.gameObject.GetComponent<ModelChild>();
                modelChild.Parent = modelParent;

                child.gameObject.layer = 6;

                if (child.gameObject.GetComponent<MeshCollider>() == null)
                {
                    child.gameObject.AddComponent<MeshCollider>();
                }
            }

            if (child.childCount > 0)
            {
                PrepareChildren(child, modelParent);
            }
        }
    }
}
#endif