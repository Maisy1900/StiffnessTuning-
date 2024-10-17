using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleController : MonoBehaviour
{
    public ArticulationBody joint_1;
    public Transform joint_1_driver;
    public Rigidbody cube;

    private float startTime;
    private float finalTime; // to store the time when max velocity is reached
    private bool hasStarted = false;
    private float lastVelocity = 0f;
    private float velocityThreshold = 0.001f; // smaller threshold for stability check
    private bool reachedMaxVelocity = false;
    private float initialVelocity = 0f;
    private float mass = 1f; // mass of the object in kg

    void FixedUpdate() // switching to FixedUpdate for more accurate physics
    {
        Vector3 currentVelocity = cube.velocity;

        // check if movement has started
        if (!hasStarted && currentVelocity.magnitude > 0)
        {
            // start time when movement begins
            startTime = Time.time;
            hasStarted = true;
            print("movement started at time " + startTime);
        }

        // move the joint to match the target position
        ArticulationDrive jointDrive = joint_1.xDrive;
        jointDrive.target = joint_1_driver.localEulerAngles.z;
        joint_1.xDrive = jointDrive;

        // check when the velocity stops changing (max velocity reached)
        if (hasStarted && !reachedMaxVelocity)
        {
            float velocityChange = Mathf.Abs(currentVelocity.magnitude - lastVelocity);

            // if velocity change is small enough we hit max velocity
            if (velocityChange < velocityThreshold)
            {
                reachedMaxVelocity = true;
                float timeToMaxVelocity = Time.time - startTime;
                finalTime = Time.time; // capture the final time when max velocity is reached

                // calculate acceleration using (v_f - v_i) / t
                float finalVelocity = currentVelocity.magnitude; // assuming this is max velocity
                float acceleration = (finalVelocity - initialVelocity) / timeToMaxVelocity;

                // calculate force using F = m * a
                float force = mass * acceleration;

                // print time, velocity, calculated acceleration, and force
                print("max velocity reached in " + timeToMaxVelocity + " seconds");
                print("final velocity " + finalVelocity + " m/s");
                print("acceleration " + acceleration + " m/s^2");
                print("force " + force + " N");
                print("final time " + finalTime + " seconds"); // printing the final time

                // print xDrive properties target and velocity
                print("xDrive target " + jointDrive.target);
            }

            // update last velocity for next frame
            lastVelocity = currentVelocity.magnitude;
        }
    }
}