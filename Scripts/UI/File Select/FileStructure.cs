using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public struct FileStructure
{
    public string title;
    public string[][] files;
    public UnityAction<string> action;

    public int fileCount;

    public FileStructure(string title, string[][] files, UnityAction<string> action)
    {
        this.title = title;
        this.files = files;
        this.action = action;

        fileCount = files.GetLength(0);
    }
}
