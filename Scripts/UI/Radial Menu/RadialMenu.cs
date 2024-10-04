using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Vertex;

/// <summary>
/// The fundamental definition of each menu
/// </summary>
public enum Menu
{
    None,
    Main,
    Environments
}

/// <summary>
/// Defines a quadrants appearance in the radial menu
/// </summary>
[System.Serializable]
public struct Quadrant
{
    [SerializeField] public Image icon;
    [SerializeField] public Image background;

    private bool active; // True if the quadrant should be shown
    private bool selected; // True if the quadrant has been selected

    public void Update()
    {
        if (active)
        {
            if (selected)
            {
                Scale(background.rectTransform, 1.1f, background.rectTransform.localScale.x >= 1 ? 12: 30);

                icon.color = Palette.darkGrey;
                background.color = Palette.white;
            }
            else
            {
                Scale(background.rectTransform, 1, background.rectTransform.localScale.x >= 1 ? 12 : 30);

                icon.color = Palette.white;
                background.color = Palette.darkGrey;
            }
        }
        else
        {
            Scale(background.rectTransform, 0, 30);
        }

        Scale(icon.rectTransform, 1, 15);
    }

    /// <summary>
    /// Tries to activate a quadrant
    /// </summary>
    /// <param name="quadrantCount">Total quadrants on the current page</param>
    /// <param name="index">The index of this quadrant</param>
    /// <param name="data">The data to visualize if the quadrant is active</param>
    public void SetActive(int quadrantCount, int index, RadialQuadrantData[] data)
    {
        if (quadrantCount > index) // Activate the quadrant
        {
            active = true;

            icon.rectTransform.localScale = Vector3.zero;
            icon.sprite = data[index].icon;
        }
        else // Deactivate the quadrant
        {
            active = false;
        }
    }

    /// <summary>
    /// Tries to select the quadrant based on the given condition
    /// </summary>
    /// <param name="condition">Selection condition</param>
    public void TryToSelect(bool condition)
    {
        selected = condition;
    }

    /// <summary>
    /// Scales the given RectTransform
    /// </summary>
    /// <param name="rectTransform">The RectTransform to scale</param>
    /// <param name="target">The target scale</param>
    /// <param name="speed">The speed of the scaling. Deltatime is automatically applied</param>
    private void Scale(RectTransform rectTransform, float target, float speed)
    {
        rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, new Vector3(target, target, target), speed * getReal3D.Cluster.deltaTime);
    }
}

public class RadialMenu : MonoBehaviour
{
    private bool inMenu = false;

    [SerializeField] private ModelCache modelCache;
    [SerializeField] private SceneDescriptionManager sceneDescriptionManager;
    [SerializeField] private Environments environments;
    [SerializeField] private ObjectCursor modelCursor;
    [SerializeField] private FileSelection fileSelection;
    [SerializeField] private KeyboardInput keyboardInput;
    [SerializeField] private Viewpoint viewpoint;
    [SerializeField] private RectTransform container;
    [SerializeField] private TMP_Text t_Title;
    [SerializeField] private TMP_Text t_Label;
    [SerializeField] private TMP_Text t_Page;
    [SerializeField] private TMP_Text t_LT;

    [Header("Quadrants")]
    [SerializeField] private Quadrant topLeft;
    [SerializeField] private Quadrant topRight;
    [SerializeField] private Quadrant bottomLeft;
    [SerializeField] private Quadrant bottomRight;

    [Header("Icons")]
    [SerializeField] private Sprite modelIcon;
    [SerializeField] private Sprite environmentIcon;
    [SerializeField] private Sprite recordIcon;
    [SerializeField] private Sprite optionsIcon;

    private Menu currentMenu = Menu.Main;
    private Dictionary<Menu, RadialMenuData> menus = new Dictionary<Menu, RadialMenuData>(); // Contains all menus and their data
    private int selectionIndex = 0; // The selected quadrant
    private int page = 0;
    private RadialQuadrantData[] pageQuadrants; // The data for each quadrant on the current page
    private int quadrantPageCount = 4; // The number of quadrants on the current page (max 4)
    private int totalQuadrantCount = 4; // The total number of quadrants in the current menu

    private string pageFormat = "{0} / {1}";

    public bool InMenu { get => inMenu; }

