using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pauseMenuController : MonoBehaviour
{
    public GameObject pauseMenu;
    public bool paused = false;

    public mouseController mouseController;




    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            if (paused)
            {
                paused = false;
                Unpause();
            }
            else
            {
                paused = true;
                Pause();
            }
        }
    }

    void Pause()
    {
        Time.timeScale = 0f;
        pauseMenu.SetActive(true);

        mouseController.ShowCursor();
    }
    void Unpause()
    {
        Time.timeScale = 1f;
        pauseMenu.SetActive(false);

        mouseController.HideCursor();
    }
}
