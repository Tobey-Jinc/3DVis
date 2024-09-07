using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public struct Quadrant
{
    [SerializeField] public RectTransform rectTransform;
    [SerializeField] public Image icon;
    [SerializeField] public Image background;
}

public class RadialMenu : MonoBehaviour
{
    [SerializeField] private TMP_Text t_Title;
    [SerializeField] private TMP_Text t_Label;
    [SerializeField] private TMP_Text t_Page;

    [Header("Quadrants")]
    [SerializeField] private Quadrant topLeft;
    [SerializeField] private Quadrant topRight;
    [SerializeField] private Quadrant bottomLeft;
    [SerializeField] private Quadrant bottomRight;

    [Header("Icons")]
    [SerializeField] private Sprite modelIcon;

    private enum Menu
    {
        Main,
        Environments
    }
    private Menu currentMenu = Menu.Main;

    private Dictionary<Menu, RadialMenuData> menus = new();

    private string pageFormat = "{0} / {1}";

    void Start()
    {
        CreateMenu(
            Menu.Main, "Main", new RadialQuadrantData[]
            { 
                new RadialQuadrantData("Models", modelIcon, () => { Debug.Log("Models"); }),
                new RadialQuadrantData("Environemnt", modelIcon, () => { Debug.Log("Env"); }),
                new RadialQuadrantData("Record", modelIcon, () => { Debug.Log("Record"); }),
                new RadialQuadrantData("Options", modelIcon, () => { Debug.Log("Options"); }),
            }
        );

        GoToMenu(Menu.Main);
    }

    void Update()
    {
        Vector2 selectInput = new Vector2(getReal3D.Input.GetAxis("Pitch"), getReal3D.Input.GetAxis("Yaw"));
        if (selectInput != Vector2.zero)
        {
            float angle = Mathf.Atan2(selectInput.y, selectInput.x) / Mathf.PI;
            angle *= 180;
            angle += 90;
            if (angle < 0)
            {
                angle += 360;
            }
            Debug.Log(angle);

            //bottomLeft.rectTransform.localRotation = Quaternion.Euler(0, 0, angle);
        }
    }

    private void CreateMenu(Menu menu, string title, RadialQuadrantData[] quadrants)
    {
        menus.Add(menu, new RadialMenuData(title, quadrants));
    }

    private void GoToMenu(Menu menu)
    {
        currentMenu = menu;
        RenderPage(0, menus[menu]);
    }

    private void RenderPage(int page, RadialMenuData menuData)
    {
        topLeft.icon.sprite = menuData.quadrants[0].icon;
        topRight.icon.sprite = menuData.quadrants[1].icon;
        bottomLeft.icon.sprite = menuData.quadrants[2].icon;
        bottomRight.icon.sprite = menuData.quadrants[3].icon;
    }
}
