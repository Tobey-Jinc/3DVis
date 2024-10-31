using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.IO;
using System;
using System.Linq;
using System.Threading.Tasks;
using Vertex;

/// <summary>
/// Downloads an entire folder from the network folder
/// </summary>
public class NetworkFolderDownloader : MonoBehaviour
{
    string networkFolder = "\\\\CAVE-HEADNODE\\data\\3dvis";
    string sourcePath;
    string targetPath;
    private UnityAction action;

    /// <summary>
    /// Starts the download process
    /// </summary>
    /// <param name="folder">The folder name to download from, and download to</param>
    /// <param name="action">The action to occur after the entire download process has completed</param>
    public void Download(string folder, UnityAction action)
    {
        sourcePath = $"\\\\CAVE-HEADNODE\\data\\3dvis\\{folder}";
        targetPath = Application.persistentDataPath + $"\\{folder}\\";

        this.action = action;

        // Only do the download process in builds
#if !UNITY_EDITOR
        CreateNetworkFolders();
        ClearWorkingDirectory();
        StartCoroutine(WaitForAvailability());
#endif

#if UNITY_EDITOR
        action.Invoke();
#endif
    }

    /// <summary>
    /// Creates the folders if they do not exist (only run by the master node)
    /// </summary>
    private void CreateNetworkFolders()
    {
        if (getReal3D.Cluster.isMaster)
        {
            if (!Directory.Exists(networkFolder))
            {
                Directory.CreateDirectory(networkFolder);
            }

            if (!Directory.Exists(sourcePath))
            {
                Directory.CreateDirectory(sourcePath);
            }
        }
    }

    /// <summary>
    /// Clears all files and folders in the working directory. This prevent conflicts
    /// </summary>
    private void ClearWorkingDirectory()
    {
        if (Directory.Exists(targetPath))
        {
            DirectoryInfo directory = new DirectoryInfo(targetPath);

            // Delete all files
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

            // Delete all folders
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

    /// <summary>
    /// Waits for files to be copied. This can fail multiple times as the other nodes are trying to also download the folder. Only one can do it at a time.
    /// </summary>
    /// <returns></returns>
    private IEnumerator WaitForAvailability()
    {
        // Folder and file counts are stored to check if everything was successfully copied
        Vector2Int fileCounts = Vector2Int.zero;

        yield return new WaitUntil(() =>
        {
            fileCounts = CopyFilesRecursively();

            // -1 is returned when the copy fails
            return fileCounts.x != -1 && fileCounts.y != -1;
        });


        StartCoroutine(WaitForFile(targetPath, fileCounts.x, fileCounts.y));
    }

    /// <summary>
    /// Copies all files and folders
    /// </summary>
    /// <returns>An Vector2Int containing the folder count as X and the file count as Y</returns>
    private Vector2Int CopyFilesRecursively()
    {
        Vector2Int files = Vector2Int.zero;

        try
        {
            // Copy all the folders
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            {
                files.x++;
                Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
            }

            // Copy all the files
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
        catch (Exception e) // Failure most commonly occurs due to another node being in the process of copying
        {
            Debug.Log(e);

            files = new Vector2Int(-1, -1);
        }

        return files;
    }

    /// <summary>
    /// Waits for all folders and files to be copied
    /// </summary>
    /// <param name="folder">The folder to check in</param>
    /// <param name="folderCount">The number of folders expected</param>
    /// <param name="fileCount">The number of files expected</param>
    /// <returns></returns>
    private IEnumerator WaitForFile(string folder, int folderCount, int fileCount)
    {
        yield return new WaitUntil(() =>
        {
            int folders = 0;
            int files = 0;
            try
            {
                // Count folders
                foreach (string dirPath in Directory.GetDirectories(folder, "*", SearchOption.AllDirectories))
                {
                    folders++;
                }

                // Count files
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

            // Returns true if all folders and files are accounted for
            return folderCount == folders && fileCount == files;
        });

        // Finish up by running the action
        action?.Invoke();
    }
}
