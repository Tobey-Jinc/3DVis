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

    //Allows materials to be reused
    private Dictionary<Material, Material> materialMappings = new Dictionary<Material, Material>();

    private int hierachyCount = 0;

    protected virtual void LateUpdate()
    {
        // Converts all images to overlay images
        // This is needed as the UI is a world space UI, so 3D objects could normally be drawn in front of it
        if (hierachyCount != transform.hierarchyCount)
        {
            Image[] images = gameObject.GetComponentsInChildren<Image>();

            foreach (Graphic image in images)
            {
                Material material = image.materialForRendering;
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
                image.material = materialCopy;
            }

            hierachyCount = transform.hierarchyCount;
        }
    }
}