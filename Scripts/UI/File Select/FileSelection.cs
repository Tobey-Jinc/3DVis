using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Vertex;
using Unity.VisualScripting;
using UnityEngine.UIElements;

public class FileSelection : MonoBehaviour
{
    private bool inMenu = false;

    [SerializeField] private RadialMenu radialMenu;
    [SerializeField] private float inputDelay = 0.25f;
    [SerializeField] private RectTransform container;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private GameObject noFilesFoundWarning;
    [SerializeField] private TMP_Text t_Title;
    [SerializeField] private TMP_Text t_Category;
    [SerializeField] private TMP_Text t_PreviousCategory;
    [SerializeField] private TMP_Text t_NextCategory;
    [SerializeField] private RectTransform categoryBar;
    [SerializeField] private float scrollOffset;

    [SerializeField] private FileView fileViewPrefab;

    private FileStructure fileStructure;
    private List<FileView> fileViews = new List<FileView>();
    private List<FileView> pooledFileViews = new List<FileView>();

    private string category;
    private int categoryIndex;
    private int selectionIndex;
    private int fileCount;

    // Input
    private int prolongedInput = 0;
    private bool prolongingInput = false;
    private bool acceptInput = false;

    public bool InMenu { get => inMenu; }
    public int SelectionIndex { get => selectionIndex; }

    void Start()
    {
        container.localScale = Vector3.zero;
    }

    void Update()
    {
        if (inMenu)
        {
            if (fileStructure.files.Count > 0)
            {
                noFilesFoundWarning.SetActive(false);

                // Get navigation input
                int dpadInput = Inputs.Composite(Inputs.dpadDown, Inputs.dpadUp);
                int stickInput = Inputs.AxisToInt(Inputs.leftStickY) * -1;
                int input = Mathf.Clamp(dpadInput + stickInput, -1, 1);

                // Do navigation
                if (input != 0)
                {
                    if (!prolongingInput)
                    {
                        prolongingInput = true;
                        prolongedInput = input;
                        StartCoroutine(ProlongInput());
                    }
                    else if (input != prolongedInput)
                    {
                        prolongingInput = false;
                        StopAllCoroutines();
                    }
                }
                else
                {
                    prolongingInput = false;
                    StopAllCoroutines();
                }

                // Set content pivot
                if (scrollRect.verticalScrollbar.gameObject.activeInHierarchy)
                {
                    scrollRect.content.pivot = new Vector2(0, 0);
                }
                else
                {
                    scrollRect.content.pivot = new Vector2(0, 1);
                }

                Canvas.ForceUpdateCanvases();

                // Handle scroll focus
                Vector2 pos = (Vector2)scrollRect.transform.InverseTransformPoint(scrollRect.content.position)
                               - (Vector2)scrollRect.transform.InverseTransformPoint(fileViews[selectionIndex].transform.position);

                Vector2 scrollPos = new Vector2(scrollRect.content.anchoredPosition.x, pos.y - scrollOffset);
                scrollRect.content.anchoredPosition = Vector2.Lerp(scrollRect.content.anchoredPosition, scrollPos, 10 * getReal3D.Cluster.deltaTime);

                scrollRect.verticalNormalizedPosition = Mathf.Clamp(scrollRect.verticalNormalizedPosition, 0, 1);
                scrollRect.horizontalNormalizedPosition = Mathf.Clamp(scrollRect.horizontalNormalizedPosition, 0, 1);

                // Handle categories
                if (fileStructure.categories.Length > 1)
                {
                    // Get input
                    int categoryScroll = Inputs.Composite(Inputs.leftShoulder, Inputs.rightShoulder, false);

                    // Move through categories
                    if (categoryScroll != 0)
                    {
                        categoryIndex += categoryScroll;
                        if (categoryIndex < 0)
                        {
                            categoryIndex = fileStructure.categories.Length - 1;
                        }
                        else if (categoryIndex > fileStructure.categories.Length - 1)
                        {
                            categoryIndex = 0;
                        }

                        // Regenerate file structure with category
                        GenerateFileSelection(fileStructure, fileStructure.categories[categoryIndex]);
                    }
                }
            }
            else
            {
                noFilesFoundWarning.SetActive(true);
            }

            container.localScale = Vector3.Lerp(container.localScale, Vector3.one, Data.menuScaleSpeed * getReal3D.Cluster.deltaTime);
        }
        else
        {
            container.localScale = Vector3.Lerp(container.localScale, Vector3.zero, Data.menuScaleSpeed * getReal3D.Cluster.deltaTime);

            // Ensures the scroll view is at the top upon opening
            scrollRect.verticalNormalizedPosition = 1;
        }
    }

