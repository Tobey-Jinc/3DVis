using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class FileStructure
{
    public string title;
    public Dictionary<string, List<string[]>> files; // The key is the category
    public UnityAction<string> action;
    public bool closeOnSelect;
    public string[] categories;
    
    public FileStructure() { }

    /// <summary>
    /// Initialize the structure
    /// </summary>
    /// <param name="title">Structure name</param>
    /// <param name="files">The files</param>
    /// <param name="action">Selection action</param>
    public FileStructure(string title, Dictionary<string, List<string[]>> files, UnityAction<string> action)
    {
        this.title = title;
        this.action = action;

        SetFiles(files);

        closeOnSelect = false;
    }

    /// <summary>
    /// Initialize the structure without an action
    /// </summary>
    /// <param name="title">Structure name</param>
    /// <param name="files">The files</param>
    public FileStructure(string title, Dictionary<string, List<string[]>> files)
    {
        this.title = title;

        SetFiles(files);

        action = null;
        closeOnSelect = false;
    }

    /// <summary>
    /// Set the files of this structure
    /// </summary>
    /// <param name="files">The files to set to</param>
    public void SetFiles(Dictionary<string, List<string[]>> files)
    {
        this.files = files;

        categories = files.Keys.ToArray();
    }
}
