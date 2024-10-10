using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vertex;

[System.Serializable]
public struct EnvironmentPreset
{
    [SerializeField] public string id;
    [SerializeField] public string displayName;
    [SerializeField] public string category;
    [SerializeField] public Transform prefab;
}

public class Environments : MonoBehaviour
{
    [SerializeField] private EnvironmentPreset[] environments;

    private FileStructure fileStructure;

    private Transform currentEnvironment = null;
    private string currentEnvironmentID;

    public string CurrentEnvironmentID { get => currentEnvironmentID; }

    private void Start()
    {
        Dictionary<string, List<string[]>> files = new();

        for (int i = 0; i < environments.Length; i++)
        {
            EnvironmentPreset environment = environments[i];

            string[] file = new string[] { environment.displayName, environment.displayName };

            FileSelection.AddFile(files, Data.allCategory, file);
            FileSelection.AddFile(files, environment.category, file);
        }

        fileStructure = new FileStructure("Select an Environment", files, (string id) => { SetEnvironment(id); });

        SetEnvironment(environments[0].id);
    }

    public void SetEnvironment(string id)
    {
        if (currentEnvironment != null)
        {
            Destroy(currentEnvironment.gameObject);
            currentEnvironment = null;
        }

        foreach (EnvironmentPreset environment in environments)
        {
            if (environment.id == id)
            {
                currentEnvironment = Instantiate(environment.prefab);
                currentEnvironmentID = environment.id;
                break;
            }
        }
    }

    public FileStructure GetFileStructure()
    {
        return fileStructure;
    }
}
