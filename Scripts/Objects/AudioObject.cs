using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using Vertex;

public class AudioObject : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private Transform minDistanceVisualizer;
    [SerializeField] private Transform maxDistanceVisualizer;
    [SerializeField] private Transform volumeVisualizer;
    [SerializeField] private MeshRenderer editorRenderer;

    private ObjectCursor cursor;
    private KeyboardInput keyboardInput;

    private TransformModeAndControls[] transformModes;

    [SerializeField] public Audio myAudio;

    public AudioSource AudioSource { get => audioSource; }

    void Start()
    {
        cursor = ObjectCursor.Instance;
        keyboardInput = KeyboardInput.Instance;

        // Subscribe to events
        cursor.OnSelect += Cursor_OnSelect;
        cursor.OnCopy += Cursor_OnCopy;

        // Define transform modes
        transformModes = new TransformModeAndControls[] {
            new(TransformMode.Position, $"{Data.switchControl}Move <sprite=6>    Up / Down <sprite=9>"),
            new(TransformMode.Scale, $"{Data.switchControl}Max Range <sprite=8>    Min Range <sprite=8>"),
            new(TransformMode.Volume, $"{Data.switchControl}Volume <sprite=8>")
        };
    }

    /// <summary>
    /// Set up the audio from an Audio struct
    /// </summary>
    /// <param name="audio"></param>
    public void Setup(Audio audio)
    {
        myAudio = audio;

        audioSource.clip = audio.audioClip;
        Play();
    }

    /// <summary>
    /// Set up the audio from Scene Description Audio
    /// </summary>
    /// <param name="audio"></param>
    /// <param name="sdAudio"></param>
    public void Setup(Audio audio, SDAudio sdAudio)
    {
        myAudio = audio;

        transform.position = sdAudio.position;

        audioSource.volume = sdAudio.volume;
        audioSource.spatialBlend = sdAudio.spatialBlend;
        audioSource.minDistance = sdAudio.minDistance;
        audioSource.maxDistance = sdAudio.maxDistance;

        audioSource.clip = audio.audioClip;
        Play();
    }

    /// <summary>
    /// Executed when this object is selected. Selects the object
    /// </summary>
    private void Cursor_OnSelect(Transform selection, Vector3 selectionPoint)
    {
        if (selection == transform)
        {
            cursor.SelectObject(transform, transformModes, transform);
        }
    }

    /// <summary>
    /// Copies this object
    /// </summary>
    private void Cursor_OnCopy(Transform selection)
    {
        if (selection == transform)
        {
            ModelCache.Instance.Copy(gameObject);
        }
    }

    void Update()
    {
        // Hide if not in edit mode
        editorRenderer.enabled = cursor.EditMode;

        if (cursor.SelectedObject == transform && !keyboardInput.InMenu)
        {
            // Show visualizers
            bool showDistanceVisualizers = cursor.CursorTransformMode == TransformMode.Scale && audioSource.spatialBlend == 1;
            minDistanceVisualizer.gameObject.SetActive(showDistanceVisualizers);
            maxDistanceVisualizer.gameObject.SetActive(showDistanceVisualizers);
            volumeVisualizer.gameObject.SetActive(cursor.CursorTransformMode == TransformMode.Volume);

            // Handle transform modes
            switch (cursor.CursorTransformMode)
            {
                case TransformMode.Position:
                    cursor.Position(transform);

                    break;

                case TransformMode.Scale:
                    // Change min and max ranges
                    float minDistanceInput = getReal3D.Input.GetAxis(Inputs.rightStickY);
                    float maxDistanceInput = getReal3D.Input.GetAxis(Inputs.leftStickY);

                    float scaleSpeed = 12 * CurrentOptions.options.scaleSpeed;

                    audioSource.minDistance += minDistanceInput * scaleSpeed * getReal3D.Cluster.deltaTime;
                    audioSource.maxDistance += maxDistanceInput * scaleSpeed * getReal3D.Cluster.deltaTime;

                    audioSource.maxDistance = Mathf.Max(audioSource.maxDistance, audioSource.minDistance + 0.01f);

                    // Visualize ranges
                    minDistanceVisualizer.localScale = Vector3.one * audioSource.minDistance * 2;
                    maxDistanceVisualizer.localScale = Vector3.one * audioSource.maxDistance * 2;

                    // Toggle spatial blend
                    if (getReal3D.Input.GetButtonDown(Inputs.rightShoulder))
                    {
                        audioSource.spatialBlend = audioSource.spatialBlend == 0 ? 1 : 0;
                    }

                    break;

                case TransformMode.Volume:
                    // Change volume
                    float volumeInput = getReal3D.Input.GetAxis(Inputs.leftStickY);

                    float volumeSpeed = 4 * CurrentOptions.options.scaleSpeed;

                    audioSource.volume += volumeInput * volumeSpeed * getReal3D.Cluster.deltaTime;

                    // Visualize volume
                    volumeVisualizer.localScale = Vector3.one * audioSource.volume;

                    break;
            }
        }
        else
        {
            minDistanceVisualizer.gameObject.SetActive(false);
            maxDistanceVisualizer.gameObject.SetActive(false);
            volumeVisualizer.gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        // Ensure audio gets played on enable
        Play();
    }

    private void Play()
    {
        if (audioSource.clip != null)
        {
            audioSource.Play();
            audioSource.time = Random.Range(0, audioSource.clip.length - 0.1f); // Start audio at random point
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        cursor.OnSelect -= Cursor_OnSelect;
        cursor.OnCopy -= Cursor_OnCopy;
    }
}
