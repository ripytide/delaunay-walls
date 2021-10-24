using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class projectileShooter : MonoBehaviour
{
    public pauseMenuController pauseMenu;

    public GameObject projectile;
    public GameObject projectileExplosive;
    private GameObject currentProjectile;
    public int power = 50;
    private bool explosive;
    public bool explosibeEnabled;

    // Start is called before the first frame update
    void Start()
    {
        currentProjectile = projectile;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("fire"))
        {
            if (!pauseMenu.paused)
            {
                GameObject projectileInstance = Instantiate(currentProjectile, transform.position, transform.rotation);

                projectileInstance.GetComponent<Rigidbody>().velocity = power * transform.forward;
            }
        }
    }

    public void setExplosivity(bool value)
    {
        if (value && explosibeEnabled)
        {
            currentProjectile = projectileExplosive;
        }
        else
        {
            currentProjectile = projectile;
        }
    }
}
