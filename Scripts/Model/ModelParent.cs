using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GLTFast;
using Vertex;
using System.Threading.Tasks;

public class ModelParent : MonoBehaviour
{
    [Header("Selection")]
    [SerializeField] private Transform selectionAnchor;
    [SerializeField] private SpriteRenderer transformIcon;
    [SerializeField] private Sprite positionIcon;
    [SerializeField] private Color positionColor;
    [SerializeField] private Sprite rotationIcon;
    [SerializeField] private Color rotationColor;
    [SerializeField] private Sprite scaleIcon;
    [SerializeField] private Color scaleColor;

    private string path;

    private ModelCursor cursor;
    private Transform wand;
    private new Transform camera;

    public string Path { get => path; }

    private void Start()
    {
        cursor = ModelCursor.Instance;
        wand = WandTransform.Instance.Transform;
        camera = wand.parent;
    }

    public async Task Setup(string path)
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
        bool success = await gltf.Load(path, settings);

        if (success)
        {
            transform.name = path;
            await gltf.InstantiateMainSceneAsync(transform);

            ModelCache.Instance.CacheModel(path, this);
        }
        else
        {
            Debug.LogError("Loading glTF failed!");
        }
    }

    public void CachedSetup(string path)
    {
        this.path = path;

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
            selectionAnchor.gameObject.SetActive(true);
            selectionAnchor.LookAt(camera);

            switch (cursor.TransformMode)
            {
                case TransformMode.None:
                    transformIcon.color = Color.black;

                    break;

                case TransformMode.Position:
                    transformIcon.sprite = positionIcon;
                    transformIcon.color = positionColor;

                    Vector2 movementInput = new Vector2(getReal3D.Input.GetAxis(Inputs.leftStickY), getReal3D.Input.GetAxis(Inputs.leftStickX));
                    float upDownInput = getReal3D.Input.GetAxis(Inputs.rightStickY);

                    transform.Translate((wand.right * movementInput.y + wand.forward * movementInput.x + Vector3.up * upDownInput) * 5 * getReal3D.Cluster.deltaTime, Space.World);

                    break;

                case TransformMode.Rotation:
                    transformIcon.sprite = rotationIcon;
                    transformIcon.color = rotationColor;

                    float rotateX = getReal3D.Input.GetAxis(Inputs.leftStickY);
                    float rotateY = getReal3D.Input.GetAxis(Inputs.leftStickX);

                    transform.Rotate(new Vector3(0, -rotateY, 0) * 50 * getReal3D.Cluster.deltaTime, Space.World);

                    transform.RotateAround(transform.position, wand.right, rotateX * 50 * getReal3D.Cluster.deltaTime);

                    if (getReal3D.Input.GetButtonDown(Inputs.rightShoulder))
                    {
                        transform.rotation = Quaternion.identity;
                    }

                    break;

                case TransformMode.Scale:
                    transformIcon.sprite = scaleIcon;
                    transformIcon.color = scaleColor;

                    float scaleInput = getReal3D.Input.GetAxis(Inputs.leftStickY);

                    transform.localScale += Vector3.one * scaleInput * getReal3D.Cluster.deltaTime;

                    break;
            }
        }
        else
        {
            selectionAnchor.gameObject.SetActive(false);
        }
    }

    public void Select(Vector3 selectionPoint)
    {
        selectionAnchor.position = selectionPoint;
        cursor.SelectObject(transform);
    }
}
