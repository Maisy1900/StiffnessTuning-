using UnityEngine;
using UnityEngine.UI;

public class EnhancedRotationVisualizer : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The object to monitor rotation (leave empty to use this object)")]
    public Transform targetTransform;

    [Tooltip("Optional UI Text component to display rotation value")]
    public Text rotationText;

    [Header("Display Settings")]
    [Tooltip("Which rotation axis to monitor")]
    public RotationAxis rotationAxis = RotationAxis.Z;

    [Tooltip("Show rotation relative to starting rotation")]
    public bool showRelativeRotation = true;

    [Header("Visualization Settings")]
    [Tooltip("Length of axis visualization lines")]
    public float axisLength = 2.0f;

    [Tooltip("Width of the rotation arc")]
    [Range(0.01f, 0.2f)]
    public float arcWidth = 0.05f;

    [Tooltip("Radius of the rotation arc")]
    public float arcRadius = 1.0f;

    [Tooltip("Color for X axis")]
    public Color xAxisColor = Color.red;

    [Tooltip("Color for Y axis")]
    public Color yAxisColor = Color.green;

    [Tooltip("Color for Z axis")]
    public Color zAxisColor = Color.blue;

    [Tooltip("Color for rotation indicator")]
    public Color rotationColor = Color.yellow;

    // Private variables
    private Vector3 initialEulerAngles;
    private float timer;
    private GUIStyle guiStyle;
    private GameObject axisIndicators;
    private LineRenderer xAxisLine;
    private LineRenderer yAxisLine;
    private LineRenderer zAxisLine;
    private LineRenderer rotationArc;

    // Enum for axis selection
    public enum RotationAxis
    {
        X, Y, Z
    }

    private void Start()
    {
        // Set up target transform
        if (targetTransform == null)
        {
            targetTransform = transform;
            Debug.Log("No target transform assigned, using this object instead.", this);
        }

        // Store initial rotation
        initialEulerAngles = targetTransform.eulerAngles;

        // Set up GUI style for on-screen display
        guiStyle = new GUIStyle();
        guiStyle.fontSize = 24;
        guiStyle.normal.textColor = Color.white;
        guiStyle.fontStyle = FontStyle.Bold;
        guiStyle.alignment = TextAnchor.UpperLeft;
        guiStyle.normal.background = MakeTexture(2, 2, new Color(0f, 0f, 0f, 0.5f));
        guiStyle.padding = new RectOffset(10, 10, 5, 5);

        // Create visualization objects
        CreateAxisVisualization();
    }

    private void CreateAxisVisualization()
    {
        // Create parent object for visualization
        axisIndicators = new GameObject("AxisVisualization");
        axisIndicators.transform.SetParent(targetTransform, false);
        axisIndicators.transform.localPosition = Vector3.zero;
        axisIndicators.transform.localRotation = Quaternion.identity;

        // Create X axis line
        GameObject xAxis = new GameObject("X_Axis");
        xAxis.transform.SetParent(axisIndicators.transform, false);
        xAxisLine = xAxis.AddComponent<LineRenderer>();
        SetupLineRenderer(xAxisLine, xAxisColor, 0.05f);

        // Create Y axis line
        GameObject yAxis = new GameObject("Y_Axis");
        yAxis.transform.SetParent(axisIndicators.transform, false);
        yAxisLine = yAxis.AddComponent<LineRenderer>();
        SetupLineRenderer(yAxisLine, yAxisColor, 0.05f);

        // Create Z axis line
        GameObject zAxis = new GameObject("Z_Axis");
        zAxis.transform.SetParent(axisIndicators.transform, false);
        zAxisLine = zAxis.AddComponent<LineRenderer>();
        SetupLineRenderer(zAxisLine, zAxisColor, 0.05f);

        // Create rotation indicator
        GameObject rotationIndicator = new GameObject("Rotation_Indicator");
        rotationIndicator.transform.SetParent(axisIndicators.transform, false);
        rotationArc = rotationIndicator.AddComponent<LineRenderer>();
        SetupLineRenderer(rotationArc, rotationColor, arcWidth);
    }

    private void SetupLineRenderer(LineRenderer lineRenderer, Color color, float width)
    {
        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
        lineRenderer.useWorldSpace = false;
        lineRenderer.positionCount = 2; // Default to 2 points for axis lines
    }

    private void Update()
    {
        timer += Time.deltaTime;

        // Update visualization
        UpdateAxisLines();
        UpdateRotationArc();

        // Update UI text if available
        if (rotationText != null && timer >= 0.1f)
        {
            timer = 0f;
            rotationText.text = GetRotationString();
        }
    }

    private void UpdateAxisLines()
    {
        // X axis (red)
        xAxisLine.SetPosition(0, Vector3.zero);
        xAxisLine.SetPosition(1, Vector3.right * axisLength);

        // Y axis (green)
        yAxisLine.SetPosition(0, Vector3.zero);
        yAxisLine.SetPosition(1, Vector3.up * axisLength);

        // Z axis (blue)
        zAxisLine.SetPosition(0, Vector3.zero);
        zAxisLine.SetPosition(1, Vector3.forward * axisLength);
    }

    private void UpdateRotationArc()
    {
        float angle = GetCurrentAngle();
        int segments = Mathf.Max(2, Mathf.Abs(Mathf.RoundToInt(angle / 5.0f)) + 1);
        rotationArc.positionCount = segments;

        Vector3 arcDirection = Vector3.zero;
        Vector3 arcNormal = Vector3.zero;

        switch (rotationAxis)
        {
            case RotationAxis.X:
                arcDirection = Vector3.up;
                arcNormal = Vector3.right;
                break;
            case RotationAxis.Y:
                arcDirection = Vector3.forward;
                arcNormal = Vector3.up;
                break;
            case RotationAxis.Z:
                arcDirection = Vector3.right;
                arcNormal = Vector3.forward;
                break;
        }

        // Draw arc from 0 to current angle
        for (int i = 0; i < segments; i++)
        {
            float segmentAngle = (angle * i) / (segments - 1);
            Quaternion rotation = Quaternion.AngleAxis(segmentAngle, arcNormal);
            rotationArc.SetPosition(i, rotation * (arcDirection * arcRadius));
        }
    }

    private void OnGUI()
    {
        // Always display on screen if no UI Text is assigned
        if (rotationText == null)
        {
            GUI.Label(new Rect(20, 20, 300, 100), GetRotationString(), guiStyle);
        }
    }

    private string GetRotationString()
    {
        float currentAngle = GetCurrentAngle();
        string axisName = rotationAxis.ToString();
        return $"Rotation ({axisName}): {currentAngle:F2}°";
    }

    private float GetCurrentAngle()
    {
        Vector3 currentEuler = targetTransform.eulerAngles;
        float angle = 0f;

        // Get the raw angle from the selected axis
        switch (rotationAxis)
        {
            case RotationAxis.X:
                angle = currentEuler.x;
                break;
            case RotationAxis.Y:
                angle = currentEuler.y;
                break;
            case RotationAxis.Z:
                angle = currentEuler.z;
                break;
        }

        // Normalize the angle to -180 to 180 range
        if (angle > 180f) angle -= 360f;

        // Calculate relative angle if needed
        if (showRelativeRotation)
        {
            float initialAngle = 0f;
            switch (rotationAxis)
            {
                case RotationAxis.X:
                    initialAngle = initialEulerAngles.x;
                    break;
                case RotationAxis.Y:
                    initialAngle = initialEulerAngles.y;
                    break;
                case RotationAxis.Z:
                    initialAngle = initialEulerAngles.z;
                    break;
            }

            // Normalize initial angle
            if (initialAngle > 180f) initialAngle -= 360f;

            // Subtract initial angle
            angle -= initialAngle;

            // Ensure result is in -180 to 180 range
            if (angle > 180f) angle -= 360f;
            if (angle < -180f) angle += 360f;
        }

        return angle;
    }

    // Helper method to create a texture for GUI background
    private Texture2D MakeTexture(int width, int height, Color color)
    {
        Color[] pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = color;
        }

        Texture2D texture = new Texture2D(width, height);
        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }

    // Optional: Draw additional gizmos in editor
    private void OnDrawGizmos()
    {
        if (targetTransform == null) return;

        // Draw a sphere to indicate the center of rotation
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(targetTransform.position, 0.1f);

        // Draw selected axis
        Vector3 direction = Vector3.zero;
        switch (rotationAxis)
        {
            case RotationAxis.X:
                Gizmos.color = xAxisColor;
                direction = targetTransform.right;
                break;
            case RotationAxis.Y:
                Gizmos.color = yAxisColor;
                direction = targetTransform.up;
                break;
            case RotationAxis.Z:
                Gizmos.color = zAxisColor;
                direction = targetTransform.forward;
                break;
        }

        Gizmos.DrawRay(targetTransform.position, direction * axisLength * 1.2f);
    }
}