    void Start()
    {
        // =========================================================================================================================
        // DEFINE MENUS HERE

        CreateMenu(
            Menu.Main, Menu.None, "Main", new RadialQuadrantData[]
            {
                new RadialQuadrantData("Models", modelIcon, () => { CreateModelExplorer(); }),
                new RadialQuadrantData("Environemnt", environmentIcon, () => { CreateEnvironmentExplorer(); }),
                new RadialQuadrantData("Options", optionsIcon, () => { CreateSceneExplorer(); }),
                new RadialQuadrantData("Save", recordIcon, () => { 
                    keyboardInput.Open("Name your scene", (string fileName) => 
                        { 
                            sceneDescriptionManager.SaveScene(fileName); 
                        }, (string text) =>
                        { 
                            return sceneDescriptionManager.ValidateSceneName(text);
                        }, "Scene name is already taken!"); 
                }),
                new RadialQuadrantData("Sync", recordIcon, () => { viewpoint.SyncTransformWithHeadnode(); }),
                new RadialQuadrantData("Record2", recordIcon, () => { modelCache.InstantiateTextObject(); }),
                new RadialQuadrantData("Record2", recordIcon, () => { Debug.Log("Record2"); }),
                new RadialQuadrantData("Record2", recordIcon, () => { Debug.Log("Record2"); }),
                new RadialQuadrantData("Record2", recordIcon, () => { Debug.Log("Record2"); }),
            }
        );



        CreateMenu(
            Menu.Environments, Menu.Main, "Environment", new RadialQuadrantData[]
            {
                new RadialQuadrantData("Environemnt1", environmentIcon, () => { Debug.Log("Environment"); }),
                new RadialQuadrantData("Environemnt2", environmentIcon, () => { Debug.Log("Environment2"); }),
                new RadialQuadrantData("Environemnt3", environmentIcon, () => { Debug.Log("Environment3"); }),
                new RadialQuadrantData("Environemnt4", environmentIcon, () => { Debug.Log("Environment4"); }),
                new RadialQuadrantData("Environemnt5", environmentIcon, () => { Debug.Log("Environment5"); }),
            }
        );

        // =========================================================================================================================

        container.localScale = Vector3.zero;
    }

    void Update()
    {
        t_LT.SetText(getReal3D.Input.GetButton(Inputs.leftTrigger).ToString());

        if (inMenu)
        {
            // Only run logic if not in a file selection menu
            if (!fileSelection.InMenu && !keyboardInput.InMenu)
            {
                // Try to select a quadrant
                int index = SelectionIndex();
                if (index != -1)
                {
                    selectionIndex = index;
                    topLeft.TryToSelect(index == 0);
                    topRight.TryToSelect(index == 1);
                    bottomRight.TryToSelect(index == 2);
                    bottomLeft.TryToSelect(index == 3);
                }

                // Selected quadrant logic
                if (selectionIndex < quadrantPageCount)
                {
                    RadialQuadrantData data = pageQuadrants[selectionIndex];

                    t_Label.SetText(data.label);

                    // Run select quadrants action
                    if (getReal3D.Input.GetButtonDown(Inputs.a))
                    {
                        data.action?.Invoke();
                    }
                }
                else
                {
                    SelectFirstQuadrant();
                }

                if (getReal3D.Input.GetButtonDown(Inputs.b)) // Go back
                {
                    if (currentMenu != Menu.Main)
                    {
                        GoToMenu(menus[currentMenu].previousMenu);
                    }
                    else // Close radial menu if in main menu
                    {
                        inMenu = false;
                    }
                }
                else if (getReal3D.Input.GetButtonDown(Inputs.leftShoulder)) // Previous page
                {
                    IncrementPage(-1);
                }
                else if (getReal3D.Input.GetButtonDown(Inputs.rightShoulder)) // Next page
                {
                    IncrementPage(1);
                }

                // Show radial menu
                container.localScale = Vector3.Lerp(container.localScale, Vector3.one, 30 * getReal3D.Cluster.deltaTime);
            }
            else
            {
                // Hide radial menu
                container.localScale = Vector3.Lerp(container.localScale, Vector3.zero, 30 * getReal3D.Cluster.deltaTime);
            }
        }
        else
        {
            // Activate radial menu
            if (modelCursor.SelectedObject == null && getReal3D.Input.GetButtonDown(Inputs.a) && !modelCursor.Active)
            {
                currentMenu = Menu.Main;
                GoToMenu(currentMenu);
                inMenu = true;

                SelectFirstQuadrant();
            }

            // Hide radial menu
            container.localScale = Vector3.Lerp(container.localScale, Vector3.zero, Data.menuScaleSpeed * getReal3D.Cluster.deltaTime);
        }

        // Update the appearance of each quadrant
        topLeft.Update();
        topRight.Update();
        bottomRight.Update();
        bottomLeft.Update();
    }

