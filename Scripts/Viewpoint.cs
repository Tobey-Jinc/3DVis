using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vertex;

public class Viewpoint : MonoBehaviour
{
    [SerializeField] private ModelCursor modelCursor;
    [SerializeField] private RadialMenu radialMenu;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Transform wand;

    [Header("Speeds")]
    [SerializeField] private float movementSpeed;
    [SerializeField] private float lookSpeed;

    private float xRotation = 0;

    void Update()
    {
        if (CanMove())
        {
            Vector2 movementInput = new Vector2(getReal3D.Input.GetAxis(Inputs.leftStickY), getReal3D.Input.GetAxis(Inputs.leftStickX));
            float upDownInput = getReal3D.Input.GetAxis(Inputs.rightStickY);

            characterController.Move((wand.right * movementInput.y + wand.forward * movementInput.x + Vector3.up * upDownInput) * movementSpeed * getReal3D.Cluster.deltaTime);
        }
    }

    private void LateUpdate()
    {
        if (CanMove())
        {
            xRotation += getReal3D.Input.GetAxis(Inputs.rightStickX) * lookSpeed * getReal3D.Cluster.deltaTime;

            cameraTransform.rotation = Quaternion.Euler(0, xRotation, 0);
        }
    }

    private bool CanMove()
    {
        return !radialMenu.InMenu && (modelCursor.SelectedObject == null || modelCursor.TransformMode == TransformMode.None);
    }
}
