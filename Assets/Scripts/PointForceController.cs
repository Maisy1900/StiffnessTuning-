using UnityEngine;
using NaughtyAttributes;
#if UNITY_EDITOR
using UnityEditor;
using System.Collections.Generic;
#endif

[ExecuteAlways]
public class PointForceController : MonoBehaviour
{
    [Header("Force Settings")]
    [Range(0f, 100f)]
    public float forceMagnitude = 10f;

    [Tooltip("Direction of the force to apply")]
    public Vector3 forceDirection = Vector3.up;

    [Header("Force Application Point")]
    [Dropdown("forcePointModeOptions")]
    [OnValueChanged("UpdateForcePoint")]
    public ForcePointMode forcePointMode = ForcePointMode.Center;

    [ShowIf("IsCustomForcePoint")]
    [Tooltip("Custom force point in local coordinates")]
    public Vector3 customForcePoint = Vector3.zero;

    [ShowIf("IsTopForcePoint")]
    [Tooltip("Offset from the top face (X,Z)")]
    public Vector2 topFaceOffset = Vector2.zero;

    [ShowIf("IsTopForcePoint")]
    [Tooltip("Height offset from the top face")]
    [Range(0f, 2f)]
    public float heightOffset = 0.1f;

    [Foldout("Debug Visualization")]
    [ColorUsage(true)]
    public Color gizmoColor = new Color(1f, 0.3f, 0.3f, 0.8f);

    [Foldout("Debug Visualization")]
    [Range(0.01f, 0.5f)]
    public float gizmoSize = 0.1f;

    // Debug values
    [Foldout("Runtime Debug")]
    [ReadOnly]
    public Vector3 currentWorldForcePoint;

    [Foldout("Runtime Debug")]
    [ReadOnly]
    public Vector3 actualForceVector;

    private Rigidbody rb;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Bounds objectBounds;
    private bool isInitialized = false;

#if UNITY_EDITOR
    // Static flag to track if we're currently testing physics
    private static bool isPhysicsTestActive = false;
    private static int simulationFrameCount = 0;

    // Store original state of all objects
    private class RbState
    {
        public Vector3 position;
        public Quaternion rotation;
        public bool isKinematic;
        public Vector3 velocity;
        public Vector3 angularVelocity;
    }

    private static Dictionary<Rigidbody, RbState> originalState = new Dictionary<Rigidbody, RbState>();
#endif

    // Define ways to apply force
    public enum ForcePointMode
    {
        Center,    // Apply force at the center of the object
        Top,       // Apply force at the top of the object
        Custom     // Apply force at a custom point
    }

    private DropdownList<ForcePointMode> forcePointModeOptions = new DropdownList<ForcePointMode>()
    {
        { "Center of Object", ForcePointMode.Center },
        { "Top of Object", ForcePointMode.Top },
        { "Custom Point", ForcePointMode.Custom }
    };

    private bool IsCustomForcePoint() => forcePointMode == ForcePointMode.Custom;
    private bool IsTopForcePoint() => forcePointMode == ForcePointMode.Top;

    private void Awake()
    {
        InitializeComponent();
    }

    private void OnEnable()
    {
        if (!isInitialized)
        {
            InitializeComponent();
        }
    }

    private void InitializeComponent()
    {
        // Get or add a Rigidbody
        rb = GetComponent<Rigidbody>();
        if (rb == null && Application.isPlaying)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            Debug.Log("Rigidbody component was missing and has been added.", this);
        }

        // Store initial state
        SaveInitialState();

        // Calculate object bounds
        CalculateBounds();

        // Update the force application point
        UpdateForcePoint();

