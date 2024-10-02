using UnityEngine;

public class ApplyWeightForce : MonoBehaviour
{
    private ArticulationBody articulationBody;

    public float massInKg = 1f;

    private float gravity = 9.81f;

    void Start()
    {

        articulationBody = GetComponent<ArticulationBody>();
    }

    void Update()
    {
        ApplyWeight();
    }

    // Function to apply weight force based on mass
    void ApplyWeight()
    {

        float weightForce = massInKg * gravity;
        articulationBody.AddForce(Vector3.down * weightForce);
    }
}
