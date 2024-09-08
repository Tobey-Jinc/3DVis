using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vertex;

public class Viewpoint : MonoBehaviour
{
    [SerializeField] private RadialMenu radialMenu;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Transform cameraSecondTransform;

    [Header("Speeds")]
    [SerializeField] private float movementSpeed;
    [SerializeField] private float lookSpeed;

    private float xRotation = 0;
    private float yRotation = 0;

    void Update()
    {
        if (!radialMenu.InMenu)
        {
            Vector2 movementInput = new Vector2(getReal3D.Input.GetAxis(Inputs.leftStickY), getReal3D.Input.GetAxis(Inputs.leftStickX));

            characterController.Move((cameraTransform.right * movementInput.y + cameraTransform.forward * movementInput.x) * movementSpeed * Time.deltaTime);
        }
    }

    private void LateUpdate()
    {
        if (!radialMenu.InMenu)
        {
            Vector2 lookInput = new Vector2(getReal3D.Input.GetAxis(Inputs.rightStickY), getReal3D.Input.GetAxis(Inputs.rightStickX));

            xRotation += -lookInput.x * lookSpeed;
            yRotation += lookInput.y * lookSpeed;

            xRotation = Mathf.Clamp(xRotation, -90, 90);

            cameraTransform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
            cameraTransform.localPosition = Vector3.zero;
            cameraSecondTransform.localPosition = Vector3.zero;
        }
    }
}
