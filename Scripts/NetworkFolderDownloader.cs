using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.IO;
using System;
using System.Linq;
using System.Threading.Tasks;
using Vertex;

public class NetworkFolderDownloader : MonoBehaviour
{
    string sourcePath;
    string targetPath;
    private UnityAction action;

    public void Download(string folder, UnityAction action)
    {
        sourcePath = $"\\\\CAVE-HEADNODE\\data\\3dvis\\{folder}";
        targetPath = Application.persistentDataPath + $"\\{folder}\\";

        this.action = action;

#if !UNITY_EDITOR
        ClearWorkingDirectory();
        StartCoroutine(WaitForAvailability());
#endif

#if UNITY_EDITOR
        action.Invoke();
#endif
    }

    private void ClearWorkingDirectory()
    {
        if (Directory.Exists(targetPath))
        {
            DirectoryInfo directory = new DirectoryInfo(targetPath);

            foreach (FileInfo file in directory.GetFiles())
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
            foreach (DirectoryInfo dir in directory.GetDirectories())
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
        Vector2Int fileCounts = Vector2Int.zero;

        yield return new WaitUntil(() =>
        {
            fileCounts = CopyFilesRecursively();

            return fileCounts.x != -1 && fileCounts.y != -1;
        });

        StartCoroutine(WaitForFile(targetPath, fileCounts.x, fileCounts.y));
    }

    private Vector2Int CopyFilesRecursively()
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
            Debug.Log((folderCount, fileCount, folders, files, folder));
            return folderCount == folders && fileCount == files;
        });

        action?.Invoke();
    }
}
