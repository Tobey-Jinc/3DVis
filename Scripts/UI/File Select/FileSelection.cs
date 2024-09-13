using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Vertex;

public class FileSelection : MonoBehaviour
{
    private bool inMenu = false;

    [SerializeField] private float inputDelay = 0.25f;
    [SerializeField] private RectTransform container;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private TMP_Text t_Title;
    [SerializeField] private float scrollOffset;

    [SerializeField] private FileView fileViewPrefab;

    private FileStructure fileStructure;
    private List<FileView> fileViews = new List<FileView>();
    private List<FileView> pooledFileViews = new List<FileView>();

    private int selectionIndex = 0;

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

            if (acceptInput && getReal3D.Input.GetButtonDown(Inputs.a))
            {
                fileStructure.action?.Invoke(fileStructure.files[selectionIndex][1]);
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
            acceptInput = true;

            if (getReal3D.Input.GetButtonDown(Inputs.b))
            {
                inMenu = false;
            }
        }
        else
        {
            acceptInput = false;
        }
    }

    public void GenerateFileSelection(FileStructure fileStructure)
    {
        ClearFileViews();

        this.fileStructure = fileStructure;

        t_Title.SetText(fileStructure.title);

        for (int i = 0; i < fileStructure.fileCount; i++)
        {
            string fileName = fileStructure.files[i][0];

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

            fileViews.Add(fileView);
        }

        selectionIndex = 0;
        prolongingInput = false;
        StopAllCoroutines();

        inMenu = true;
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
        if (selectionIndex > fileStructure.fileCount - 1)
        {
            selectionIndex = 0;
        }
        else if (selectionIndex < 0)
        {
            selectionIndex = fileStructure.fileCount - 1;
        }

        yield return new WaitForSeconds(inputDelay);

        StartCoroutine(ProlongInput());
    }
}
