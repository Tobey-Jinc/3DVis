using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SceneDescriptionManager : MonoBehaviour
{
    public static Transform Scene;

    [SerializeField] private Transform scene;

    [SerializeField] private Environments environments;
    [SerializeField] private ModelCursor modelCursor;
    [SerializeField] private ModelCache modelCache;

    private void Awake()
    {
        Scene = scene;
    }

    public void GenerateSceneDescription()
    {
        SceneDescription sd = new SceneDescription();
        sd.name = "My Cool Scene";
        sd.environmentPresetID = environments.CurrentEnvironmentID;

        // Save Models
        ModelParent[] modelParents = FindObjectsOfType<ModelParent>();
        SDModel[] models = new SDModel[modelParents.Length];

        for (int i = 0; i < modelParents.Length; i++)
        {
            ModelParent modelParent = modelParents[i];
            SDModel model = new SDModel();

            model.id = modelParent.FolderName;

            model.position = modelParent.transform.position;
            model.rotation = modelParent.transform.localRotation;
            model.scale = modelParent.transform.localScale;

            models[i] = model;
        }

        sd.models = models;

        string sceneJSON = JsonUtility.ToJson(sd, true);

        if (!Directory.Exists(Application.persistentDataPath + "/Scenes"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/Scenes");
        }

        File.WriteAllText(Application.persistentDataPath + "/Scenes/Save.json", sceneJSON);
    }

    public async void LoadSceneDescription()
    {
        modelCursor.DeselectObject();

        foreach (Transform child in scene)
        {
            Destroy(child.gameObject);
        }

        string sceneJSON = File.ReadAllText(Application.persistentDataPath + "/Scenes/Save.json");
        SceneDescription sceneDescription = JsonUtility.FromJson<SceneDescription>(sceneJSON);

        environments.SetEnvironment(sceneDescription.environmentPresetID);

        foreach (SDModel model in sceneDescription.models)
        {
            await modelCache.InstantiateModelFromSceneDescription(model);
        }
    }
}
