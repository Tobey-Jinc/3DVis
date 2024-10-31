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

    private GameObject copiedObject;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // Create file structure
        fileStructure = new FileStructure();
        fileStructure.title = "Select a model";
        fileStructure.action = (string file) => { InstantiateModelSetup(file); };

        // Download models
        networkFolderDownloader.Download("models", () => { GenerateFileStructure(); });
    }

    private void Update()
    {
        // Handle object pasting
        if (cursor.Active && getReal3D.Input.GetButton(Inputs.leftShoulder) && copiedObject != null)
        {
            copyCursor.position = copiedObject.transform.position;
            copyCursor.LookAt(wand);

            copyCursor.gameObject.SetActive(true);
        }
        else
        {
            copyCursor.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Generates the model file structure, based on the downloaded models folder, and the meta data
    /// </summary>
    private void GenerateFileStructure()
    {
        string folder = Application.persistentDataPath + "\\models";
        string[] folders = Directory.GetDirectories(Application.persistentDataPath + "\\models").Select(Path.GetFileName).ToArray();
        Dictionary<string, List<string[]>> files = new();

        for (int i = 0; i < folders.Length; i++)
        {
            try
            {
                // Get the meta data
                string metaDataText = File.ReadAllText(Paths.GetModelFolder() + folders[i] + "\\metadata.json");
                ModelMetaData metaData = JsonUtility.FromJson<ModelMetaData>(metaDataText);

                // Get the display name and category
                string modelDisplayName = metaData.modelDisplayName;
                string modelCategory = metaData.modelCategory;

                // Create the file
                string[] file = new string[] { modelDisplayName, folders[i] };

                // Define the categories
                FileSelection.AddFile(files, Data.allCategory, file);
                FileSelection.AddFile(files, modelCategory, file);
            }
            catch (Exception e)
            {
                Debug.Log($"An error occurred whilst reading the model... \n{e}");
            }
        }

        // Apply the files
        fileStructure.SetFiles(files);

        Loaded = true;

        loadingScreen.enabled = false;

        // Load temp scene if necessary (used after app reload)
        sceneDescriptionManager.LoadTempScene();
    }

    /// <summary>
    /// Gets the spawn position for a model
    /// </summary>
    /// <param name="position">An options position to force</param>
    /// <returns>The spawn position</returns>
    private Vector3 GetSpawnPosition(Vector3? position = null)
    {
        if (position == null)
        {
            // Find the point in front of the wand. Will spawn in front of geometry if a collider is found
            if (Physics.Raycast(wand.position, wand.forward, out RaycastHit hit, 10, 1, QueryTriggerInteraction.Ignore))
            {
                return hit.point;
            }

            // No collider was found so spawn 10 units in front of wand
            return wand.position + (wand.forward * 10);
        }

        return (Vector3)position;
    }

    /// <summary>
    /// Instantiate the model from the master node
    /// </summary>
    /// <param name="modelPath">Path of model to instantiate</param>
    /// <param name="position">Position to spawn at</param>
    /// <param name="copy">Whether or not the model should be copied</param>
    private void InstantiateModelSetup(string modelPath, Vector3? position = null, bool copy = false)
    {
        if (getReal3D.Cluster.isMaster)
        {
            Vector3 spawnPosition = GetSpawnPosition();

            // Instantiate on the folder on child nodes
            CallRpc(instantiateModelSyncedMethod, modelPath, spawnPosition, copy);

            // Instantiate the model
            InstantiateModel(modelPath, spawnPosition, copy);
        }
    }

    /// <summary>
    /// Instantiates a model at the same position as the master node
    /// </summary>
    /// <param name="modelPath">Path of model to instantiate</param>
    /// <param name="spawnPosition">Position to spawn at</param>
    /// <param name="copy">Whether or not the model should be copied</param>
    [getReal3D.RPC]
    private void InstantiateModelSynced(string modelPath, Vector3 spawnPosition, bool copy = false)
    {
        if (!getReal3D.Cluster.isMaster)
        {
            InstantiateModel(modelPath, spawnPosition, copy);
        }
    }

    /// <summary>
    /// Instantiates a model
    /// </summary>
    /// <param name="modelPath">Path of model to instantiate</param>
    /// <param name="spawnPosition">Position to spawn at</param>
    /// <param name="copy">Whether or not the model should be copied</param>
    /// <returns>The instantiated model</returns>
    private async Task<Transform> InstantiateModel(string modelPath, Vector3 spawnPosition, bool copy = false)
    {
        // Check if the model has been cached
        ModelParent cachedModel = GetCachedModel(modelPath);

        if (cachedModel == null) // Has not be cached, so create a new model
        {
            Debug.Log("Model has not been cached. Importing model and trying again...");

            ModelParent importedModel = Instantiate(modelParentPrefab, transform);

            // Setup the model
            await importedModel.Setup(modelPath);

            // Deactive this model, as it will be used as the cached variant
            importedModel.gameObject.SetActive(false);

            // Cache it
            CacheModel(modelPath, importedModel);

            // Try again, this time the cached version will be instantiated
            return await InstantiateModel(modelPath, spawnPosition, copy);
        }
        else // The model has been cached, so spawn that instead
        {
            // Instantiate and setup the cached model
            ModelParent model = Instantiate(cachedModel, SceneDescriptionManager.Scene);
            model.CachedSetup(modelPath);

            model.transform.position = spawnPosition;

            model.gameObject.SetActive(true);

            // Sync all nodes with the master node
            viewpoint.SyncTransformWithMasterNode();

            if (copy)
            {
                copiedObject = model.gameObject;
            }

            return model.transform;
        }
    }

    /// <summary>
    /// Instantiates a model from the model library
    /// </summary>
    /// <param name="model">The model to instantiate</param>
    /// <param name="position">Position to spawn at</param>
    /// <param name="copy">Whether or not the model should be copied</param>
    public void InstantiateModelFromLibrary(Model model, Vector3? position = null, bool copy = false)
    {
        ModelParent modelParent = Instantiate(model.prefab, SceneDescriptionManager.Scene);
        modelParent.folderName = model.id;

        modelParent.transform.position = GetSpawnPosition(position);

        if (copy)
        {
            copiedObject = modelParent.gameObject;
        }
    }

    /// <summary>
    /// Creates a text object
    /// </summary>
    /// <param name="position">Spawn position</param>
    /// <param name="copy">Whether or not the model should be copied</param>
    public void InstantiateTextObject(Vector3? position = null, bool copy = false)
    {
        TextObject textObject = Instantiate(textObjectPrefab, SceneDescriptionManager.Scene);

        // Make text look at camera
        textObject.transform.position = GetSpawnPosition(position);
        textObject.transform.LookAt(wand.position);

        textObject.transform.rotation = Quaternion.Euler(0, textObject.transform.eulerAngles.y, 0);

        if (copy)
        {
            copiedObject = textObject.gameObject;
        }
    }

    /// <summary>
    /// Creates a light object
    /// </summary>
    /// <param name="position">Spawn position</param>
    /// <param name="copy">Whether or not the model should be copied</param>
    public void InstantiateLightObject(Vector3? position = null, bool copy = false)
    {
        LightObject lightObject = Instantiate(lightObjectPrefab, SceneDescriptionManager.Scene);

        lightObject.transform.position = GetSpawnPosition(position);

        if (copy)
        {
            copiedObject = lightObject.gameObject;
        }
    }

    /// <summary>
    /// Creates an audio object
    /// </summary>
    /// <param name="audio">The Audio struct to derive audio data from</param>
    /// <param name="position">Spawn position</param>
    /// <param name="copy">Whether or not the model should be copied</param>
    public void InstantiateAudioObject(Audio audio, Vector3? position = null, bool copy = false)
    {
        AudioObject audioObject = Instantiate(audioObjectPrefab, SceneDescriptionManager.Scene);

        audioObject.transform.position = GetSpawnPosition(position);

        audioObject.Setup(audio);

        if (copy)
        {
            copiedObject = audioObject.gameObject;
        }
    }

    /// <summary>
    /// Copies the given GameObject
    /// </summary>
    /// <param name="copiedObject">The Gameobject to copy</param>
    public void Copy(GameObject copiedObject)
    {
        this.copiedObject = copiedObject;
    }

    /// <summary>
    /// Pastes the copied object at the given position
    /// </summary>
    /// <param name="position">Paste position</param>
    public void Paste(Vector3 position)
    {
        if (copiedObject != null)
        {
            GameObject pastedObject = Instantiate(copiedObject, SceneDescriptionManager.Scene);
            pastedObject.transform.position = position;
        }
    }

    // Scene Description Loading

    /// <summary>
    /// Loads a model from a Scene Description
    /// </summary>
    /// <param name="sdModel">The model data</param>
    /// <param name="libraryModel">Whether or not the model is a library model</param>
    /// <returns></returns>
    public async Task InstantiateModelFromSceneDescription(SDModel sdModel, bool libraryModel)
    {
        Transform model;

        if (!libraryModel)
        {
            // Instaniate from folder
            model = await InstantiateModel(sdModel.id, sdModel.position);
        }
        else
        {
            // Instantiate from library
            Model lModel = modelLibrary.GetModel(sdModel.id);
            ModelParent modelParent = Instantiate(lModel.prefab, SceneDescriptionManager.Scene);
            modelParent.folderName = lModel.id;

            model = modelParent.transform;
            model.position = sdModel.position;
        }

        model.rotation = sdModel.rotation;
        model.localScale = sdModel.scale;
    }

    /// <summary>
    /// Loads a text object from a Scene Description
    /// </summary>
    /// <param name="sdText">The text data</param>
    public void InstantiateTextFromSceneDescription(SDText sdText)
    {
        TextObject textObject = Instantiate(textObjectPrefab, SceneDescriptionManager.Scene);

        textObject.Setup(sdText);   
    }

    /// <summary>
    /// Loads a light object from a Scene Description
    /// </summary>
    /// <param name="sdLight">The lights data</param>
    public void InstantiateLightFromSceneDescription(SDLight sdLight)
    {
        LightObject lightObject = Instantiate(lightObjectPrefab, SceneDescriptionManager.Scene);

        lightObject.Setup(sdLight);
    }

    /// <summary>
    /// Load an audio object from a Scene Description
    /// </summary>
    /// <param name="sdAudio">The audio's data</param>
    public void InstantiateAudioFromSceneDescription(SDAudio sdAudio)
    {
        AudioObject audioObject = Instantiate(audioObjectPrefab, SceneDescriptionManager.Scene);

        audioObject.Setup(audioLibrary.GetAudio(sdAudio.id), sdAudio);
    }

    /// <summary>
    /// Gets the file structure
    /// </summary>
    /// <param name="quickPlace">Whether or not the user is in quick place mode</param>
    /// <returns>The model file structure</returns>
    public FileStructure GetFileStructure(bool quickPlace)
    {
        fileStructure.closeOnSelect = quickPlace;

        // Change action if in quick place mode
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

    /// <summary>
    /// Caches the given model
    /// </summary>
    /// <param name="path">The path of cached model</param>
    /// <param name="modelParent">The model to cache</param>
    public void CacheModel(string path, ModelParent modelParent)
    {
        if (!cachedModels.ContainsKey(path))
        {
            cachedModels.Add(path, modelParent);
        }
    }

    /// <summary>
    /// Gets a model from the cache
    /// </summary>
    /// <param name="path">The model to get</param>
    /// <returns>The cached model. Null if the model hasn't been cached</returns>
    public ModelParent GetCachedModel(string path)
    {
        if (cachedModels.ContainsKey(path))
        {
            return cachedModels[path];
        }

        return null; // Return null if the model hasn't been cached
    }
}
