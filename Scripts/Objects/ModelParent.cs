using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GLTFast;
using Vertex;
using System.Threading.Tasks;

public class ModelParent : MonoBehaviour
{
    [SerializeField] private Transform selectionAnchor;
    [SerializeField] public bool libraryModel;
    [SerializeField] private GameObject errorModel;

    private ObjectCursor cursor;

    private TransformModeAndControls[] transformModes;

    [HideInInspector][SerializeField] public string folderName;

    public bool LibraryModel { get => libraryModel; }

    private void Start()
    {
        cursor = ObjectCursor.Instance;

        // Define the transform modes
        transformModes = new TransformModeAndControls[] { 
            new(TransformMode.Position, $"{Data.switchControl}Move <sprite=6>    Up / Down <sprite=9>"),
            new(TransformMode.Rotation, $"{Data.switchControl}Rotate <sprite=6>    Reset <sprite=5>"),
            new(TransformMode.Scale, $"{Data.switchControl}Scale <sprite=6>")
        };
    }

    /// <summary>
    /// Sets up a non-cached model
    /// </summary>
    /// <param name="folder">The folder containing the model</param>
    /// <returns></returns>
    public async Task Setup(string folder)
    {
        GltfImport gltf = new GltfImport();

        // Define import settings
        ImportSettings settings = new ImportSettings
        {
            GenerateMipMaps = true,
            AnisotropicFilterLevel = 3,
            NodeNameMethod = NameImportMethod.OriginalUnique
        };

        // Load the model
        bool success = await gltf.Load(Paths.GetModelFolder() + folder + "\\scene.gltf", settings);

        if (success)
        {
            // Instantiate and cache the loaded model

            transform.name = folder;
            await gltf.InstantiateMainSceneAsync(transform);

            ModelCache.Instance.CacheModel(folder, this);
        }
        else
        {
            Debug.LogError("Loading glTF failed!");

            errorModel.SetActive(true);
        }
    }

    /// <summary>
    /// Sets up cached model
    /// </summary>
    /// <param name="folderName">The cached model folder</param>
    public void CachedSetup(string folderName)
    {
        this.folderName = folderName;

        PrepareChildren(transform);
    }

    /// <summary>
    /// Prepares the children of the model
    /// </summary>
    /// <param name="parent">The immediate parent of the child</param>
    private void PrepareChildren(Transform parent)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);

            // Add MeshRenderer, ModelChild, and MeshCollider components
            if (child.GetComponent<MeshRenderer>() != null)
            {
                ModelChild modelChild = child.gameObject.AddComponent<ModelChild>();
                modelChild.Parent = this;

                // Selection layer
                child.gameObject.layer = 6;

                child.gameObject.AddComponent<MeshCollider>();
            }

            // Recursively prepare children
            if (child.childCount > 0)
            {
                PrepareChildren(child);
            }
        }
    }

    private void Update()
    {
        if (cursor.SelectedObject == transform)
        {
            // Handle transform modes
            switch (cursor.CursorTransformMode)
            {
                case TransformMode.Position:
                    cursor.Position(transform);

                    break;

                case TransformMode.Rotation:
                    cursor.Rotate(transform, Quaternion.identity);

                    break;

                case TransformMode.Scale:
                    // Get input
                    float scaleInput = getReal3D.Input.GetAxis(Inputs.leftStickY);

                    // Determine speed
                    float scaleSpeed = 2 * CurrentOptions.options.scaleSpeed;

                    // Apply scaling
                    transform.localScale += Vector3.one * scaleSpeed * scaleInput * getReal3D.Cluster.deltaTime;

                    break;
            }
        }
    }

    /// <summary>
    /// Selects the model
    /// </summary>
    /// <param name="selectionPoint">The point to place the transform icon</param>
    public void Select(Vector3 selectionPoint)
    {
        selectionAnchor.position = selectionPoint;
        cursor.SelectObject(transform, transformModes, selectionAnchor);
    }
}
