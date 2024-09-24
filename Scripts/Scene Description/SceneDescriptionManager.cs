using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SceneDescriptionManager : MonoBehaviour
{
    [SerializeField] private Environments environments;

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

            model.path = modelParent.Path;

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
}
