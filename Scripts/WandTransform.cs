using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WandTransform : MonoBehaviour
{
    public static WandTransform Instance;

    [SerializeField] private Transform point;

    private PlayerInputs playerInputs;

    public Transform Transform { get => point; set => point = value; }

    void Awake()
    {
        Instance = this;

        playerInputs = GetComponent<PlayerInputs>();
    }

    void Update()
    {
        // Smoothly rotate to line up with the wand sensor
        getReal3D.Sensor headSensor = playerInputs.Wand;
        point.localRotation = Quaternion.Slerp(point.localRotation, headSensor.rotation, CurrentOptions.options.wandSmoothing * getReal3D.Cluster.deltaTime);
    }
}
