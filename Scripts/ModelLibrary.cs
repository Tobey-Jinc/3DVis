using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vertex;

[System.Serializable]
public struct Model
{
    [SerializeField] public string id;
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

    public Model GetModel(string id)
    {
        foreach (Model model in models)
        {
            if (model.id == id)
            {
                return model;
            }
        }

        return new Model();
    }

    public FileStructure GetFileStructure(bool quickPlace)
    {
        fileStructure.closeOnSelect = quickPlace;

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
