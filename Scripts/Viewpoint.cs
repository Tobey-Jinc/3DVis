using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Viewpoint : MonoBehaviour
{
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Transform cameraSecondTransform;

    [Header("Speeds")]
    [SerializeField] private float movementSpeed;
    [SerializeField] private float lookSpeed;

    [SerializeField] private TMP_Text t_Test;

    private float xRotation = 0;
    private float yRotation = 0;

    void Start()
    {
        
    }

    void Update()
    {
        Vector2 movementInput = new Vector2(getReal3D.Input.GetAxis("Forward"), getReal3D.Input.GetAxis("Yaw"));

        t_Test.SetText(transform.position.ToString());

        characterController.Move((cameraTransform.right * movementInput.y + cameraTransform.forward * movementInput.x) * movementSpeed * Time.deltaTime);
    }

    private void LateUpdate()
    {
        Vector2 lookInput = new Vector2(getReal3D.Input.GetAxis("Pitch"), getReal3D.Input.GetAxis("Strafe"));

        xRotation += -lookInput.x * lookSpeed;
        yRotation += lookInput.y * lookSpeed;

        xRotation = Mathf.Clamp(xRotation, -90, 90);

        cameraTransform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        cameraTransform.localPosition = Vector3.zero;
        cameraSecondTransform.localPosition = Vector3.zero;
    }
}
