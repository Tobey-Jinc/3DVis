using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using Vertex;
using System.IO;

public class KeyboardInput : getReal3D.MonoBehaviourWithRpc
{
    private bool inMenu = false;

    [SerializeField] private Canvas canvas;
    [SerializeField] private TMP_Text t_InputField;
    [SerializeField] private TMP_Text t_Title;
    [SerializeField] private TMP_Text t_ValidationFailed;
    [SerializeField] private RectTransform attention;

    private string text;
    private string previousText;
    private UnityAction<string> action;
    private System.Func<string, bool> validationMethod;
    private string validationFailedText;

    private string setTextMethod = "SetText";

    public bool InMenu { get => inMenu; }

    void Update()
    {
        if (inMenu)
        {
            if (getReal3D.Cluster.isMaster)
            {
                if (Input.GetKeyDown(KeyCode.Backspace))
                {
                    if (text.Length > 0)
                    {
                        text = text.Remove(text.Length - 1);
                    }
                }
                else if (Input.GetKeyDown(KeyCode.Return) && text.Length > 0)
                {
                    if (validationMethod == null || validationMethod.Invoke(text))
                    {
                        action?.Invoke(text);
                        inMenu = false;
                    }
                    else
                    {
                        t_ValidationFailed.SetText(validationFailedText);
                    }
                }
                else
                {
                    string inputString = Input.inputString;

                    if (getReal3D.Cluster.isMaster &&
                        inputString.Length > 0 && inputString.IndexOfAny(Path.GetInvalidFileNameChars()) == -1)
                    {
                        text += inputString;
                    }
                }

                if (text != previousText)
                {
                    CallRpc(setTextMethod, text);
                    t_InputField.SetText(text);
                }
                previousText = text;

                canvas.enabled = true;
                attention.localScale = Vector3.zero;
            }
            else
            {
                canvas.enabled = false;
                attention.localScale = Vector3.Lerp(attention.localScale, Vector3.one, Data.menuScaleSpeed * getReal3D.Cluster.deltaTime);
            }
        }
        else
        {
            canvas.enabled = false;
            attention.localScale = Vector3.Lerp(attention.localScale, Vector3.zero, Data.menuScaleSpeed * getReal3D.Cluster.deltaTime);
        }
    }

    private void LateUpdate()
    {
        if (inMenu)
        {
            if (Input.GetKeyDown(KeyCode.Escape) || getReal3D.Input.GetButtonDown(Inputs.b))
            {
                inMenu = false;
            }
        }
    }

    [getReal3D.RPC]
    void SetText(string text)
    {
        if (!getReal3D.Cluster.isMaster)
        {
            this.text = text;
        }
    }

    public void Open(string title, UnityAction<string> action, System.Func<string, bool> validationMethod = null, string validationFailedText = "") 
    {
        this.action = action;
        this.validationMethod = validationMethod;
        this.validationFailedText = validationFailedText;

        text = string.Empty;
        previousText = string.Empty;

        t_InputField.SetText(text);
        t_Title.SetText(title);
        t_ValidationFailed.SetText(string.Empty);

        inMenu = true;
    }
}
