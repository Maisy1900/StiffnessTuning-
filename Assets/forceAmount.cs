using UnityEngine;

public class ApplyWeightForce : MonoBehaviour
{
    private ArticulationBody articulationBody;

    public float massInKg = 1f;

    private float gravity = 9.81f;

    public string type = "up";
    Vector3 force; 

    void Start()
    {

        articulationBody = GetComponent<ArticulationBody>();
    }

    void Update()
    {
        ApplyWeight();
        getForce(type);
    }

    // Function to apply weight force based on mass
    void ApplyWeight()
    {

        float weightForce = massInKg * gravity;
        articulationBody.AddForce(Vector3.down * weightForce);   
    }
    void getForce(string type)
    {
        if( type == "up")
        {
            force = Vector3.up;
        }
        if( type == "down")
        {
            force = Vector3.down;
        }
        if(type == "left")
        {
            force = Vector3.left;
        }
        if(type == "right")
        {
            force = Vector3.up;
        }
    }
}
