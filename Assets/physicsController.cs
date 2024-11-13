using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class physicsController : MonoBehaviour
{
    Rigidbody cube; 
    // Start is called before the first frame update
    void Start()
    {
        cube = GetComponent<Rigidbody>();

        cube.AddForce(new Vector3(0 ,0 ,50) , ForceMode.Impulse);//applies force of 5n along the xasis at the centre of the cube.
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
