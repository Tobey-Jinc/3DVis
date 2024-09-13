using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vertex;

public class ModelCursor : MonoBehaviour
{
    [SerializeField] private Transform wand;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private Transform cursor;
    [SerializeField] private Material foundModelMaterial;
    [SerializeField] private Material didNotModelMaterial;

    void Update()
    {
        lineRenderer.SetPosition(0, wand.position);

        if (Physics.Raycast(wand.position, wand.forward, out RaycastHit hit, Mathf.Infinity, 1 << LayerMask.NameToLayer(Layer.model), QueryTriggerInteraction.Collide))
        {
            cursor.position = hit.point + (hit.normal * 0.1f);
            cursor.forward = hit.normal;
            cursor.gameObject.SetActive(true);

            lineRenderer.SetPosition(1, hit.point);
            lineRenderer.material = foundModelMaterial;
        }
        else
        {
            cursor.gameObject.SetActive(false);

            lineRenderer.SetPosition(1, wand.position + (wand.forward * 1000));
            lineRenderer.material = didNotModelMaterial;
        }
    }
}
