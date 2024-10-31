using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using Vertex;
using System.IO;

public class KeyboardInput : getReal3D.MonoBehaviourWithRpc
{
    public static KeyboardInput Instance;

    private bool inMenu = false;

    [SerializeField] private Canvas canvas;
    [SerializeField] private TMP_Text t_InputField;
    [SerializeField] private TMP_Text t_Title;
    [SerializeField] private TMP_Text t_ValidationFailed;
    [SerializeField] private GameObject dots;
    [SerializeField] private RectTransform attention;
    [SerializeField] private GameObject windowNotFocusedWarning;
    [SerializeField] private float deleteInterval = 0.2f;

    private string text;
    private string previousText;
    private UnityAction<string> action;
    private System.Func<string, bool> validationMethod;
    private string validationFailedText;

    private float deleteCooldown = 0;

    private string setTextMethod = "SetText";
    private string submitMethod = "Submit";
    private string cancelMethod = "Cancel";

    public bool InMenu { get => inMenu; }

    private void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (inMenu)
        {
            if (getReal3D.Cluster.isMaster)
            {
                windowNotFocusedWarning.SetActive(!Application.isFocused); // Warn that the window isn't focused
                dots.SetActive(text.Length == 0);

                if (Input.GetKey(KeyCode.Backspace)) // Delete characters
                {
                    // Only delete if there are characters to delete and not in a cooldown
                    if (text.Length > 0 && deleteCooldown <= 0)
                    {
                        text = text.Remove(text.Length - 1);

                        deleteCooldown = deleteInterval;
                    }

                    deleteCooldown -= Time.deltaTime;
                }
                else if (Input.GetKeyDown(KeyCode.Return) && text.Length > 0) // Send text
                {
                    if (validationMethod == null || validationMethod.Invoke(text)) // Validate
                    {
                        action?.Invoke(text);

                        // Send to child nodes
                        CallRpc(submitMethod, text);

                        inMenu = false;
                    }
                    else
                    {
                        t_ValidationFailed.SetText(validationFailedText);
                    }
                }
                else // Input text
                {
                    string inputString = Input.inputString;

                    // Only allow valid windows file characters, and only allow input on the master node
                    if (getReal3D.Cluster.isMaster &&
                        inputString.Length > 0 && inputString.IndexOfAny(Path.GetInvalidFileNameChars()) == -1)
                    {
                        text += inputString;
                    }
                }

                // Clear delete cooldown
                if (!Input.GetKey(KeyCode.Backspace))
                {
                    deleteCooldown = 0;
                }

                // Set text on child nodes
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
            // Cancel input
            if (Input.GetKeyDown(KeyCode.Escape) || getReal3D.Input.GetButtonDown(Inputs.b))
            {
                CallRpc(cancelMethod);
                inMenu = false;
            }
        }
    }

    /// <summary>
    /// Updates the text in real on the child nodes
    /// </summary>
    /// <param name="text">The current text</param>
    [getReal3D.RPC]
    void SetText(string text)
    {
        if (!getReal3D.Cluster.isMaster)
        {
            this.text = text;
        }
    }

    /// <summary>
    /// Submits the text on the child nodes
    /// </summary>
    /// <param name="text">The text to submit</param>
    [getReal3D.RPC]
    void Submit(string text)
    {
        if (!getReal3D.Cluster.isMaster)
        {
            action?.Invoke(text);
            inMenu = false;
        }
    }

    /// <summary>
    /// Cancels input on the child nodes
    /// </summary>
    [getReal3D.RPC]
    void Cancel()
    {
        if (!getReal3D.Cluster.isMaster)
        {
            inMenu = false;
        }
    }

    /// <summary>
    /// Opens the keyboard input menu
    /// </summary>
    /// <param name="title">Menu title</param>
    /// <param name="action">Submit action</param>
    /// <param name="validationMethod">How should text be validated (null if no validation is needed)</param>
    /// <param name="validationFailedText">Text to be displayed if validation fails</param>
    /// <param name="startText">The initial text to show</param>
    public void Open(string title, UnityAction<string> action, System.Func<string, bool> validationMethod = null, string validationFailedText = "", string startText = "") 
    {
        this.action = action;
        this.validationMethod = validationMethod;
        this.validationFailedText = validationFailedText;

        text = startText;
        previousText = startText;

        t_InputField.SetText(text);
        t_Title.SetText(title);
        t_ValidationFailed.SetText(string.Empty);

        inMenu = true;
    }
}
