using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Model
{
    [SerializeField] public string id;
    [SerializeField] public string displayName;
    [SerializeField] public ModelParent prefab;
}

public class ModelLibrary : MonoBehaviour
{
    [SerializeField] private Model[] models;
    [SerializeField] private ModelCache modelCache;

    private FileStructure fileStructure;

    void Start()
    {
        string[][] files = new string[models.Length][];

        for (int i = 0; i < models.Length; i++)
        {
            Model model = models[i];

            files[i] = new string[] { model.displayName, model.displayName };
        }

        fileStructure = new FileStructure("Select a Model", files, (string id) => { modelCache.InstantiateModelFromLibrary(GetModel(id)); });
    }

    private Model GetModel(string id)
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

    public FileStructure GetFileStructure()
    {
        return fileStructure;
    }
}
