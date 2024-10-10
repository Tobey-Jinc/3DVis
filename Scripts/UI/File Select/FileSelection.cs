using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Vertex;
using Unity.VisualScripting;

public class FileSelection : MonoBehaviour
{
    private bool inMenu = false;

    [SerializeField] private RadialMenu radialMenu;
    [SerializeField] private float inputDelay = 0.25f;
    [SerializeField] private RectTransform container;
    [SerializeField] private ScrollRect scrollRect;
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
            int dpadInput = Inputs.Composite(Inputs.dpadDown, Inputs.dpadUp);
            int stickInput = Inputs.AxisToInt(Inputs.leftStickY) * -1;
            int input = Mathf.Clamp(dpadInput + stickInput, -1, 1);

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

            if (scrollRect.verticalScrollbar.gameObject.activeInHierarchy)
            {
                scrollRect.content.pivot = new Vector2(0, 0);
            }
            else
            {
                scrollRect.content.pivot = new Vector2(0, 1);
            }

            Canvas.ForceUpdateCanvases();

            Vector2 pos = (Vector2)scrollRect.transform.InverseTransformPoint(scrollRect.content.position)
                           - (Vector2)scrollRect.transform.InverseTransformPoint(fileViews[selectionIndex].transform.position);

            Vector2 scrollPos = new Vector2(scrollRect.content.anchoredPosition.x, pos.y - scrollOffset);
            scrollRect.content.anchoredPosition = Vector2.Lerp(scrollRect.content.anchoredPosition, scrollPos, 10 * getReal3D.Cluster.deltaTime);

            scrollRect.verticalNormalizedPosition = Mathf.Clamp(scrollRect.verticalNormalizedPosition, 0, 1);
            scrollRect.horizontalNormalizedPosition = Mathf.Clamp(scrollRect.horizontalNormalizedPosition, 0, 1);

            container.localScale = Vector3.Lerp(container.localScale, Vector3.one, Data.menuScaleSpeed * getReal3D.Cluster.deltaTime);

            if (fileStructure.categories.Length > 1)
            {
                int categoryScroll = Inputs.Composite(Inputs.leftShoulder, Inputs.rightShoulder, false);
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

                    GenerateFileSelection(fileStructure, fileStructure.categories[categoryIndex]);
                }
            }
        }
        else
        {
            container.localScale = Vector3.Lerp(container.localScale, Vector3.zero, Data.menuScaleSpeed * getReal3D.Cluster.deltaTime);
        }
    }

    private void LateUpdate()
    {
        if (inMenu)
        {
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

    private void Close()
    {
        categoryIndex = 0;
        inMenu = false;
    }

    public void GenerateFileSelection(FileStructure fileStructure, string category = Data.allCategory)
    {
        ClearFileViews();

        this.fileStructure = fileStructure;
        this.category = category;
        fileCount = fileStructure.files[category].Count;

        t_Title.SetText(fileStructure.title);

        ShowCategories();

        for (int i = 0; i < fileCount; i++)
        {
            string fileName = fileStructure.files[category][i][0];

            FileView fileView;
            if (pooledFileViews.Count > 0)
            {
                fileView = pooledFileViews[0];

                pooledFileViews.RemoveAt(0);

                fileView.Setup(i, fileName);
            }
            else
            {
                fileView = Instantiate(fileViewPrefab, scrollRect.content);

                fileView.Setup(this, i, fileName);
            }

            fileView.transform.SetAsLastSibling();
            fileViews.Add(fileView);
        }

        selectionIndex = 0;
        prolongingInput = false;
        StopAllCoroutines();

        inMenu = true;
    }

    private void ShowCategories()
    {
        int categoryCount = fileStructure.categories.Length - 1;
        if (categoryCount > 0)
        {
            categoryBar.localScale = Vector3.one;

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

            t_Category.SetText(category);
            t_PreviousCategory.SetText(fileStructure.categories[previousCategory]);
            t_NextCategory.SetText(fileStructure.categories[nextCategory]);
        }
        else
        {
            categoryBar.localScale = Vector3.zero;
        }
    }

    private void ClearFileViews()
    {
        foreach (FileView fileView in fileViews)
        {
            fileView.ResetSelection();
            pooledFileViews.Add(fileView);
        }

        fileViews.Clear();
    }

    private IEnumerator ProlongInput()
    {
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

    public static void AddFile(Dictionary<string, List<string[]>> files, string category, string[] file)
    {
        if (category != string.Empty)
        {
            if (!files.ContainsKey(category))
            {
                files.Add(category, new List<string[]>() { file });
            }
            else
            {
                files[category].Add(file);
            }
        }
    }
}
