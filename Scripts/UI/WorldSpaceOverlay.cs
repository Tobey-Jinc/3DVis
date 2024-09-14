using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Modified version of code from https://discussions.unity.com/t/world-space-canvas-on-top-of-everything/128165/6

[ExecuteInEditMode] //Disable if you don't care about previewing outside of play mode
public class WorldSpaceOverlay : MonoBehaviour
{
    private const string shaderTestMode = "unity_GUIZTestMode"; //The magic property we need to set
    [SerializeField] UnityEngine.Rendering.CompareFunction desiredUIComparison = UnityEngine.Rendering.CompareFunction.Always; //If you want to try out other effects

    //Allows us to reuse materials
    private Dictionary<Material, Material> materialMappings = new Dictionary<Material, Material>();

    private int hierachyCount = 0;

    protected virtual void LateUpdate()
    {
        if (hierachyCount != transform.hierarchyCount)
        {
            Graphic[] graphics = gameObject.GetComponentsInChildren<Graphic>();
            TMP_Text[] textObjects = gameObject.GetComponentsInChildren<TMP_Text>();

            foreach (Graphic graphic in graphics)
            {
                Material material = graphic.materialForRendering;
                if (material == null)
                {
                    continue;
                }

                if (!materialMappings.TryGetValue(material, out Material materialCopy))
                {
                    materialCopy = new Material(material);
                    materialMappings.Add(material, materialCopy);
                }

                materialCopy.SetInt(shaderTestMode, (int)desiredUIComparison);
                graphic.material = materialCopy;
            }

            foreach (TMP_Text text in textObjects)
            {
                Material material = text.fontMaterial;
                if (material == null)
                {
                    continue;
                }

                if (!materialMappings.TryGetValue(material, out Material materialCopy))
                {
                    materialCopy = new Material(material);
                    materialMappings.Add(material, materialCopy);
                }

                Debug.Log(materialCopy.HasInt(shaderTestMode), text.gameObject);

                materialCopy.SetInt(shaderTestMode, (int)desiredUIComparison);
                text.material = materialCopy;
            }

            hierachyCount = transform.hierarchyCount;
        }
    }
}