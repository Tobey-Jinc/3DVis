using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vertex;

/// <summary>
/// Defines the data of an audio source
/// </summary>
[System.Serializable]
public struct Audio
{
    [SerializeField] public string id;
    [SerializeField] public string displayName;
    [SerializeField] public string category;
    [SerializeField] public AudioClip audioClip;
}

public class AudioLibrary : MonoBehaviour
{
    [SerializeField] private Audio[] audioClips;
    [SerializeField] private ModelCache modelCache;
    [SerializeField] private ObjectCursor cursor;

    private FileStructure fileStructure;

    void Start()
    {
        // Create the file structure
        Dictionary<string, List<string[]>> files = new();

        for (int i = 0; i < audioClips.Length; i++)
        {
            Audio audio = audioClips[i];

            string[] file = new string[] { audio.displayName, audio.displayName };

            FileSelection.AddFile(files, Data.allCategory, file);
            FileSelection.AddFile(files, audio.category, file);
        }

        fileStructure = new FileStructure("Select an Audio Clip", files);
    }

    /// <summary>
    /// Retreives from audio with the given ID
    /// </summary>
    /// <param name="id">The audio ID to search for</param>
    /// <returns>The found audio. Will return an empty Audio struct if the ID could not be found</returns>
    public Audio GetAudio(string id)
    {
        foreach (Audio audio in audioClips)
        {
            if (audio.id == id)
            {
                return audio;
            }
        }

        return new Audio();
    }

    /// <summary>
    /// Gets the file structure
    /// </summary>
    /// <param name="quickPlace">Whether or not the user is in quick place mode</param>
    /// <returns>The audio library file structure</returns>
    public FileStructure GetFileStructure(bool quickPlace)
    {
        fileStructure.closeOnSelect = quickPlace;

        // Change the action if in quick place mode
        if (quickPlace)
        {
            fileStructure.action = (string id) => { modelCache.InstantiateAudioObject(GetAudio(id), cursor.GetCursorPosition(), true); };
        }
        else
        {
            fileStructure.action = (string id) => { modelCache.InstantiateAudioObject(GetAudio(id)); };
        }

        return fileStructure;
    }
}
