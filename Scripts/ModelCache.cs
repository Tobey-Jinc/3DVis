using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Linq;
using System.Threading.Tasks;
using Vertex;

public class ModelCache : getReal3D.MonoBehaviourWithRpc
{
    public static ModelCache Instance;
    public static bool Loaded = false;

    [SerializeField] private NetworkFolderDownloader networkFolderDownloader;
    [SerializeField] private SceneDescriptionManager sceneDescriptionManager;
    [SerializeField] private Transform wand;
    [SerializeField] private Viewpoint viewpoint;
    [SerializeField] private ObjectCursor cursor;
    [SerializeField] private ModelLibrary modelLibrary;
    [SerializeField] private AudioLibrary audioLibrary;
    [SerializeField] private Transform copyCursor;

    [Header("Prefabs")]
    [SerializeField] private ModelParent modelParentPrefab;
    [SerializeField] private TextObject textObjectPrefab;
    [SerializeField] private LightObject lightObjectPrefab;
    [SerializeField] private AudioObject audioObjectPrefab;

    [Header("Loading UI")]
    [SerializeField] private Canvas loadingScreen;

    private FileStructure fileStructure;

    private Dictionary<string, ModelParent> cachedModels = new Dictionary<string, ModelParent>();

    private string instantiateModelSyncedMethod = "InstantiateModelSynced";

    private Transform copiedObject;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        fileStructure = new FileStructure();
        fileStructure.title = "Select a model";
        fileStructure.action = (string file) => { InstantiateModelSetup(file); };

