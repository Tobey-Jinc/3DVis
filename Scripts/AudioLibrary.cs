using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Audio
{
    [SerializeField] public string id;
    [SerializeField] public string displayName;
    [SerializeField] public AudioClip audioClip;
}

public class AudioLibrary : MonoBehaviour
{
    [SerializeField] private Audio[] audioClips;
    [SerializeField] private ModelCache modelCache;

    private FileStructure fileStructure;

    void Start()
    {
        string[][] files = new string[audioClips.Length][];

        for (int i = 0; i < audioClips.Length; i++)
        {
            Audio audio = audioClips[i];

            files[i] = new string[] { audio.displayName, audio.displayName };
        }

        fileStructure = new FileStructure("Select an Audio Clip", files, (string id) => { modelCache.InstantiateAudioObject(GetAudio(id)); });
    }

    private Audio GetAudio(string id)
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

    public FileStructure GetFileStructure()
    {
        return fileStructure;
    }
}
