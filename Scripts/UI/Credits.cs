using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Credits : MonoBehaviour
{
    [SerializeField] private GameObject[] pages;
    [SerializeField] private Canvas canvas;
    [SerializeField] private KeyboardInput keyboardInput;
    [SerializeField] private GameObject closedControls;
    [SerializeField] private GameObject openControls;

    private int page;

    void Update()
    {
        // Only open credits when not in Keyboard Input menu, and only on the master node
        if (getReal3D.Cluster.isMaster && !keyboardInput.InMenu)
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                canvas.enabled = !canvas.enabled;
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                canvas.enabled = false;
            }
        }
        else
        {
            canvas.enabled = false;
        }

        if (canvas.enabled)
        {
            // Show correct controls
            closedControls.SetActive(false);
            openControls.SetActive(true);

            // Navigation
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                page--;
            }
            else if(Input.GetKeyDown(KeyCode.RightArrow))
            {
                page++;
            }

            // Wrap navigation
            if (page < 0)
            {
                page = pages.Length - 1;
            }
            else if (page >= pages.Length)
            {
                page = 0;
            }

            // Show current page
            for (int i = 0; i < pages.Length; i++)
            {
                pages[i].SetActive(i == page);
            }
        }
        else
        {
            // Show correct controls
            closedControls.SetActive(true);
            openControls.SetActive(false);

            page = 0;
        }
    }
}
