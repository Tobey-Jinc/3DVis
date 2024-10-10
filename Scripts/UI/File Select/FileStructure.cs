using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class FileStructure
{
    public string title;
    public Dictionary<string, List<string[]>> files;
    public UnityAction<string> action;
    public bool closeOnSelect;
    public string[] categories;
    
    public FileStructure() { }

    public FileStructure(string title, Dictionary<string, List<string[]>> files, UnityAction<string> action)
    {
        this.title = title;
        this.action = action;

        SetFiles(files);

        closeOnSelect = false;
    }

    public FileStructure(string title, Dictionary<string, List<string[]>> files)
    {
        this.title = title;

        SetFiles(files);

        action = null;
        closeOnSelect = false;
    }

    public void SetFiles(Dictionary<string, List<string[]>> files)
    {
        this.files = files;

        categories = files.Keys.ToArray();
    }
}
