using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WandTransform : MonoBehaviour
{
    [SerializeField] private Transform point;

    private PlayerInputs playerInputs;

    void Awake()
    {
        playerInputs = GetComponent<PlayerInputs>();
    }

    void Update()
    {
        getReal3D.Sensor headSensor = playerInputs.Wand;
        point.localRotation = headSensor.rotation;
    }
}
