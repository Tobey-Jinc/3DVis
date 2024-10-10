using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vertex;

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

    public FileStructure GetFileStructure(bool quickPlace)
    {
        fileStructure.closeOnSelect = quickPlace;

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