        isInitialized = true;
    }

    private void SaveInitialState()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }

    private void CalculateBounds()
    {
        // Get the renderer bounds
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            objectBounds = renderer.bounds;
        }
        else
        {
            // Fallback if no renderer
            Collider collider = GetComponent<Collider>();
            if (collider != null)
            {
                objectBounds = collider.bounds;
            }
            else
            {
                // Default to a simple bounds centered on the object
                objectBounds = new Bounds(transform.position, Vector3.one);
            }
        }
    }

    [Button("Recalculate Force Point")]
    public void UpdateForcePoint()
    {
        Vector3 localForcePoint = Vector3.zero;

        switch (forcePointMode)
        {
            case ForcePointMode.Center:
                // Convert world center to local space
                CalculateBounds();
                localForcePoint = transform.InverseTransformPoint(objectBounds.center);
                break;

            case ForcePointMode.Top:
                CalculateBounds();
                // Calculate the top center of the bounds in local space
                Vector3 localTopCenter = transform.InverseTransformPoint(
                    new Vector3(
                        objectBounds.center.x,
                        objectBounds.max.y,
                        objectBounds.center.z
                    )
                );

                // Apply the offset to the top center
                localForcePoint = new Vector3(
                    localTopCenter.x + topFaceOffset.x,
                    localTopCenter.y + heightOffset,
                    localTopCenter.z + topFaceOffset.y
                );
                break;

            case ForcePointMode.Custom:
                localForcePoint = customForcePoint;
                break;
        }

        // Only update the custom point if not in custom mode
        if (forcePointMode != ForcePointMode.Custom)
        {
            customForcePoint = localForcePoint;
        }

        // Calculate the world force point for debugging and visualization
        currentWorldForcePoint = transform.TransformPoint(
            forcePointMode == ForcePointMode.Custom ? customForcePoint : localForcePoint
        );

        // Update force vector
        actualForceVector = forceDirection.normalized * forceMagnitude;
    }

    // This runs in the editor and continuously during play
    private void OnValidate()
    {
        if (!isInitialized)
        {
            InitializeComponent();
        }
        else
        {
            CalculateBounds();
            UpdateForcePoint();
        }

        // Normalize the force direction
        if (forceDirection != Vector3.zero)
        {
            forceDirection.Normalize();
        }

        // Only check for inputs in Play mode
        if (Application.isPlaying)
        {
            // Check for F key press to apply force
            if (Input.GetKey(KeyCode.F))
            {
                ApplyPointForce();
            }

            // Check for Spacebar press to reset
            if (Input.GetKey(KeyCode.Space))
            {
                ResetObject();
            }
        }
    }

    // Regular Update for input handling
    private void Update()
    {
        if (Application.isPlaying)
        {
            // Apply force when F key is pressed
            if (Input.GetKeyDown(KeyCode.F))
            {
                ApplyPointForce();
            }

            // Reset when spacebar is pressed
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ResetObject();
            }
        }
        else
        {
            // Always update the force point in Edit mode
            UpdateForcePoint();
        }
    }

    [Button("Apply Force")]
    public void ApplyPointForce()
    {
        if (rb != null)
        {
            UpdateForcePoint();

            // In Play mode: use regular physics
            if (Application.isPlaying)
            {
                rb.AddForceAtPosition(forceDirection.normalized * forceMagnitude, currentWorldForcePoint, ForceMode.Impulse);
                Debug.Log($"Applied force of {forceMagnitude} in direction {forceDirection}", this);
            }
            // In Edit mode: start physics test
            else
            {
#if UNITY_EDITOR
                if (!isPhysicsTestActive)
                {
                    StartPhysicsTest();
                }

                rb.isKinematic = false;
                // Apply more force in Edit mode to overcome potential damping
                float editModeMultiplier = 2.0f; // Apply double force in Edit mode
                rb.AddForceAtPosition(forceDirection.normalized * forceMagnitude * editModeMultiplier, currentWorldForcePoint, ForceMode.Impulse);

                // Make sure the rigidbody is awake and will process physics
                rb.WakeUp();

                // Print debug info including the current position and velocity
                Debug.Log($"Applied force of {forceMagnitude * editModeMultiplier} in direction {forceDirection}. Position: {rb.position}, Velocity: {rb.linearVelocity}", this);

                // Force immediate physics update
                Physics.Simulate(0.02f);
                EditorApplication.QueuePlayerLoopUpdate();
#endif
            }
        }
    }

    [Button("Reset Position")]
    public void ResetObject()
    {
        if (rb != null)
        {
            // In Play mode: reset normally
            if (Application.isPlaying)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                transform.position = initialPosition;
                transform.rotation = initialRotation;
                Debug.Log("Object reset to initial state", this);
            }
            // In Edit mode: restore from original state
            else
            {
#if UNITY_EDITOR
                StopPhysicsTest();
#endif
            }
        }
    }

