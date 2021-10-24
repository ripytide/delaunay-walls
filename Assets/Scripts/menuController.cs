using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class menuController : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject optionsMenu;
    public GameObject luv;
    public GameObject cool;
    public Transform source;



    public float XcoolLB;
    public float XcoolUB;

    public float YcoolLB;
    public float YcoolUB;


    public float XluvLB;
    public float XluvUB;

    public float YluvLB;
    public float YluvUB;








    public void Start()
    {
        optionsMenu.SetActive(false);
    }
    public void MainMenu()
    {
        optionsMenu.SetActive(false);
        mainMenu.SetActive(true);
    }
    public void OptionsMenu()
    {
        ClearEffects();

        mainMenu.SetActive(false);
        optionsMenu.SetActive(true);
    }
    public void PlayGame()
    {
        SceneManager.LoadScene("game");
    }
    public void QuitGame()
    {
        Application.Quit();
    }

    public void MakeEffect()
    {
        if (Random.Range(0f, 1f) < 0.98)
        {
            GameObject newEffect = Instantiate(cool, source);
            Rigidbody2D newEffectRb = newEffect.GetComponent<Rigidbody2D>();

            newEffectRb.velocity = new Vector2(Random.Range(XcoolLB, XcoolUB), Random.Range(YcoolLB, YcoolUB));

            Destroy(newEffect, 10f);
        }
        else
        {
            GameObject newEffect = Instantiate(luv, source);
            Rigidbody2D newEffectRb = newEffect.GetComponent<Rigidbody2D>();

            newEffectRb.velocity = new Vector2(Random.Range(XluvLB, XluvUB), Random.Range(YluvLB, YluvUB));

            Destroy(newEffect, 10f);
        }

    }

    public void ClearEffects()
    {
        for (int i = 0; i < source.childCount; i++)
        {
            Destroy(source.GetChild(i).gameObject);
        }
    }
}
