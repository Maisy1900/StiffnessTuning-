using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using static UnityEngine.GraphicsBuffer;
using System.IO.Enumeration;
using System;
using Unity.VisualScripting;
using UnityEngine.UIElements;

public class SimpleController : MonoBehaviour
{
    public ArticulationBody joint_1;
    public Transform joint_1_driver;
    public Rigidbody cube;
    public Rigidbody tip;
    public string path;
    public Animation rotateAnim;
    //private Animation playedAnim;

    public Animator main_anim;
    public string[] animation_names = new string[] { "Joint_Rotation_5_Degrees", "Joint_Rotation_10_Degrees", "Joint_Rotation_15_Degrees", "Joint_Rotation_20_Degrees", 
                                                     "Joint_Rotation_25_Degrees", "Joint_Rotation_30_Degrees", "Joint_Rotation_35_Degrees", "Joint_Rotation_40_Degrees", 
                                                     "Joint_Rotation_45_Degrees" };

    public int[] targetAngle = new int[7];
    private int counter = 0;

    private float startTime;
    private float finalTime; // to store the time when max velocity is reached
    private bool hasStarted = false;
    private float lastVelocity = 0f;
    private float velocityThreshold = 0.001f; // smaller threshold for stability check
    private bool reachedMaxVelocity = false;
    private float initialVelocity = 0f;
    private int count = 0; 

    Vector3 lastGripperVelocity;  // Store velocity from last frame
    Vector3 lastCubeVelocity;     // Store cube velocity from last frame
    Vector3 lastGripperAngularVelocity;

    private float massOfCube = 1f; // mass of the object in kg
    private float massOfGripper = 1f;
    string allData; 

    private void Start()
    {
        targetAngle = new int[] { 5, 10, 15, 20, 25, 30, 35 };

    }

    private void Update()
    {
        
        if(Input.GetKeyDown(KeyCode.A))
        {
            // Randomly/orderly select one of the animations and run them here.
            // play animations

            counter++;
        }
    }

    void FixedUpdate()
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
                          //Time.fixedTime;

                // calculate acceleration using (v_f - v_i) / t
                float finalVelocity = currentVelocity.magnitude; // assuming this is max velocity
                float acceleration = (finalVelocity - initialVelocity) / timeToMaxVelocity;

                // calculate force using F = m * a
                float force = massOfCube * acceleration;

                // print time, velocity, calculated acceleration, and force
                print("max velocity reached in " + timeToMaxVelocity + " seconds");
                print("final velocity " + finalVelocity + " m/s");
                print("acceleration " + acceleration + " m/s^2");
                print("force " + force + " N");
                print("final time " + finalTime + " seconds"); // printing the final time

                // print xDrive properties target and velocity
                print("xDrive target " + jointDrive.target);
                
                allData = timeToMaxVelocity.ToString() + "," + finalVelocity.ToString() + "," + acceleration.ToString() + "," + force.ToString() + "," + jointDrive.target.ToString();
            }


            int randNumb = UnityEngine.Random.Range(100, 999);
            string fileName = "/" + randNumb.ToString() + "_physics_data.csv";//call it animation type + trial number 
            File.AppendAllText(Application.persistentDataPath + fileName, allData);
            //File.WriteAllText(Application.persistentDataPath + fileName, allData);

            // update last velocity for next frame
            lastVelocity = currentVelocity.magnitude;
        }
    }
   void StartTrial()
    {
        // Increment the trial counter for each new trial
        count++;

        // Randomly/orderly select one of the animations and run them here
      /* 
        string animName = animation_names[animIndex];

        // Create a unique file name for this trial
        string fileName = animName + "_trial_" + trialCounter.ToString() + ".csv";
        currentFilePath = Application.persistentDataPath + "/" + fileName;

        // Write column labels (headers) to the CSV file
        string headers = "Time,CubePosX,CubePosY,CubePosZ,CubeVelX,CubeVelY,CubeVelZ,CubeAccX,CubeAccY,CubeAccZ,CubeForceX,CubeForceY,CubeForceZ," +
                         "GripperPosX,GripperPosY,GripperPosZ,GripperVelX,GripperVelY,GripperVelZ,GripperAccX,GripperAccY,GripperAccZ,GripperForceX,GripperForceY,GripperForceZ," +
                         "GripperAngularVelX,GripperAngularVelY,GripperAngularVelZ,AngularAccX,AngularAccY,AngularAccZ,TorqueX,TorqueY,TorqueZ";
        File.WriteAllText(currentFilePath, headers + "\n");  // Write headers

        // Start the logging process for this animation trials
      */
       // StartCoroutine(PlayAnimation(animIndex));
    }
    void RecordResults(float normTime)
    {
        //velocity
        Vector3 currentGripperVelocity = tip.velocity;
        Vector3 currentCubeVelocity = cube.velocity;
        Vector3 currentGripperAngularVelocity = tip.angularVelocity;
        //acceleration
        Vector3 gripperAcceleration = (currentGripperVelocity - lastGripperVelocity) / Time.fixedDeltaTime;
        Vector3 cubeAcceleration = (currentCubeVelocity - lastCubeVelocity) / Time.fixedDeltaTime;
        Vector3 angularAcceleration = (currentGripperAngularVelocity - lastGripperAngularVelocity) / Time.fixedDeltaTime;
        
        //torque 
        // Moment of inertia for the Rigidbody (Unity handles it internally, but you can define it for custom shapes)
        float momentOfInertia = tip.inertiaTensor.z;
        // Calculate the torque using (torque = moment of inertia * angular acceleration)
        Vector3 torque = momentOfInertia * angularAcceleration;

        //force
        Vector3 gripperForce = massOfGripper * gripperAcceleration;
        Vector3 cubeForce = massOfCube * cubeAcceleration;

        //record 
        Vector3 cubePos = cube.transform.position;
        Vector3 cubeVelocity = cube.velocity;
        float time = normTime;
        Vector3 griperVel = tip.velocity; 
        Vector3 gripperPos = tip.transform.position;
        //store the data

        //store velocities for next frame
        lastGripperVelocity = currentGripperVelocity;
        lastCubeVelocity = currentCubeVelocity;
    }
    //Play animation
    public IEnumerator PlayAnimation(int animIndex)
    {
        yield return null;
        main_anim.Play(animation_names[animIndex], 0);
        float normedTime = 0f;
        yield return null; 
        while(normedTime < 1)
        {
            normedTime = main_anim.GetCurrentAnimatorStateInfo(0).normalizedTime;
            //record results of the cube position, velocity, angular velocity (of the tip of the gripper <add rigid body to the centre of the gripper> 
            yield return new WaitForFixedUpdate();
        }
    }
}
