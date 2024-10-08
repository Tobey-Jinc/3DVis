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

    private ObjectCursor cursor;
    private KeyboardInput keyboardInput;

    private TransformMode[] transformModes;

    public Audio MyAudio { get; private set; }

    public AudioSource AudioSource { get => audioSource; }

    void Start()
    {
        cursor = ObjectCursor.Instance;
        keyboardInput = KeyboardInput.Instance;

        cursor.OnSelect += Cursor_OnSelect;

        transformModes = new[] { TransformMode.Position, TransformMode.Scale, TransformMode.Volume };
    }

    public void Setup(Audio audio)
    {
        MyAudio = audio;

        audioSource.clip = audio.audioClip;
        audioSource.Play();
    }

    private void Cursor_OnSelect(Transform selection, Vector3 selectionPoint)
    {
        cursor.SelectObject(transform, transformModes, transform);
    }

    void Update()
    {
        if (cursor.SelectedObject == transform && !keyboardInput.InMenu)
        {
            bool showDistanceVisualizers = cursor.CursorTransformMode == TransformMode.Scale && audioSource.spatialBlend == 1;
            minDistanceVisualizer.gameObject.SetActive(showDistanceVisualizers);
            maxDistanceVisualizer.gameObject.SetActive(showDistanceVisualizers);

            switch (cursor.CursorTransformMode)
            {
                case TransformMode.Position:
                    cursor.Position(transform);

                    break;

                case TransformMode.Scale:
                    float minDistanceInput = getReal3D.Input.GetAxis(Inputs.rightStickY);
                    float maxDistanceInput = getReal3D.Input.GetAxis(Inputs.leftStickY);

                    audioSource.minDistance += minDistanceInput * 6 * getReal3D.Cluster.deltaTime;
                    audioSource.maxDistance += maxDistanceInput * 6 * getReal3D.Cluster.deltaTime;

                    audioSource.maxDistance = Mathf.Max(audioSource.maxDistance, audioSource.minDistance + 0.01f);

                    minDistanceVisualizer.localScale = Vector3.one * audioSource.minDistance * 2;
                    maxDistanceVisualizer.localScale = Vector3.one * audioSource.maxDistance * 2;

                    if (getReal3D.Input.GetButtonDown(Inputs.rightShoulder))
                    {
                        audioSource.spatialBlend = audioSource.spatialBlend == 0 ? 1 : 0;
                    }

                    break;

                case TransformMode.Volume:
                    float volumeInput = getReal3D.Input.GetAxis(Inputs.leftStickY);

                    audioSource.volume += volumeInput * 2 * getReal3D.Cluster.deltaTime;

                    volumeVisualizer.localScale = Vector3.one * audioSource.volume;

                    break;
            }

            if (getReal3D.Input.GetButtonDown(Inputs.y))
            {
                
            }
        }
        else
        {
            minDistanceVisualizer.gameObject.SetActive(false);
            maxDistanceVisualizer.gameObject.SetActive(false);
        }
    }
}
