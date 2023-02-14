using UnityEngine;
using System.Collections;

public class ActivityMonitor : MonoBehaviour
{
    public Transform leftController; // left VR controller
    public Transform rightController; // right VR controller
    public Transform VRHeadset; // VR headset

    private Vector3 previousLeftPosition; // previous position of left VR controller
    private Vector3 previousRightPosition; // previous position of right VR controller
    private Vector3 previousHeadsetPosition; // previous position of VR headset

    private float leftDistanceMoved = 0f; // Total distance right controller has moved
    private float rightDistanceMoved = 0f; // Total distance left controller has moved
    private float headsetDistanceMoved = 0f; // Total distance VR headset has moved

    private float movementThreshold = 0.1f; // The minimum distance each point need to move to be triggered as movement
    private float movementToDistance = 100f; // In real life: 100f = 1cm, 0.05f = 5cm, 0.1f = 10cm

    private float totalMovementData = 0f; // leftDistanceMoved, rightDistanceMoved & headsetDistanceMoved combined into one value

    private bool initialized = false;
    IEnumerator Initialize()
    {
        // Wait for 1 second to make sure the controllers are in their initial position
        yield return new WaitForSeconds(1f);
        // Store the initial positions of the controllers and headset
        previousLeftPosition = leftController.position;
        previousRightPosition = rightController.position;
        previousHeadsetPosition = VRHeadset.position;
        initialized = true;
    }
    void Start()
    {
        // Start the initialization process
        StartCoroutine(Initialize());
    }
    void Update()
    {
        // Check if the initialization process has finished
        // To prevent tracking points to gain points when the teleport from their initial positions
        if (!initialized)
        {
            return;
        }

        // Check if the left controller has moved
        if (Vector3.Distance(leftController.position, previousLeftPosition) > movementThreshold)
        {
            // Calculate the distance the left controller has moved
            float leftMovement = Vector3.Distance(leftController.position, previousLeftPosition);
            // increase the distanceMoved by the amount the controller has moved
            leftDistanceMoved += leftMovement * movementToDistance;
            // Update the previous position of the left controller
            previousLeftPosition = leftController.position;
            // Output the distanceMoved variable to the console
            Debug.Log("Left controller moved " + leftDistanceMoved + " centimeters this frame.");
            totalMovementData += leftDistanceMoved;

        }
        // Check if the right controller has moved
        if (Vector3.Distance(rightController.position, previousRightPosition) > movementThreshold)
        {
            // Calculate the distance the right controller has moved
            float rightMovement = Vector3.Distance(rightController.position, previousRightPosition);
            // increase the distanceMoved by the amount the controller has moved
            rightDistanceMoved += rightMovement * movementToDistance;
            // Update the previous position of the right controller
            previousRightPosition = rightController.position;
            // Output the distanceMoved variable to the console
            //Debug.Log("Right controller moved " + rightDistanceMoved + " centimeters this frame.");
            totalMovementData += rightDistanceMoved;
        }
        // Check if the VR headset has moved
        if (Vector3.Distance(VRHeadset.position, previousHeadsetPosition) > movementThreshold)
        {
            // Calculate the distance the VR headset has moved
            float headsetMovement = Vector3.Distance(VRHeadset.position, previousHeadsetPosition);
            // increase the distanceMoved by the amount the VR headset has moved
            headsetDistanceMoved += headsetMovement * movementToDistance;
            // Update the previous position of the right controller
            previousHeadsetPosition = VRHeadset.position;
            // Output the distanceMoved variable to the console
            //Debug.Log("VR headset moved " + headsetDistanceMoved + " centimeters this frame.");
            totalMovementData += headsetDistanceMoved;
        }

        //totalMovementData = headsetDistanceMoved + rightDistanceMoved + leftDistanceMoved;
        //Debug.Log(totalMovementData);
    }
}