    private void LateUpdate()
    {
        if (inMenu)
        {
            // Select a file
            if (acceptInput && getReal3D.Input.GetButtonDown(Inputs.a))
            {
                fileStructure.action?.Invoke(fileStructure.files[category][selectionIndex][1]);
                if (fileStructure.closeOnSelect)
                {
                    radialMenu.Close();
                    Close();
                }
            }

            acceptInput = true;

            // Close the menu
            if (getReal3D.Input.GetButtonDown(Inputs.b))
            {
                Close();
            }
        }
        else
        {
            acceptInput = false;
        }
    }

    /// <summary>
    /// Closes the menu
    /// </summary>
    private void Close()
    {
        categoryIndex = 0;
        inMenu = false;
    }

    /// <summary>
    /// Generates the file structure
    /// </summary>
    /// <param name="fileStructure">The file structure to generate from</param>
    /// <param name="category">The cetagory to generate with</param>
    public void GenerateFileSelection(FileStructure fileStructure, string category = Data.allCategory)
    {
        ClearFileViews();

        this.fileStructure = fileStructure;
        this.category = category;

        if (fileStructure.files.Count > 0)
        {
            fileCount = fileStructure.files[category].Count;
        }
        else
        {
            fileCount = 0;
        }

        t_Title.SetText(fileStructure.title);

        ShowCategories();

        for (int i = 0; i < fileCount; i++)
        {
            string fileName = fileStructure.files[category][i][0];

            FileView fileView;
            if (pooledFileViews.Count > 0) //  Create file view from pool
            {
                fileView = pooledFileViews[0];

                pooledFileViews.RemoveAt(0);

                fileView.Setup(i, fileName);
            }
            else // Create a new file view
            {
                fileView = Instantiate(fileViewPrefab, scrollRect.content);

                fileView.Setup(this, i, fileName);
            }

            // Make sure the file view is last
            fileView.transform.SetAsLastSibling();

            fileViews.Add(fileView);
        }

        selectionIndex = 0;
        prolongingInput = false;
        StopAllCoroutines();

        inMenu = true;
    }

    /// <summary>
    /// Displays the categories
    /// </summary>
    private void ShowCategories()
    {
        int categoryCount = fileStructure.categories.Length - 1;
        if (categoryCount > 0)
        {
            categoryBar.localScale = Vector3.one;

            // Determine previous and next categories
            int previousCategory = categoryIndex - 1;
            int nextCategory = categoryIndex + 1;

            if (previousCategory < 0)
            {
                previousCategory = categoryCount;
            }

            if (nextCategory > categoryCount)
            {
                nextCategory = 0;
            }

            // Show category names
            t_Category.SetText(category);
            t_PreviousCategory.SetText(fileStructure.categories[previousCategory]);
            t_NextCategory.SetText(fileStructure.categories[nextCategory]);
        }
        else
        {
            categoryBar.localScale = Vector3.zero;
        }
    }

    /// <summary>
    /// Deletes all file views. Adds them to the pool
    /// </summary>
    private void ClearFileViews()
    {
        foreach (FileView fileView in fileViews)
        {
            fileView.ResetSelection();
            pooledFileViews.Add(fileView);
        }

        fileViews.Clear();
    }

    /// <summary>
    /// Allows navigation inputs to be held
    /// </summary>
    /// <returns></returns>
    private IEnumerator ProlongInput()
    {
        // Apply and wrap navigation
        selectionIndex += prolongedInput;
        if (selectionIndex > fileCount - 1)
        {
            selectionIndex = 0;
        }
        else if (selectionIndex < 0)
        {
            selectionIndex = fileCount - 1;
        }

        yield return new WaitForSeconds(inputDelay);

        StartCoroutine(ProlongInput());
    }

    /// <summary>
    /// Helper method for adding a file to a file structure
    /// </summary>
    /// <param name="files">The files to add to</param>
    /// <param name="category">The files' category</param>
    /// <param name="file">The file to add</param>
    public static void AddFile(Dictionary<string, List<string[]>> files, string category, string[] file)
    {
        if (category != string.Empty)
        {
            // Add the category if hasn't already
            if (!files.ContainsKey(category))
            {
                files.Add(category, new List<string[]>() { file });
            }
            else // Add to the existing category
            {
                files[category].Add(file);
            }
        }
    }
}
