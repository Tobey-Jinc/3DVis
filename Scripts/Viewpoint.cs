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
            // Get input
            Vector2 movementInput = new Vector2(getReal3D.Input.GetAxis(Inputs.leftStickY), getReal3D.Input.GetAxis(Inputs.leftStickX));
            float upDownInput = getReal3D.Input.GetAxis(Inputs.rightStickY);

            // Determine speed
            float speed = CurrentOptions.options.movementSpeed;
            if (getReal3D.Input.GetButton(Inputs.x)) // Sprint
            {
                speed *= 2;
            }

            // Apply movement
            characterController.Move((wand.right * movementInput.y + wand.forward * movementInput.x + Vector3.up * upDownInput) * movementSpeed * speed * getReal3D.Cluster.deltaTime);
        }
    }

    private void LateUpdate() // Camera changes should be done in LateUpdate
    {
        if (CanMove())
        {
            // Get input
            float input = getReal3D.Input.GetAxis(Inputs.rightStickX);

            // Invert if necessary
            if (CurrentOptions.options.invertCameraControls)
            {
                input *= -1;
            }

            // Apply input
            xRotation += input * lookSpeed * CurrentOptions.options.cameraSensitivity * getReal3D.Cluster.deltaTime;

            cameraTransform.rotation = Quaternion.Euler(0, xRotation, 0);
        }
    }

    /// <summary>
    /// Determines if the user is in a state where they can move
    /// </summary>
    /// <returns>True if they can move</returns>
    private bool CanMove()
    {
        return !radialMenu.InMenu && (modelCursor.SelectedObject == null || modelCursor.CursorTransformMode == TransformMode.None);
    }

    /// <summary>
    /// Master node only - sends an RPC call to child nodes, telling them to sync the camera position and rotation with the master node
    /// </summary>
    public void SyncTransformWithMasterNode()
    {
        if (getReal3D.Cluster.isMaster)
        {
            CallRpc(syncTransformMethod, transform.position, xRotation);
        }
    }

    /// <summary>
    /// Syncs the transform with the master node
    /// </summary>
    /// <param name="position">Master node position</param>
    /// <param name="xRotation">Master node camera rotation</param>
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
