using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bodyController : MonoBehaviour
{

    public pauseMenuController pauseMenu;


    public CharacterController controller;
    public float speed = 10;
    public float mouseSpeed = 1;
    private float yRotation;

    public float gravity = -9.81f;

    public Vector3 velocity;
    public bool isGrounded;

    public Transform groundCheck;
    public float groundDistance = 0.5f;
    public LayerMask groundMask;
   
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
        //movement
        float hori = Input.GetAxis("Horizontal");
        float verti = Input.GetAxis("Vertical");

        Vector3 move = (transform.right * hori + transform.forward * verti);

        controller.Move(move * speed);

        //gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        //ground check
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        //horizontal rotation
        float mouseX = Input.GetAxis("Mouse X") * mouseSpeed * 0.01f;
        yRotation -= mouseX;
        transform.Rotate(Vector3.up * mouseX);
    }
}
