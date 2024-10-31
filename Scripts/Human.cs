using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Human : MonoBehaviour
{
    [SerializeField] private Transform uiContainer;

    private PlayerInputs playerInputs;

    void Awake()
    {
        playerInputs = GetComponent<PlayerInputs>();
    }

    void Update()
    {
        // Face where the wand is pointing. Apply smoothing
        getReal3D.Sensor headSensor = playerInputs.Wand;
        uiContainer.localRotation = Quaternion.Slerp(uiContainer.localRotation, Quaternion.Euler(0, headSensor.rotation.eulerAngles.y, 0), CurrentOptions.options.wandSmoothing * getReal3D.Cluster.deltaTime);
    }
}
