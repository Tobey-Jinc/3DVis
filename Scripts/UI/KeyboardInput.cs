using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class KeyboardInput : getReal3D.MonoBehaviourWithRpc
{
    [SerializeField] private TMP_Text t_InputField;

    void Update()
    {
        string inputString = Input.inputString;
        if (getReal3D.Cluster.isMaster &&
            inputString.Length > 0)
        {
            CallRpc("TypeInput", inputString);
        }
    }

    [getReal3D.RPC]
    void TypeInput(string inputString)
    {
        t_InputField.text += inputString;
    }
}
