using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SceneDescription
{
    public string sceneDisplayName;
    public string sceneCategory;

    public string environmentPresetID;

    public SDSky sky;

    public SDModel[] models;

    public SDText[] text;

    public SDLight[] lights;

    public SDAudio[] audio;
}

[System.Serializable]
public struct SDSky
{
    public string id;
    public bool libraryModel;

    public Vector3 position;
    public Quaternion rotation;
    public int skyboxIndex;
    public int colorIndex;
    public float intensity;
    public LightShadows shadows;
}

[System.Serializable]
public struct SDModel
{
    public string id;
    public bool libraryModel;

    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;
}

[System.Serializable]
public struct SDText
{
    public string text;

    public Vector3 position;
    public Quaternion rotation;
    public int colorIndex;
    public float fontSize;
    public float width;
    public TMPro.TextAlignmentOptions textAlignment;
}

[System.Serializable]
public struct SDLight
{
    public Vector3 position;
    public int colorIndex;
    public float range;
    public float intensity;
    public LightShadows shadows;
}

[System.Serializable]
public struct SDAudio
{
    public string id;

    public Vector3 position;
    public float volume;
    public float spatialBlend;
    public float minDistance;
    public float maxDistance;
}
