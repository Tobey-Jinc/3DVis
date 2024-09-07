using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;
using GLTFast;
using System;
using System.Threading;
using System.Threading.Tasks;

public class ModelExplorer : MonoBehaviour
{
    [SerializeField] private TMP_Text t_Path;
    [SerializeField] private GltfAsset model;

    private void Start()
    {
        Debug.Log(Directory.GetCurrentDirectory());
        string currentDir = Directory.GetCurrentDirectory() + "\\models\\";
        ClearWorkingDirectory(currentDir);
        StartCoroutine(WaitForAvailability());

    }

    private void ClearWorkingDirectory(string directory)
    {
        if (Directory.Exists(directory))
        {
            DirectoryInfo di = new DirectoryInfo(directory);

            foreach (FileInfo file in di.GetFiles())
            {
                try
                {
                    file.Delete();
                    Debug.Log("deleted file " + file.Name);
                }
                catch (Exception e)
                {
                    Debug.Log("file: it failed sad " + file.Name + " " + e);
                }
            }
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                try
                {
                    dir.Delete(true);
                    Debug.Log("deleted directory " + dir.Name);
                }
                catch (Exception e)
                {
                    Debug.Log("directory: it failed sad " + dir.Name + " " + e);
                }
            }
        }
        
    }

    private IEnumerator WaitForAvailability()
    {
        string currentDir = Directory.GetCurrentDirectory() + "\\models\\";
        Debug.Log(currentDir);
        //copy all models from shared network folder to currentdirectory/models

        Vector2Int fileCounts = Vector2Int.zero;

        yield return new WaitUntil(() =>
        {
            fileCounts = CopyFilesRecursively("\\\\CAVE-HEADNODE\\data\\3dvis", currentDir);

            return fileCounts.x != -1 && fileCounts.y != -1;
        });

        StartCoroutine(WaitForFile(currentDir, fileCounts.x, fileCounts.y));
    }

    private Vector2Int CopyFilesRecursively(string sourcePath, string targetPath)
    {
        Vector2Int files = Vector2Int.zero;

        try
        {
            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            {
                files.x++;
                Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
            }

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            {
                files.y++;
                File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
                Debug.Log(sourcePath);
                Debug.Log(newPath);
                Debug.Log(targetPath);

            }
            Debug.Log("complete");
        }
        catch (Exception e)
        {
            Debug.Log(e);

            files = new Vector2Int(-1, -1);
        }

        return files;
    }

    private IEnumerator WaitForFile(string folder, int folderCount, int fileCount)
    {
        yield return new WaitUntil(() =>
        {
            int folders = 0;
            int files = 0;
            try
            {
                //Now Create all of the directories
                foreach (string dirPath in Directory.GetDirectories(folder, "*", SearchOption.AllDirectories))
                {
                    folders++;
                }

                //Copy all the files & Replaces any files with the same name
                foreach (string newPath in Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories))
                {
                    files++;
                }
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
            Debug.Log((folderCount, fileCount, folders, files));
            return folderCount == folders && fileCount == files;
        });


        model.Load(folder + "/stylized_rock/scene.gltf");
    }
}
