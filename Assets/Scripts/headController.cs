using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class headController : MonoBehaviour
{

    public pauseMenuController pauseMenu;



    public float mouseSpeed = 800;
    public float minLimit = -45;
    public float maxLimit = 270;
    
    private float yRotation;


    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (!pauseMenu.paused)
        {
            DoMotion();
        }
    }

    void DoMotion()
    {
        //vertical looking
        float mouseY = Input.GetAxis("Mouse Y") * mouseSpeed;

        if (!((mouseY < 0 && yRotation < minLimit) || (mouseY > 0 && yRotation > maxLimit)))
        {
            yRotation += mouseY * 0.01f;
            transform.localRotation = Quaternion.Euler(Vector3.left * yRotation);
        }
    }
}
