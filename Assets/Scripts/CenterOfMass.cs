using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public class CenterOfMass : MonoBehaviour
{
    [Header("Visualisation Settinggs")]
    [SerializeField] private bool showCenterOfMass = true;
    [SerializeField] private Color comColor = Color.red;
    [SerializeField] private float comRadius = 0.1f;


    [Header("Custom Center of Mass")]
    [SerializeField] private bool userCustomCenterOfMass = false;
    [SerializeField] private Vector3 customCenterOfMass = Vector3.zero;

    // internal references
    private Rigidbody rb;
    private GameObject comVisualiser;
    void Start()
    {
        //get rigidbody reference
        rb = GetComponent<Rigidbody>();

        // create center of mass visualiser
        CreateCOMVisualiser();

        if (userCustomCenterOfMass) 
        {
        
        rb.centerOfMass = customCenterOfMass;
        
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (showCenterOfMass && comVisualiser != null)
        {
            // update visualiser position  to match the
            comVisualiser.transform.position = rb.worldCenterOfMass;
        }

        comVisualiser.SetActive(showCenterOfMass);
    }



    private void CreateCOMVisualiser()
    {
        comVisualiser = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        comVisualiser.name = "CenterOfMass_" + gameObject.name;

        comVisualiser.transform.localScale = Vector3.one * comRadius * 2;
        comVisualiser.transform.position = rb.worldCenterOfMass;

        Destroy(comVisualiser.GetComponent<Collider>());
        if(comVisualiser.TryGetComponent<Rigidbody>(out Rigidbody comRb))
        {
            Destroy(comRb);
        }
        //set maetrial color
        Renderer renderer = comVisualiser.GetComponent<Renderer>();
        if (renderer != null) 
        {
            renderer.material = new Material(Shader.Find("Standard"));
            renderer.material.color = comColor;
            renderer.material.SetFloat("_Matallic", 0.8f);
            renderer.material.SetFloat("_Glossiness", 0.8f);
        
        }
    }

    private void OnDrawGizmos()
    {
        
        if (!Application.isPlaying && GetComponent<Rigidbody>() != null)
        {
            Rigidbody rbGizmo = GetComponent<Rigidbody>();

            Vector3 com = userCustomCenterOfMass ? customCenterOfMass : rbGizmo.centerOfMass;
        
            Gizmos.color = comColor;
            Gizmos.DrawSphere(transform.TransformPoint(com), comRadius);
        
        }
    }

    public Vector3 GetCenterOfMass()
    {
        return rb != null ? rb.centerOfMass : Vector3.zero;
    }

    public void SetCustomCenterOfMass(Vector3 newCOM)
    {
        customCenterOfMass = newCOM;
        userCustomCenterOfMass = true;

        if (rb != null)
        {
            rb.centerOfMass = customCenterOfMass;
        }
    }

    public void ResetToDefaultCenterOfMass()
    {
        userCustomCenterOfMass = false;
        if(rb != null)
        {
            rb.ResetCenterOfMass();
        }
    }
}
