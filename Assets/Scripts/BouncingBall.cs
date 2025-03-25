using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BounceTracker : MonoBehaviour
{
    // Configuration
    public float bounceThreshold = 0.05f; // Minimum velocity to count as a bounce
    public float stopThreshold = 0.01f;   // Velocity below which we consider the ball stopped

    // Tracking variables
    private bool hasFirstContact = false;
    private float firstContactTime = 0f;
    private float startTime = 0f;
    private int bounceCount = 0;
    private float stopTime = 0f;
    private bool hasStopped = false;

    // References
    private Rigidbody rb;
    private bool isRising = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        startTime = Time.time;

        // Log initial setup
        Debug.Log("Tracking started. Waiting for first contact...");
    }

    void Update()
    {
        // Skip if already stopped
        if (hasStopped) return;

        // Check if ball has stopped bouncing
        if (hasFirstContact && !hasStopped && rb.linearVelocity.magnitude < stopThreshold && transform.position.y <= transform.localScale.y * 0.6f)
        {
            hasStopped = true;
            stopTime = Time.time;
            float totalTime = stopTime - startTime;

            Debug.Log($"<color=yellow>Ball stopped bouncing:</color>");
            Debug.Log($"  • Total bounce count: {bounceCount}");
            Debug.Log($"  • Time of first contact: {firstContactTime - startTime:F3} seconds");
            Debug.Log($"  • Total experiment time: {totalTime:F3} seconds");
            Debug.Log($"  • Time until stopping after first contact: {stopTime - firstContactTime:F3} seconds");
        }

        // Track direction changes for bounce detection
        bool isRisingNow = rb.linearVelocity.y > 0;

        // Detect bounce (when direction changes from down to up)
        if (!isRising && isRisingNow && hasFirstContact && rb.linearVelocity.magnitude > bounceThreshold)
        {
            bounceCount++;
            Debug.Log($"Bounce #{bounceCount} detected at {Time.time - startTime:F3} seconds");
        }

        isRising = isRisingNow;
    }

    void OnCollisionEnter(Collision collision)
    {
        // Detect first contact with any surface
        if (!hasFirstContact)
        {
            hasFirstContact = true;
            firstContactTime = Time.time;
            Debug.Log($"<color=green>First contact detected at {firstContactTime - startTime:F3} seconds</color>");
        }
    }
}