        networkFolderDownloader.Download("models", () => { GenerateFileStructure(); });
    }

    private void Update()
    {
        if (cursor.Active && getReal3D.Input.GetButton(Inputs.leftShoulder) && copiedObject != null)
        {
            copyCursor.position = copiedObject.position;
            copyCursor.LookAt(wand);

            copyCursor.gameObject.SetActive(true);
        }
        else
        {
            copyCursor.gameObject.SetActive(false);
        }
    }

    private void GenerateFileStructure()
    {
        string folder = Application.persistentDataPath + "\\models";
        string[] folders = Directory.GetDirectories(Application.persistentDataPath + "\\models").Select(Path.GetFileName).ToArray();
        Dictionary<string, List<string[]>> files = new();

        for (int i = 0; i < folders.Length; i++)
        {
            try
            {
                string metaDataText = File.ReadAllText(Paths.GetModelFolder() + folders[i] + "\\metadata.json");
                ModelMetaData metaData = JsonUtility.FromJson<ModelMetaData>(metaDataText);

                string modelDisplayName = metaData.modelDisplayName;
                string modelCategory = metaData.modelCategory;

                string[] file = new string[] { modelDisplayName, folders[i] };

                FileSelection.AddFile(files, Data.allCategory, file);
                FileSelection.AddFile(files, modelCategory, file);
            }
            catch (Exception e)
            {
                Debug.Log($"An error occurred whilst reading the model... \n{e}");
            }
        }

        fileStructure.SetFiles(files);

        Loaded = true;

        loadingScreen.enabled = false;

        sceneDescriptionManager.LoadTempScene();
    }

    private Vector3 GetSpawnPosition(Vector3? position = null)
    {
        if (position == null)
        {
            if (Physics.Raycast(wand.position, wand.forward, out RaycastHit hit, 10, 1, QueryTriggerInteraction.Ignore))
            {
                return hit.point;
            }

            return wand.position + (wand.forward * 10);
        }

        return (Vector3)position;
    }

    private void InstantiateModelSetup(string modelPath, Vector3? position = null, bool copy = false)
    {
        if (getReal3D.Cluster.isMaster)
        {
            Vector3 spawnPosition = GetSpawnPosition();

            CallRpc(instantiateModelSyncedMethod, modelPath, spawnPosition, copy);
            InstantiateModel(modelPath, spawnPosition, copy);
        }
    }

    [getReal3D.RPC]
    private void InstantiateModelSynced(string modelPath, Vector3 spawnPosition, bool copy = false)
    {
        if (!getReal3D.Cluster.isMaster)
        {
            InstantiateModel(modelPath, spawnPosition, copy);
        }
    }

    private async Task<Transform> InstantiateModel(string modelPath, Vector3 spawnPosition, bool copy = false)
    {
        ModelParent cachedModel = GetCachedModel(modelPath);
        if (cachedModel == null)
        {
            Debug.Log("Model has not been cached. Importing model and trying again...");

            ModelParent importedModel = Instantiate(modelParentPrefab, transform);

            await importedModel.Setup(modelPath);

            importedModel.gameObject.SetActive(false);
            CacheModel(modelPath, importedModel);

            return await InstantiateModel(modelPath, spawnPosition, copy);
        }
        else
        {
            ModelParent model = Instantiate(cachedModel, SceneDescriptionManager.Scene);
            model.CachedSetup(modelPath);

            model.transform.position = spawnPosition;

            model.gameObject.SetActive(true);

            viewpoint.SyncTransformWithHeadnode();

            if (copy)
            {
                copiedObject = model.transform;
            }

            return model.transform;
        }
    }

    public void InstantiateModelFromLibrary(Model model, Vector3? position = null, bool copy = false)
    {
        ModelParent modelParent = Instantiate(model.prefab, SceneDescriptionManager.Scene);
        modelParent.FolderName = model.id;

        modelParent.transform.position = GetSpawnPosition(position);

        if (copy)
        {
            copiedObject = modelParent.transform;
        }
    }

    public void InstantiateTextObject(Vector3? position = null, bool copy = false)
    {
        TextObject textObject = Instantiate(textObjectPrefab, SceneDescriptionManager.Scene);

        textObject.transform.position = GetSpawnPosition(position);
        textObject.transform.LookAt(wand.position);

        textObject.transform.rotation = Quaternion.Euler(0, textObject.transform.eulerAngles.y, 0);

        if (copy)
        {
            copiedObject = textObject.transform;
        }
    }

    public void InstantiateLightObject(Vector3? position = null, bool copy = false)
    {
        LightObject lightObject = Instantiate(lightObjectPrefab, SceneDescriptionManager.Scene);

        lightObject.transform.position = GetSpawnPosition(position);

        if (copy)
        {
            copiedObject = lightObject.transform;
        }
    }

    public void InstantiateAudioObject(Audio audio, Vector3? position = null, bool copy = false)
    {
        AudioObject audioObject = Instantiate(audioObjectPrefab, SceneDescriptionManager.Scene);

        audioObject.transform.position = GetSpawnPosition(position);

        audioObject.Setup(audio);

        if (copy)
        {
            copiedObject = audioObject.transform;
        }
    }

    public void Copy(Transform copiedObject)
    {
        this.copiedObject = copiedObject;
    }

    public void Paste(Vector3? position)
    {
        if (copiedObject != null)
        {
            Transform pastedObject = Instantiate(copiedObject, SceneDescriptionManager.Scene);
            pastedObject.position = GetSpawnPosition(position);
        }
    }

    // Scene Description Loading
    public async Task InstantiateModelFromSceneDescription(SDModel sdModel, bool libraryModel)
    {
        Transform model;

        if (!libraryModel)
        {
            model = await InstantiateModel(sdModel.id, sdModel.position);
        }
        else
        {
            Model lModel = modelLibrary.GetModel(sdModel.id);
            ModelParent modelParent = Instantiate(lModel.prefab, SceneDescriptionManager.Scene);
            modelParent.FolderName = lModel.id;

            model = modelParent.transform;
            model.position = sdModel.position;
        }

        model.rotation = sdModel.rotation;
        model.localScale = sdModel.scale;
    }

    public void InstantiateTextFromSceneDescription(SDText sdText)
    {
        TextObject textObject = Instantiate(textObjectPrefab, SceneDescriptionManager.Scene);

        textObject.Setup(sdText);   
    }

    public void InstantiateLightFromSceneDescription(SDLight sdLight)
    {
        LightObject lightObject = Instantiate(lightObjectPrefab, SceneDescriptionManager.Scene);

        lightObject.Setup(sdLight);
    }

    public void InstantiateAudioFromSceneDescription(SDAudio sdAudio)
    {
        AudioObject audioObject = Instantiate(audioObjectPrefab, SceneDescriptionManager.Scene);

        audioObject.Setup(audioLibrary.GetAudio(sdAudio.id), sdAudio);
    }

    // Other
    public FileStructure GetFileStructure(bool quickPlace)
    {
        fileStructure.closeOnSelect = quickPlace;

        if (quickPlace)
        {
            fileStructure.action = (string file) => { InstantiateModelSetup(file, cursor.GetCursorPosition(), true); };
        }
        else
        {
            fileStructure.action = (string file) => { InstantiateModelSetup(file); };
        }

        return fileStructure;
    }

    public void CacheModel(string path, ModelParent modelParent)
    {
        if (!cachedModels.ContainsKey(path))
        {
            cachedModels.Add(path, modelParent);
        }
    }

    public ModelParent GetCachedModel(string path)
    {
        if (cachedModels.ContainsKey(path))
        {
            return cachedModels[path];
        }

        return null;
    }
}
