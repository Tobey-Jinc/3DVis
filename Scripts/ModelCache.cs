using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Linq;
using System.Threading.Tasks;
using Vertex;

public class ModelCache : MonoBehaviour
{
    public static ModelCache Instance;
    public static bool Loaded = false;

    [SerializeField] private Transform wand;
    [SerializeField] private ModelParent modelParentPrefab;

    [Header("Loading UI")]
    [SerializeField] private Canvas loadingScreen;

    private FileStructure fileStructure;

    private Dictionary<string, ModelParent> cachedModels = new();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        fileStructure = new FileStructure();
        fileStructure.title = "Select a model";
        fileStructure.action = (string file) => { InstantiateModel(file); };

#if !UNITY_EDITOR
        Debug.Log(Application.persistentDataPath);
        string currentDir = Application.persistentDataPath + "\\models\\";
        ClearWorkingDirectory(currentDir);
        StartCoroutine(WaitForAvailability());
#endif

#if UNITY_EDITOR
        GenerateFileStructure(Application.persistentDataPath + "\\models");
#endif
    }

    private void ClearWorkingDirectory(string directory)
    {
        if (Directory.Exists(directory))
        {
            DirectoryInfo di = new DirectoryInfo(directory);

            foreach (FileInfo file in di.GetFiles())
            {
                try
                {
                    file.Delete();
                    Debug.Log("deleted file " + file.Name);
                }
                catch (Exception e)
                {
                    Debug.Log("file: it failed sad " + file.Name + " " + e);
                }
            }
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                try
                {
                    dir.Delete(true);
                    Debug.Log("deleted directory " + dir.Name);
                }
                catch (Exception e)
                {
                    Debug.Log("directory: it failed sad " + dir.Name + " " + e);
                }
            }
        }
        
    }

    private IEnumerator WaitForAvailability()
    {
        string currentDir = Application.persistentDataPath + "\\models\\";
        Debug.Log(currentDir);
        //copy all models from shared network folder to currentdirectory/models

        Vector2Int fileCounts = Vector2Int.zero;

        yield return new WaitUntil(() =>
        {
            fileCounts = CopyFilesRecursively("\\\\CAVE-HEADNODE\\data\\3dvis\\models", currentDir);

            return fileCounts.x != -1 && fileCounts.y != -1;
        });

        StartCoroutine(WaitForFile(currentDir, fileCounts.x, fileCounts.y));
    }

    private Vector2Int CopyFilesRecursively(string sourcePath, string targetPath)
    {
        Vector2Int files = Vector2Int.zero;

        try
        {
            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            {
                files.x++;
                Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
            }

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            {
                files.y++;
                File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
                Debug.Log(sourcePath);
                Debug.Log(newPath);
                Debug.Log(targetPath);

            }
            Debug.Log("complete");
        }
        catch (Exception e)
        {
            Debug.Log(e);

            files = new Vector2Int(-1, -1);
        }

        return files;
    }

    private IEnumerator WaitForFile(string folder, int folderCount, int fileCount)
    {
        yield return new WaitUntil(() =>
        {
            int folders = 0;
            int files = 0;
            try
            {
                //Now Create all of the directories
                foreach (string dirPath in Directory.GetDirectories(folder, "*", SearchOption.AllDirectories))
                {
                    folders++;
                }

                //Copy all the files & Replaces any files with the same name
                foreach (string newPath in Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories))
                {
                    files++;
                }
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
            Debug.Log((folderCount, fileCount, folders, files));
            return folderCount == folders && fileCount == files;
        });

        GenerateFileStructure(folder);

        //model.Load(folder + "/stylized_rock/scene.gltf");
    }

    private void GenerateFileStructure(string folder)
    {
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

    private async Task<Transform> InstantiateModel(string modelPath)
    {
        ModelParent cachedModel = GetCachedModel(modelPath);
        if (cachedModel == null)
        {
            Debug.Log("Model has not been cached. Importing model and trying again...");

            ModelParent importedModel = Instantiate(modelParentPrefab, transform);

            await importedModel.Setup(modelPath);

            importedModel.gameObject.SetActive(false);
            CacheModel(modelPath, importedModel);

            return await InstantiateModel(modelPath);
        }
        else
        {
            ModelParent model = Instantiate(cachedModel, SceneDescriptionManager.Scene);
            model.CachedSetup(modelPath);

            if (Physics.Raycast(wand.position, wand.forward, out RaycastHit hit, 10, 1, QueryTriggerInteraction.Ignore))
            {
                model.transform.position = hit.point;
            }
            else
            {
                model.transform.position = wand.position + (wand.forward * 10);
            }

            model.gameObject.SetActive(true);

            return model.transform;
        }
    }

    public async Task InstantiateModelFromSceneDescription(SDModel sdModel)
    {
        Transform model = await InstantiateModel(sdModel.id);

        model.position = sdModel.position;
        model.rotation = sdModel.rotation;
        model.localScale = sdModel.scale;
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
