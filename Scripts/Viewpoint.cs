using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vertex;

public class Viewpoint : MonoBehaviour
{
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
        if (!radialMenu.InMenu)
        {
            Vector2 movementInput = new Vector2(getReal3D.Input.GetAxis(Inputs.leftStickY), getReal3D.Input.GetAxis(Inputs.leftStickX));
            float upDownInput = getReal3D.Input.GetAxis(Inputs.rightStickY);

            characterController.Move((wand.right * movementInput.y + wand.forward * movementInput.x + wand.up * upDownInput) * movementSpeed * Time.deltaTime);
        }
    }

    private void LateUpdate()
    {
        if (!radialMenu.InMenu)
        {
            xRotation += getReal3D.Input.GetAxis(Inputs.rightStickX) * lookSpeed;

            cameraTransform.rotation = Quaternion.Euler(0, xRotation, 0);
        }
    }
}
