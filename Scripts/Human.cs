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
        getReal3D.Sensor headSensor = playerInputs.Wand;
        uiContainer.localRotation = Quaternion.Euler(0, headSensor.rotation.eulerAngles.y, 0);
    }
}
