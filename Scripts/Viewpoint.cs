using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vertex;

public class Viewpoint : getReal3D.MonoBehaviourWithRpc
{
    [SerializeField] private ObjectCursor modelCursor;
    [SerializeField] private RadialMenu radialMenu;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Transform wand;

    [Header("Speeds")]
    [SerializeField] private float movementSpeed;
    [SerializeField] private float lookSpeed;

    private float xRotation = 0;

    private string syncTransformMethod = "SyncTransform";

    void Update()
    {
        if (CanMove())
        {
            Vector2 movementInput = new Vector2(getReal3D.Input.GetAxis(Inputs.leftStickY), getReal3D.Input.GetAxis(Inputs.leftStickX));
            float upDownInput = getReal3D.Input.GetAxis(Inputs.rightStickY);

            float speed = CurrentOptions.options.movementSpeed;
            if (getReal3D.Input.GetButton(Inputs.x))
            {
                speed *= 2;
            }

            characterController.Move((wand.right * movementInput.y + wand.forward * movementInput.x + Vector3.up * upDownInput) * movementSpeed * speed * getReal3D.Cluster.deltaTime);
        }
    }

    private void LateUpdate()
    {
        if (CanMove())
        {
            float input = getReal3D.Input.GetAxis(Inputs.rightStickX);

            if (CurrentOptions.options.invertCameraControls)
            {
                input *= -1;
            }

            xRotation += input * lookSpeed * CurrentOptions.options.cameraSensitivity * getReal3D.Cluster.deltaTime;

            cameraTransform.rotation = Quaternion.Euler(0, xRotation, 0);
        }
    }

    private bool CanMove()
    {
        return !radialMenu.InMenu && (modelCursor.SelectedObject == null || modelCursor.CursorTransformMode == TransformMode.None);
    }

    public void SyncTransformWithHeadnode()
    {
        if (getReal3D.Cluster.isMaster)
        {
            CallRpc(syncTransformMethod, transform.position, xRotation);
        }
    }

    [getReal3D.RPC]
    private void SyncTransform(Vector3 position, float xRotation)
    {
        if (!getReal3D.Cluster.isMaster)
        {
            transform.position = position;

            this.xRotation = xRotation;
            cameraTransform.rotation = Quaternion.Euler(0, xRotation, 0);
        }
    }
}