#if UNITY_EDITOR
    [Button("Start Physics Test")]
    public void StartPhysicsTest()
    {
        if (!Application.isPlaying && !isPhysicsTestActive)
        {
            // Reset frame counter
            simulationFrameCount = 0;

            // Store original state of all Rigidbodies in the scene
            originalState.Clear();
            foreach (Rigidbody sceneRb in FindObjectsOfType<Rigidbody>())
            {
                RbState state = new RbState();
                state.position = sceneRb.transform.position;
                state.rotation = sceneRb.transform.rotation;
                state.isKinematic = sceneRb.isKinematic;
                state.velocity = sceneRb.linearVelocity;
                state.angularVelocity = sceneRb.angularVelocity;

                originalState[sceneRb] = state;

                // Make sure objects can move
                sceneRb.isKinematic = false;

                // Ensure constraints don't block expected movement
                if (sceneRb.constraints == RigidbodyConstraints.FreezeAll)
                {
                    sceneRb.constraints = RigidbodyConstraints.None;
                }

                // Reset velocities to make sure previous state doesn't interfere
                sceneRb.linearVelocity = Vector3.zero;
                sceneRb.angularVelocity = Vector3.zero;

                // Enable gravity explicitly
                sceneRb.useGravity = true;
            }

            // Configure physics settings to be more suitable for Edit mode
            float originalGravity = Physics.gravity.y;
            Physics.gravity = new Vector3(0, -9.81f, 0);

            // Start physics updates in edit mode with high frequency
            isPhysicsTestActive = true;
            EditorApplication.update += UpdatePhysics;

            Debug.Log("Started physics test in Edit mode - Objects should now respond to forces and gravity");
        }
    }

    [Button("Stop Physics Test")]
    public void StopPhysicsTest()
    {
        if (!Application.isPlaying && isPhysicsTestActive)
        {
            // Stop physics updates
            EditorApplication.update -= UpdatePhysics;
            isPhysicsTestActive = false;

            // Restore all objects to their original state
            foreach (var entry in originalState)
            {
                if (entry.Key != null)
                {
                    Rigidbody rb = entry.Key;
                    RbState state = entry.Value;

                    rb.transform.position = state.position;
                    rb.transform.rotation = state.rotation;
                    rb.isKinematic = state.isKinematic;
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
            }

            originalState.Clear();

            Debug.Log("Stopped physics test and reset all objects");
        }
    }

    private void UpdatePhysics()
    {
        if (isPhysicsTestActive)
        {
            // Simulate with a fixed timestep for more stable physics
            float fixedTimeStep = 0.02f; // 50 physics updates per second

            // Manually step the physics simulation with a fixed timestep
            Physics.Simulate(fixedTimeStep);

            // Force Unity to update the scene view
            SceneView.RepaintAll();

            // Force Unity to update the transforms
            foreach (var entry in originalState)
            {
                if (entry.Key != null)
                {
                    // This helps Unity register the transform changes
                    entry.Key.transform.hasChanged = true;
                }
            }

            // Log simulation status periodically to verify it's running
            simulationFrameCount++;
            if (simulationFrameCount % 100 == 0)
            {
                Debug.Log($"Physics simulation running: frame {simulationFrameCount}");
            }
        }
    }
#endif

    // Visual debugging
    private void OnDrawGizmosSelected()
    {
        // Update for visualization in editor
        if (!isInitialized || !Application.isPlaying)
        {
            CalculateBounds();
            UpdateForcePoint();
        }

        Gizmos.color = gizmoColor;

        // Draw a sphere at the point where force will be applied
        Gizmos.DrawSphere(currentWorldForcePoint, gizmoSize);

        // Draw a line showing the force direction and magnitude
        Gizmos.DrawRay(currentWorldForcePoint, forceDirection.normalized * forceMagnitude * 0.05f);
    }
}