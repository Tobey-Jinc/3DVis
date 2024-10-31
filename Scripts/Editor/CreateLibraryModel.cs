using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if (UNITY_EDITOR)
using UnityEditor;

class CreateLibraryModel : EditorWindow
{
    /// <summary>
    /// Converts the selected object to a model
    /// </summary>
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
            modelParent.libraryModel = true;
        }

        // Prepare children
        PrepareChildren(parent, modelParent);

        Undo.CollapseUndoOperations(undoGroupIndex);
    }

    /// <summary>
    /// Prepares children of a model
    /// </summary>
    /// <param name="parent">The childs immediate parent</param>
    /// <param name="modelParent">The ModelParent that all children fall under</param>
    static void PrepareChildren(Transform parent, ModelParent modelParent)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);

            // Add components
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

            // Execute recursively
            if (child.childCount > 0)
            {
                PrepareChildren(child, modelParent);
            }
        }
    }
}
#endif