    /// <summary>
    /// Gets the index of the selected quadrant based on user input
    /// </summary>
    /// <returns></returns>
    private int SelectionIndex()
    {
        // Get user input from the left stick
        Vector2 selectInput = new Vector2(getReal3D.Input.GetAxis(Inputs.leftStickY), getReal3D.Input.GetAxis(Inputs.leftStickX));
        selectInput.Normalize();

        if (selectInput != Vector2.zero)
        {
            // Calculate angle of input
            float angle = Mathf.Atan2(selectInput.y, selectInput.x) / Mathf.PI;
            angle *= 180;
            angle += 45;
            if (angle < 0)
            {
                angle += 360;
            }

            angle = Mathf.Round(angle / 90) * 90; // Round input to 90 degree increments

            // Convert 360 to 0 for simplicity
            if (angle == 360)
            {
                angle = 0;
            }

            // Convert angle to index
            if (angle == 0)
            {
                return 0;
            }
            else if (angle == 90)
            {
                return 1;
            }
            else if (angle == 180)
            {
                return 2;
            }
            else
            {
                return 3;
            }
        }

        return -1;
    }

    /// <summary>
    /// Creates a menu. Quadrant order: TOP LEFT, TOP RIGHT, BOTTOM RIGHT, BOTTOM LEFT
    /// </summary>
    /// <param name="menu">Menu ID</param>
    /// <param name="previousMenu">ID of the previous menu</param>
    /// <param name="title">Menu title</param>
    /// <param name="quadrants">All the quadrants in the menu. Pagination is automatic</param>
    private void CreateMenu(Menu menu, Menu previousMenu, string title, RadialQuadrantData[] quadrants)
    {
        menus.Add(menu, new RadialMenuData(title, quadrants, previousMenu));
    }

    /// <summary>
    /// Go to the given menu
    /// </summary>
    /// <param name="menu">The menu to navigate to</param>
    private void GoToMenu(Menu menu)
    {
        currentMenu = menu;

        RadialMenuData menuData = menus[menu];

        totalQuadrantCount = menuData.quadrants.Length;

        // Render the quadrants on the first page
        page = 0;
        RenderPage(page, menuData);

        t_Title.SetText(menuData.title);
    }

    /// <summary>
    /// Renders a page of quadrants
    /// </summary>
    /// <param name="page">The page index to render</param>
    /// <param name="menuData">The menu to get quadrants from</param>
    private void RenderPage(int page, RadialMenuData menuData)
    {
        // Determine the range of quadrants to render
        int totalQuadrants = menuData.quadrants.Length;
        int startPage = Mathf.Clamp(page * 4, 0, totalQuadrants);
        int endPage = Mathf.Clamp(startPage + 4, 0, totalQuadrants);

        // The quadrants to render
        pageQuadrants = menuData.quadrants[startPage..endPage];

        quadrantPageCount = pageQuadrants.Length;

        // Try to render each quadrant
        topLeft.SetActive(quadrantPageCount, 0, pageQuadrants);
        topRight.SetActive(quadrantPageCount, 1, pageQuadrants);
        bottomRight.SetActive(quadrantPageCount, 2, pageQuadrants);
        bottomLeft.SetActive(quadrantPageCount, 3, pageQuadrants);

        // Display the page number
        t_Page.SetText(pageFormat, page + 1, Mathf.CeilToInt(totalQuadrantCount / 4f));
    }

    /// <summary>
    /// Increments the current page
    /// </summary>
    /// <param name="increment">The increment amount</param>
    private void IncrementPage(int increment)
    {
        int previousPage = page;

        // Clamp the page
        page = Mathf.Clamp(page + increment, 0, Mathf.CeilToInt(totalQuadrantCount / 4f) - 1);

        // Only render the page if the page changed
        if (page != previousPage)
        {
            RenderPage(page, menus[currentMenu]);
        }
    }

    /// <summary>
    /// Selects the first (top left) quadrant
    /// </summary>
    private void SelectFirstQuadrant()
    {
        selectionIndex = 0;
        topLeft.TryToSelect(true);
        topRight.TryToSelect(false);
        bottomRight.TryToSelect(false);
        bottomLeft.TryToSelect(false);
    }

    private void CreateModelExplorer()
    {
        if (ModelCache.Loaded)
        {
            fileSelection.GenerateFileSelection(modelCache.GetFileStructure());
        }
    }

    private void CreateEnvironmentExplorer()
    {
        fileSelection.GenerateFileSelection(environments.GetFileStructure());
    }

    private void CreateSceneExplorer()
    {
        if (ModelCache.Loaded)
        {
            fileSelection.GenerateFileSelection(sceneDescriptionManager.GetFileStructure());
        }
    }
}
