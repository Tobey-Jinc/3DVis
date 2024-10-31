using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Vertex;

public class FileView : MonoBehaviour
{
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private Image background;
    [SerializeField] private TMP_Text t_File;
    [SerializeField] private float selectedScale;
    [SerializeField] private float selectedScaleSpeed;

    private FileSelection fileSelection;
    private int index;

    /// <summary>
    /// Sets up the view. Used for a pooled view
    /// </summary>
    /// <param name="index">The views index in the list</param>
    /// <param name="fileName">The name of the file</param>
    public void Setup(int index, string fileName)
    {
        gameObject.SetActive(true);

        this.index = index;
        t_File.SetText(fileName);
    }

    /// <summary>
    /// Sets up the view. Used for a newly created view
    /// </summary>
    /// <param name="fileSelection">Stored reference to the File Selection menu</param>
    /// <param name="index">The views index in the list</param>
    /// <param name="fileName">The name of the file</param>
    public void Setup(FileSelection fileSelection, int index, string fileName)
    {
        this.fileSelection = fileSelection;
        this.index = index;

        t_File.SetText(fileName);
    }

    /// <summary>
    /// Resets the view
    /// </summary>
    public void ResetSelection()
    {
        background.color = Palette.darkGrey;
        t_File.color = Palette.white;

        rectTransform.localScale = Vector3.one;

        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (fileSelection != null)
        {
            // Selected
            if (index == fileSelection.SelectionIndex)
            {
                background.color = Palette.white;
                t_File.color = Palette.darkGrey;

                rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, new Vector3(selectedScale, selectedScale, selectedScale), selectedScaleSpeed * getReal3D.Cluster.deltaTime);
            }
            else // Unselected
            {
                background.color = Palette.darkGrey;
                t_File.color = Palette.white;

                rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, Vector3.one, selectedScaleSpeed * getReal3D.Cluster.deltaTime);
            }
        }
    }
}
