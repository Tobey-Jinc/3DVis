using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using Vertex;
using System.Security.Cryptography;

public class SceneDescriptionManager : MonoBehaviour
{
    public static bool LoadTempSceneOnLoad;

    public static Transform Scene;

    [SerializeField] private Transform scene;

    [SerializeField] private NetworkFolderDownloader networkFolderDownloader;
    [SerializeField] private Environments environments;
    [SerializeField] private ObjectCursor modelCursor;
    [SerializeField] private ModelCache modelCache;
    [SerializeField] private SkyObject skyObject;

    private FileStructure fileStructure;

    private List<string> takenNames = new List<string>();

    private void Awake()
    {
        Scene = scene;
    }

    private void Start()
    {
        // Create file structure
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

        // Download scenes
        networkFolderDownloader.Download("scenes", () => { StoreTakenNames(); });
    }

    /// <summary>
    /// Store taken names so you can't overwrite scenes
    /// </summary>
    public void StoreTakenNames()
    {
        string[] scenes = Directory.GetFiles(Paths.GetSceneFolder());
        foreach (string scene in scenes)
        {
            takenNames.Add(Path.GetFileNameWithoutExtension(scene));
        }
    }

    /// <summary>
    /// Saves the scene
    /// </summary>
    /// <param name="sceneName">The name of the scene</param>
    /// <param name="saveAsTempScene">Whether or not this scene is a temp scene for app reloading</param>
    public void SaveScene(string sceneName, bool saveAsTempScene = false)
    {
        try
        {
            SceneDescription sd = new SceneDescription();

            sd.sceneDisplayName = sceneName;

            sd.environmentPresetID = environments.CurrentEnvironmentID;

            // Store all object types
            sd.sky = GetSky();

            sd.models = GetModels();

            sd.text = GetText();

            sd.lights = GetLights();

            sd.audio = GetAudio();

            // Conver to JSON
            string sceneJSON = JsonUtility.ToJson(sd, true);

            // Save the scene
            if (!saveAsTempScene)
            {
                File.WriteAllText(Paths.GetSceneFolder() + sceneName + ".json", sceneJSON);

                if (getReal3D.Cluster.isMaster)
                {
#if UNITY_EDITOR
                    File.WriteAllText(Application.persistentDataPath + "/" + sceneName + ".json", sceneJSON);
#else
                    File.WriteAllText("\\\\CAVE-HEADNODE\\data\\3dvis\\scenes\\" + sceneName + ".json", sceneJSON);
#endif
                }
            }
            else
            {
                File.WriteAllText(Application.persistentDataPath + "/Temp Scene.json", sceneJSON);
            }

            takenNames.Add(sceneName);
        }
        catch (System.Exception e)
        {
            Debug.Log("ERROR OCCURRED WHILE SAVING SCENE");
            Debug.Log(e);
        }
    }

    private SDSky GetSky()
    {
        SDSky sky = new SDSky();

        sky.position = skyObject.transform.position;
        sky.rotation = skyObject.transform.rotation;
        sky.skyboxIndex = skyObject.SkyboxIndex;
        sky.colorIndex = skyObject.ColorIndex;
        sky.intensity = skyObject.Sun.intensity;
        sky.shadows = skyObject.Sun.shadows;

        return sky;
    }

    private SDModel[] GetModels()
    {
        ModelParent[] modelParents = FindObjectsOfType<ModelParent>();
        SDModel[] models = new SDModel[modelParents.Length];

        // Get each model
        for (int i = 0; i < modelParents.Length; i++)
        {
            ModelParent modelParent = modelParents[i];
            SDModel sdModel = new SDModel();

            sdModel.id = modelParent.folderName;
            sdModel.libraryModel = modelParent.LibraryModel;

            sdModel.position = modelParent.transform.position;
            sdModel.rotation = modelParent.transform.localRotation;
            sdModel.scale = modelParent.transform.localScale;

            models[i] = sdModel;
        }

        return models;
    }

    private SDText[] GetText()
    {
        TextObject[] textObjects = FindObjectsOfType<TextObject>();
        SDText[] text = new SDText[textObjects.Length];

        // Get all text
        for (int i = 0; i < textObjects.Length; i++)
        {
            TextObject textObject = textObjects[i];
            SDText sdText = new SDText();

            sdText.text = textObject.Text.text;

            sdText.position = textObject.transform.position;
            sdText.rotation = textObject.transform.localRotation;
            sdText.colorIndex = textObject.ColorIndex;
            sdText.fontSize = textObject.Text.fontSize;
            sdText.width = textObject.RectTransform.sizeDelta.x;
            sdText.textAlignment = textObject.Text.alignment;

            text[i] = sdText;
        }

        return text;
    }

