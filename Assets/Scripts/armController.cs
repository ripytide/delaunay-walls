using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class armController : MonoBehaviour
{

    public Transform head;
    public float buffer = 0f;
    public float scaleFactor = 1f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float targetAngle = head.eulerAngles.x;

        targetAngle = (targetAngle > 180) ? targetAngle - 360 : targetAngle;

        targetAngle = scaleFactor * (targetAngle + buffer);

        transform.localRotation = Quaternion.Euler(Vector3.right * (targetAngle * scaleFactor));
        
    }
}
