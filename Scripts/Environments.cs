using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vertex;

/// <summary>
/// Defines the data of an environment
/// </summary>
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
        // Create the file structure
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

    /// <summary>
    /// Sets the environment to the given ID
    /// </summary>
    /// <param name="id">The ID of the environment to set to</param>
    public void SetEnvironment(string id)
    {
        // Destroy the current environment if necessary
        if (currentEnvironment != null)
        {
            Destroy(currentEnvironment.gameObject);
            currentEnvironment = null;
        }

        // Find and create the environemnt from the given ID
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

    /// <summary>
    /// Gets the environment file structure
    /// </summary>
    /// <returns>The file structure</returns>
    public FileStructure GetFileStructure()
    {
        return fileStructure;
    }
}
