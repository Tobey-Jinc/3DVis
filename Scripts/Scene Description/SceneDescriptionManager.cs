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
    [SerializeField] private ObjectCursor modelCursor;
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

            sd.models = GetModels();

            sd.text = GetText();

            sd.lights = GetLights();

            sd.audio = GetAudio();

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

    private SDModel[] GetModels()
    {
        ModelParent[] modelParents = FindObjectsOfType<ModelParent>();
        SDModel[] models = new SDModel[modelParents.Length];

        for (int i = 0; i < modelParents.Length; i++)
        {
            ModelParent modelParent = modelParents[i];
            SDModel sdModel = new SDModel();

            sdModel.id = modelParent.FolderName;
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

        for (int i = 0; i < textObjects.Length; i++)
        {
            TextObject textObject = textObjects[i];
            SDText sdText = new SDText();

            sdText.text = textObject.Text.text;

            sdText.position = textObject.transform.position;
            sdText.rotation = textObject.transform.localRotation;
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

        for (int i = 0; i < audioObjects.Length; i++)
        {
            AudioObject audioObject = audioObjects[i];
            SDAudio sdAudio = new SDAudio();

            sdAudio.id = audioObject.MyAudio.id;

            sdAudio.position = audioObject.transform.position;
            sdAudio.volume = audioObject.AudioSource.volume;
            sdAudio.spatialBlend = audioObject.AudioSource.spatialBlend;
            sdAudio.minDistance = audioObject.AudioSource.minDistance;
            sdAudio.maxDistance = audioObject.AudioSource.maxDistance;

            audio[i] = sdAudio;
        }

        return audio;
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
