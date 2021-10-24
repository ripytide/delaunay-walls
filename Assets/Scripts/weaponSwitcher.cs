using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class weaponSwitcher : MonoBehaviour
{
    public List<GameObject> weapons;

    private int currentWeapon = 0;

    public Dropdown dropdown;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("weapon1") && currentWeapon != 0)
        {
            SwitchWeapon(0);
        }
        if (Input.GetButtonDown("weapon2") && currentWeapon != 1)
        {
            SwitchWeapon(1);
        }
        if (Input.GetButtonDown("weapon3") && currentWeapon != 2)
        {
            SwitchWeapon(2);
        }
    }

    public void SwitchWeapon(int index)
    {
        weapons[currentWeapon].SetActive(false);
        weapons[index].SetActive(true);
        currentWeapon = index;

        dropdown.value = index;
    }
}