    private SDLight[] GetLights()
    {
        LightObject[] lightObjects = FindObjectsOfType<LightObject>();
        SDLight[] lights = new SDLight[lightObjects.Length];

        // Get all lights
        for (int i = 0; i < lightObjects.Length; i++)
        {
            LightObject lightObject = lightObjects[i];
            SDLight sdLight = new SDLight();

            sdLight.position = lightObject.transform.position;
            sdLight.colorIndex = lightObject.ColorIndex;
            sdLight.range = lightObject.LightSource.range;
            sdLight.intensity = lightObject.LightSource.intensity;
            sdLight.shadows = lightObject.LightSource.shadows;

            lights[i] = sdLight;
        }

        return lights;
    }

    private SDAudio[] GetAudio()
    {
        AudioObject[] audioObjects = FindObjectsOfType<AudioObject>();
        SDAudio[] audio = new SDAudio[audioObjects.Length];

        // Get all audio
        for (int i = 0; i < audioObjects.Length; i++)
        {
            AudioObject audioObject = audioObjects[i];
            SDAudio sdAudio = new SDAudio();

            sdAudio.id = audioObject.myAudio.id;

            sdAudio.position = audioObject.transform.position;
            sdAudio.volume = audioObject.AudioSource.volume;
            sdAudio.spatialBlend = audioObject.AudioSource.spatialBlend;
            sdAudio.minDistance = audioObject.AudioSource.minDistance;
            sdAudio.maxDistance = audioObject.AudioSource.maxDistance;

            audio[i] = sdAudio;
        }

        return audio;
    }

    /// <summary>
    /// Loads the given scene
    /// </summary>
    /// <param name="sceneName">The scene to load</param>
    /// <param name="tempScene">Whether or not the scene being loaded is a temp scene</param>
    private async void LoadScene(string sceneName, bool tempScene = false)
    {
        ClearScene();

        // Get scene JSON
        string sceneJSON;
        if (!tempScene)
        {
            sceneJSON = File.ReadAllText(Paths.GetSceneFolder() + sceneName + ".json");
        }
        else
        {
            // Load the temp scene
            string tempFilePath = Application.persistentDataPath + "/Temp Scene.json";
            if (!File.Exists(tempFilePath))
            {
                return;
            }

            sceneJSON = File.ReadAllText(Application.persistentDataPath + "/Temp Scene.json");
        }

        // Setup the scene

        SceneDescription sceneDescription = JsonUtility.FromJson<SceneDescription>(sceneJSON);

        environments.SetEnvironment(sceneDescription.environmentPresetID);

        // Load each object type

        skyObject.Setup(sceneDescription.sky);

        foreach (SDModel model in sceneDescription.models)
        {
            await modelCache.InstantiateModelFromSceneDescription(model, model.libraryModel);
        }

        foreach (SDText text in sceneDescription.text)
        {
            modelCache.InstantiateTextFromSceneDescription(text);
        }

        foreach (SDLight light in sceneDescription.lights)
        {
            modelCache.InstantiateLightFromSceneDescription(light);
        }

        foreach (SDAudio audio in sceneDescription.audio)
        {
            modelCache.InstantiateAudioFromSceneDescription(audio);
        }
    }

    /// <summary>
    ///Generate and get the file structure
    /// </summary>
    public FileStructure GetFileStructure()
    {
        string[] sceneFiles = Directory.GetFiles(Paths.GetSceneFolder());
        Dictionary<string, List<string[]>> files = new();

        for (int i = 0; i < sceneFiles.Length; i++)
        {
            string scene = sceneFiles[i];

            string json = File.ReadAllText(scene);
            SceneDescription sceneDescription = JsonUtility.FromJson<SceneDescription>(json);

            string sceneDisplayName = sceneDescription.sceneDisplayName;
            string sceneCategory = sceneDescription.sceneCategory;
            string sceneFileName = Path.GetFileNameWithoutExtension(scene);

            string[] file = new string[] { sceneDisplayName, sceneFileName };

            FileSelection.AddFile(files, Data.allCategory, file);
            FileSelection.AddFile(files, sceneCategory, file);
        }

        fileStructure.SetFiles(files);

        return fileStructure;
    }

    /// <summary>
    /// Check if the given scene name is taken
    /// </summary>
    /// <param name="sceneName">The name to check</param>
    /// <returns>True if valid</returns>
    public bool ValidateSceneName(string sceneName)
    {
        return !takenNames.Contains(sceneName);
    }

    /// <summary>
    /// Reloads the app, saving a temp scene to be loaded after the reload
    /// </summary>
    public void ReloadApp()
    {
        SaveScene(string.Empty, true);

        LoadTempSceneOnLoad = true;
        ModelCache.Loaded = false;

        SceneManager.LoadScene(0);
    }

    /// <summary>
    /// Loads the temp scene after a reload
    /// </summary>
    public void LoadTempScene()
    {
        if (LoadTempSceneOnLoad)
        {
            LoadScene(string.Empty, true);
        }

        LoadTempSceneOnLoad = false;
    }

    /// <summary>
    /// Clears all objects in the scene, excluding the sky, and set environment
    /// </summary>
    public void ClearScene()
    {
        modelCursor.DeselectObject();

        foreach (Transform child in scene)
        {
            Destroy(child.gameObject);
        }
    }
}
