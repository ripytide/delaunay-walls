using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ballController : MonoBehaviour
{
    public bool explosive;
    public float range;
    public float power;
    public LayerMask affected;
    public GameObject effect;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.y < -10)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (explosive)
        {
            Vector3 explosionPosition = transform.position;

            Collider[] nearbyObjects = Physics.OverlapSphere(explosionPosition, range, affected);

            foreach (Collider item in nearbyObjects)
            {
                Rigidbody rb = item.GetComponent<Rigidbody>();

                if (rb != null)
                {
                    rb.AddExplosionForce(power, explosionPosition, range);
                }
            }

            Instantiate(effect, transform.position, Quaternion.identity);

            Destroy(gameObject, Random.Range(0, 1));

        }
    }
}
