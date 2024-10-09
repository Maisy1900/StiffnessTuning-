using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetCube : MonoBehaviour
{
    private Vector3 default_position;
    private Quaternion default_rotation;
    private Rigidbody r;

    // Start is called before the first frame update
    void Start()
    {
        default_position = transform.position;
        default_rotation = transform.rotation;
        r = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            transform.position = default_position;
            transform.rotation = default_rotation;
            r.velocity = Vector3.zero;
            r.angularVelocity = Vector3.zero;
        }
        
    }
}
