using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.WSA;

public class fragmentActivator : MonoBehaviour
{
    public float dist = 2f;
    private bool activate = false;
    public void Start()
    {
    }

    private void Update()
    {
        GameObject[] balls = GameObject.FindGameObjectsWithTag("ball");

        activate = false;

        foreach (GameObject ball in balls)
        {
            if ((Vector3.Distance(gameObject.GetComponent<MeshCollider>().ClosestPoint(ball.GetComponent<Transform>().position), ball.GetComponent<Transform>().position) < dist))
            {
                activate = true;
            }
        }

        if (activate)
        {
            gameObject.GetComponent<Rigidbody>().isKinematic = false;
        }
        else
        {
            gameObject.GetComponent<Rigidbody>().isKinematic = true;
        }
    }
}
