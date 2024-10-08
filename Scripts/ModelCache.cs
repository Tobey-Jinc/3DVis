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
    [SerializeField] private Transform wand;
    [SerializeField] private Viewpoint viewpoint;

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

    private void GenerateFileStructure()
    {
        string folder = Application.persistentDataPath + "\\models";
        string[] folders = Directory.GetDirectories(Application.persistentDataPath + "\\models").Select(Path.GetFileName).ToArray();
        string[][] files = new string[folders.Length][];

        for (int i = 0; i < folders.Length; i++)
        {
            try
            {
                Debug.Log(folders[i]);
                string metaDataText = File.ReadAllText(Paths.GetModelFolder() + folders[i] + "\\metadata.json");
                ModelMetaData metaData = JsonUtility.FromJson<ModelMetaData>(metaDataText);

                Debug.Log(metaData.originalModelName);
                Debug.Log(folders[i] + "\\scene.gltf");

                files[i] = new string[] { metaData.modelDisplayName, folders[i] };
            }
            catch (Exception e)
            {
                Debug.Log($"An error occurred whilst reading the model... \n{e}");
            }
        }
        fileStructure.SetFiles(files);
        Debug.Log(files.GetLength(0));

        Loaded = true;

        loadingScreen.enabled = false;
    }

    private Vector3 GetSpawnPosition()
    {
        if (Physics.Raycast(wand.position, wand.forward, out RaycastHit hit, 10, 1, QueryTriggerInteraction.Ignore))
        {
            return hit.point;
        }
        
        return wand.position + (wand.forward * 10);
    }

    private void InstantiateModelSetup(string modelPath)
    {
        if (getReal3D.Cluster.isMaster)
        {
            Vector3 spawnPosition = GetSpawnPosition();

            CallRpc(instantiateModelSyncedMethod, modelPath, spawnPosition);
            InstantiateModel(modelPath, spawnPosition);
        }
    }

    [getReal3D.RPC]
    private void InstantiateModelSynced(string modelPath, Vector3 spawnPosition)
    {
        if (!getReal3D.Cluster.isMaster)
        {
            InstantiateModel(modelPath, spawnPosition);
        }
    }

    private async Task<Transform> InstantiateModel(string modelPath, Vector3 spawnPosition)
    {
        ModelParent cachedModel = GetCachedModel(modelPath);
        if (cachedModel == null)
        {
            Debug.Log("Model has not been cached. Importing model and trying again...");

            ModelParent importedModel = Instantiate(modelParentPrefab, transform);

            await importedModel.Setup(modelPath);

            importedModel.gameObject.SetActive(false);
            CacheModel(modelPath, importedModel);

            return await InstantiateModel(modelPath, spawnPosition);
        }
        else
        {
            ModelParent model = Instantiate(cachedModel, SceneDescriptionManager.Scene);
            model.CachedSetup(modelPath);

            model.transform.position = spawnPosition;

            model.gameObject.SetActive(true);

            viewpoint.SyncTransformWithHeadnode();

            return model.transform;
        }
    }

    public async Task InstantiateModelFromSceneDescription(SDModel sdModel)
    {
        Transform model = await InstantiateModel(sdModel.id, sdModel.position);

        model.rotation = sdModel.rotation;
        model.localScale = sdModel.scale;
    }

    public void InstantiateModelFromLibrary(Model model)
    {
        ModelParent modelParent = Instantiate(model.prefab, SceneDescriptionManager.Scene);
        modelParent.FolderName = model.id;

        modelParent.transform.position = GetSpawnPosition();
    }

    public void InstantiateTextObject()
    {
        TextObject textObject = Instantiate(textObjectPrefab, SceneDescriptionManager.Scene);

        textObject.transform.position = GetSpawnPosition();
        textObject.transform.LookAt(wand.position);
        textObject.transform.rotation = Quaternion.Euler(0, -textObject.transform.eulerAngles.y, 0);
    }

    public void InstantiateLightObject()
    {
        LightObject lightObject = Instantiate(lightObjectPrefab, SceneDescriptionManager.Scene);

        lightObject.transform.position = GetSpawnPosition();
    }

    public void InstantiateAudioObject(Audio audio)
    {
        AudioObject audioObject = Instantiate(audioObjectPrefab, SceneDescriptionManager.Scene);

        audioObject.transform.position = GetSpawnPosition();

        audioObject.Setup(audio);
    }

    public FileStructure GetFileStructure()
    {
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
