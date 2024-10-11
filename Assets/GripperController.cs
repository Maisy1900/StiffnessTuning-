using System;
using UnityEngine;

public class GripperController : MonoBehaviour
{
    public ArticulationBody leftGripper;  
    public ArticulationBody rightGripper; 
    public Rigidbody cube; 

    // Force parameters
    public float gripForceThreshold = 4.905f; // Force needed to grip the object
    public float gripSpeed = 0.01f; // Speed at which the grippers close
    private bool hasGripped = false; // Track if the object has been gripped

    void Start()
    {
        print("Hello");
        cube.useGravity = false;
    }

    void Update()
    {
        print("joint force left" + leftGripper.jointForce[0] + "joint force right" + rightGripper.jointForce[0]);
        if (!hasGripped)
        {
            // Move the left and right grippers towards the cube
            MoveGrippers();

            // Check if both grippers are applying enough force to grip the object
            if (IsGripping())
            {
                GripObject();
            }
        }
    }

    void MoveGrippers()
    {
        // Move left and right grippers towards each other
        ArticulationDrive leftDrive = leftGripper.xDrive;
        ArticulationDrive rightDrive = rightGripper.xDrive;

        // Update the target positions to move closer to the cube
        leftDrive.target -= gripSpeed;
        rightDrive.target += gripSpeed;

        // Apply the updated drive settings to each gripper
        leftGripper.xDrive = leftDrive;
        rightGripper.xDrive = rightDrive;
    }

    bool IsGripping()
    {
        return leftGripper.jointForce[0] >= gripForceThreshold && rightGripper.jointForce[0] >= gripForceThreshold;
    }

    void GripObject()
    {
        // When the object is gripped, enable gravity and set the object as a child of one of the grippers
        hasGripped = true;
        Debug.Log("Gravity enabled");
        cube.useGravity = true;
    }
}
