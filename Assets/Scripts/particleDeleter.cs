using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class particleDeleter : MonoBehaviour
{
    private ParticleSystem parts;
    // Start is called before the first frame update
    void Start()
    {
        parts = gameObject.GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!parts.IsAlive())
        {
            Destroy(gameObject);
        }
    }
}
