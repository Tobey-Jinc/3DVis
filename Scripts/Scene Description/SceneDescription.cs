using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SceneDescription
{
    public string environmentPresetID;

    public SDModel[] models;
}

[System.Serializable]
public struct SDModel
{
    public string id;

    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;
}
