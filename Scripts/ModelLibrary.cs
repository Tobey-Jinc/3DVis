using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vertex;

/// <summary>
/// Defines the fundamental data of a model library model
/// </summary>
[System.Serializable]
public struct Model
{
    [SerializeField] public string id; // ID must be unique as it is used when saving and loading your scenes
    [SerializeField] public string displayName;
    [SerializeField] public string category;
    [SerializeField] public ModelParent prefab;
}

public class ModelLibrary : MonoBehaviour
{
    [SerializeField] private Model[] models;
    [SerializeField] private ModelCache modelCache;
    [SerializeField] private ObjectCursor cursor;

    private FileStructure fileStructure;

    void Start()
    {
        // Create the file structure
        Dictionary<string, List<string[]>> files = new();

        for (int i = 0; i < models.Length; i++)
        {
            Model model = models[i];

            string[] file = new string[] { model.displayName, model.displayName };

            FileSelection.AddFile(files, Data.allCategory, file);
            FileSelection.AddFile(files, model.category, file);
        }

        fileStructure = new FileStructure("Select a Model", files);
    }

    /// <summary>
    /// Returns the model with the given ID
    /// </summary>
    /// <param name="id">The ID to search for</param>
    /// <returns>A Model struct. Will return an empty Model if nothing could be found!</returns>
    public Model GetModel(string id)
    {
        // Search for the ID
        foreach (Model model in models)
        {
            if (model.id == id)
            {
                return model;
            }
        }

        return new Model();
    }

    /// <summary>
    /// Gets the file structure of the library
    /// </summary>
    /// <param name="quickPlace">True if the file structure is being used in quick place mode</param>
    /// <returns></returns>
    public FileStructure GetFileStructure(bool quickPlace)
    {
        fileStructure.closeOnSelect = quickPlace;

        // Use different actions if in quick place mode
        if (quickPlace)
        {
            fileStructure.action = (string id) => { modelCache.InstantiateModelFromLibrary(GetModel(id), cursor.GetCursorPosition(), true); };
        }
        else
        {
            fileStructure.action = (string id) => { modelCache.InstantiateModelFromLibrary(GetModel(id)); };
        }

        return fileStructure;
    }
}
