using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Vertex;

public class SceneDescriptionManager : MonoBehaviour
{
    public static Transform Scene;

    [SerializeField] private Transform scene;

    [SerializeField] private NetworkFolderDownloader networkFolderDownloader;
    [SerializeField] private Environments environments;
    [SerializeField] private ModelCursor modelCursor;
    [SerializeField] private ModelCache modelCache;

    private FileStructure fileStructure;

    private List<string> takenNames = new List<string>();

    private void Awake()
    {
        Scene = scene;
    }

    private void Start()
    {
        fileStructure = new FileStructure();
        fileStructure.title = "Select a scene";
        fileStructure.action = (string sceneName) => 
        { 
            LoadScene(sceneName); 
        };

        if (!Directory.Exists(Paths.GetSceneFolder()))
        {
            Directory.CreateDirectory(Paths.GetSceneFolder());
        }

        networkFolderDownloader.Download("scenes", () => { StoreTakenNames(); });
    }

    public void StoreTakenNames()
    {
        string[] scenes = Directory.GetFiles(Paths.GetSceneFolder());
        foreach (string scene in scenes)
        {
            takenNames.Add(Path.GetFileNameWithoutExtension(scene));
        }
    }

    public void SaveScene(string sceneName)
    {
        try
        {
            SceneDescription sd = new SceneDescription();

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

            File.WriteAllText(Paths.GetSceneFolder() + sceneName + ".json", sceneJSON);

            if (getReal3D.Cluster.isMaster)
            {
                File.WriteAllText("\\\\CAVE-HEADNODE\\data\\3dvis\\scenes\\" + sceneName + ".json", sceneJSON);
            }

            takenNames.Add(sceneName);
        }
        catch (System.Exception e)
        {
            Debug.Log("ERROR OCCURRED WHILE SAVING SCENE");
            Debug.Log(e);
        }
    }

    private async void LoadScene(string sceneName)
    {
        modelCursor.DeselectObject();

        foreach (Transform child in scene)
        {
            Destroy(child.gameObject);
        }

        string sceneJSON = File.ReadAllText(Paths.GetSceneFolder() + sceneName + ".json");
        SceneDescription sceneDescription = JsonUtility.FromJson<SceneDescription>(sceneJSON);

        environments.SetEnvironment(sceneDescription.environmentPresetID);

        foreach (SDModel model in sceneDescription.models)
        {
            await modelCache.InstantiateModelFromSceneDescription(model);
        }
    }

    public FileStructure GetFileStructure()
    {
        string[] sceneFiles = Directory.GetFiles(Paths.GetSceneFolder());
        string[][] scenes = new string[sceneFiles.Length][];

        for (int i = 0; i < sceneFiles.Length; i++)
        {
            string sceneName = Path.GetFileNameWithoutExtension(sceneFiles[i]);
            scenes[i] = new string[] { sceneName, sceneName };
        }

        fileStructure.SetFiles(scenes);

        return fileStructure;
    }

    public bool ValidateSceneName(string sceneName)
    {
        return !takenNames.Contains(sceneName);
    }
}
