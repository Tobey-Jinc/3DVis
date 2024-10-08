using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GLTFast;
using Vertex;
using System.Threading.Tasks;

public class ModelParent : MonoBehaviour
{
    private string folderName;

    [SerializeField] private Transform selectionAnchor;
    [SerializeField] private bool libraryModel;

    private ObjectCursor cursor;

    private TransformMode[] transformModes;

    public string FolderName { get => folderName; set => folderName = value; }
    public bool LibraryModel { get => libraryModel; }

    private void Start()
    {
        cursor = ObjectCursor.Instance;

        transformModes = new[] { TransformMode.Position, TransformMode.Rotation, TransformMode.Scale };
    }

    public async Task Setup(string folder)
    {
        GltfImport gltf = new GltfImport();

        // Create a settings object and configure it accordingly
        ImportSettings settings = new ImportSettings
        {
            GenerateMipMaps = true,
            AnisotropicFilterLevel = 3,
            NodeNameMethod = NameImportMethod.OriginalUnique
        };
        // Load the glTF and pass along the settings
        bool success = await gltf.Load(Paths.GetModelFolder() + folder + "\\scene.gltf", settings);

        if (success)
        {
            transform.name = folder;
            await gltf.InstantiateMainSceneAsync(transform);

            ModelCache.Instance.CacheModel(folder, this);
        }
        else
        {
            Debug.LogError("Loading glTF failed!");
        }
    }

    public void CachedSetup(string folderName)
    {
        this.folderName = folderName;

        PrepareChildren(transform);
    }

    private void PrepareChildren(Transform parent)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);

            if (child.GetComponent<MeshRenderer>() != null)
            {
                ModelChild modelChild = child.gameObject.AddComponent<ModelChild>();
                modelChild.Parent = this;

                child.gameObject.layer = 6;

                child.gameObject.AddComponent<MeshCollider>();
            }

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
            switch (cursor.CursorTransformMode)
            {
                case TransformMode.Position:
                    cursor.Position(transform);

                    break;

                case TransformMode.Rotation:
                    cursor.Rotate(transform);

                    break;

                case TransformMode.Scale:
                    float scaleInput = getReal3D.Input.GetAxis(Inputs.leftStickY);

                    transform.localScale += Vector3.one * scaleInput * getReal3D.Cluster.deltaTime;

                    break;
            }
        }
    }

    public void Select(Vector3 selectionPoint)
    {
        selectionAnchor.position = selectionPoint;
        cursor.SelectObject(transform, transformModes, selectionAnchor);
    }
}
