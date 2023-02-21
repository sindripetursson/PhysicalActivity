using UnityEngine;
using System.Collections;

public class ActivityMonitor : MonoBehaviour
{
    // Setup
    private bool initialized = false;

    // Variables for general movements
    public Transform leftController; // left VR controller
    public Transform rightController; // right VR controller
    public Transform VRHeadset; // VR headset

    // Previous position of each tracking point
    private Vector3 previousLeftPosition; // previous position of left VR controller
    private Vector3 previousRightPosition; // previous position of right VR controller
    private Vector3 previousHeadsetPosition; // previous position of VR headset

    // Total distance of each tracking point
    public float leftDistanceMoved = 0f; // Total distance left controller has moved
    public float rightDistanceMoved = 0f; // Total distance right controller has moved
    public float headsetDistanceMoved = 0f; // Total distance VR headset has moved

    // Movement Thresholds
    private float movementThreshold = 0.1f; // The minimum distance each tracking point needs to be move to be triggered as movement
    private float teleportThreshold = 1f; // The maximum distance each tracking point can move to be triggered as movement - To avoid gaining points when tracking points teleports after disconnection

    // Measurements
    public float heightDelta; // How far player's head is from his standing upright position

    // Convertion
    private float movementToDistance = 100f; // Convert tracking point data to real life distance: In real life: 100f = 1cm, 0.05f = 5cm, 0.1f = 10cm

    // Points
    public float totalMovementData = 0f; // All physical activity data combined into one value - From each tracking point and from all beneficial movements

    // Beneficial movements
    [SerializeField] public float playersHeight = 1.7f; // Height of the VR headset when user is standing up, used for movement detection - TODO: Add a method to calibrate this
    private bool movementReset = false; // Check if user got up to standing up position between each beneficial movelemt
    [SerializeField] private float movementCoolDown = 6.0f; // Cool down time needed between beneficial movement
    private float timeSinceMovement = 0f; // Time elapsed since the last beneficial movment detection - should be set to 0

    // Squat movment
    public int numberOfSquats = 0; // Number of successful squats by the user
    public int numberOfPointsForSquats = 1000; // Number of points given for each successful squat
    [SerializeField] private float squatThreshold = 0.3f; // Percent of players height needed to travel down with the headset - 0.3 = 30% of players height - Larger number = deeper squat
    [SerializeField] private float squatHeadRotationThreshold = 0.5f; // The amount player can look down while performing a squat - 0 = looking straight forward , 1 = looking straight down

    // Jumping
    public bool isJumping = false; // Detect when player is jumping
    private float jumpStartTime = 0f; // How long was the player jumping
    private float timeSinceJump = 0f; // How long is it since the player finished a jump

    // Jumping jack
    [SerializeField] private float heightToDetectJump = 0.1f; // Threshold to detect jump - Used with heightDelta = 0 is standing upright - 0.1 is little above that
    private float maxHandsDifference = 0.2f; // Max allowed y-axis on player hands while performing jumping jacks
    public float handsHeightDifference; // Check the difference on left and right hand height
    private bool jumpingJackInitial = false; // Player is standing upright with hands down 
    private bool jumpingJackSecondPhase = false; // player is standing upright with hands up
    public int numberOfJumpingJacks = 0; // Number of successful jumping jacks by the user
    public int numberOfJumpingJacks2 = 0; // Number of successful jumping jacks by the user
    public int numberOfPointsForJumpingJack = 300; // Number of points given for each successful jumping jack

    IEnumerator Initialize()
    {
        // Wait for 1 second to make sure the controllers are in their initial position
        yield return new WaitForSeconds(1f);
        // Store the initial positions of the controllers and the headset
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
        // To prevent tracking points to gain points when they teleport from their initial positions
        if (!initialized)
        {
            return;
        }

        // Get currentHeight of the player - headset from ground
        float currentHeight = VRHeadset.position.y;
        // Get how far player's head is from his standing upright position
        heightDelta = currentHeight - playersHeight;
        //Debug.Log(heightDelta);
        // Calculate the height difference between the hands of the player
        handsHeightDifference = Mathf.Abs(leftController.position.y - rightController.position.y);
        //Debug.Log(handsHeightDifference);
        // Get current orientation of the VR headset
        Vector3 headForward = VRHeadset.forward;

        // Monitor if the tracking points are getting teleported
        if (Vector3.Distance(leftController.position, previousLeftPosition) > teleportThreshold)
        {
            Debug.Log("Teleport: left controller");
        }
        if (Vector3.Distance(rightController.position, previousRightPosition) > teleportThreshold)
        {
            Debug.Log("Teleport: right controller");
        }
        if (Vector3.Distance(VRHeadset.position, previousHeadsetPosition) > teleportThreshold)
        {
            Debug.Log("Teleport: VR headset");
        }

        // Press the A button on right VR controller to calibrate the players height
        if (Input.GetButtonDown("A_Button"))
        {
            Debug.Log("Player's height calibrated to: " + playersHeight);
            playersHeight = currentHeight;
        }

        // Checks if player's head is close to upstanding position
        // Makes sure that player has to go to initial standing position between each beneficial movement
        // Only goes into this if movementReset is false
        if (heightDelta > -0.1 && heightDelta < 0.1 && !movementReset)
        {
            Debug.Log("Movement Reset!");
            movementReset = true; // Player can now perform a new beneficial movement 
        }

        // Check if the player is jumping
        if(heightDelta > heightToDetectJump)
        {
            isJumping = true;
            // Timer on how long the jump was
            jumpStartTime = Time.time;
        }
        else
        {
            if (isJumping)
            {
                Debug.Log("No Longer jumping");
                timeSinceJump = Time.time;
            }
            // Player is no longer jumping
            isJumping = false;
        }

        //Debug.Log((leftController.position.y + rightController.position.y) / 2);

        // Check if the user has performed a beneficial movement
        // First check for a cooldown between each beneficial movement
        // Skip this check if player has got into their initial standing position (Then they dont have to wait for the cooldown timer)
        if (!movementReset && timeSinceMovement < movementCoolDown)
        {
            timeSinceMovement += Time.deltaTime;
        }
        else if (currentHeight > 0 && heightDelta < 0 && Mathf.Abs(heightDelta) > playersHeight * squatThreshold && Vector3.Dot(headForward, Vector3.down) < squatHeadRotationThreshold)
        {
            // SQUAT
            // Current Height is higher than 0 (Becomes 0 when headset disconnects / looses connection)
            // heightDelta checkes distance from players initial standing position (above 0) or (below 0)
            // If the user moves down by squat threshold (certain % of the players height) then it is detected as a squat
            // Checks how much the headset is facing down - The head can be slightliy facing down, but not over the squatHeadRotationThreshold

            movementReset = false; // is false until player's head is close to standin up position
            // Reset the timer of the beneficial movement cool down
            timeSinceMovement = 0.0f;
            numberOfSquats += 1;
            Debug.Log("Squat Detected!");
        }
        else if(handsHeightDifference < maxHandsDifference) // First check if the players hands are within similar height while performing jumping jack
        {
            // Calculates the average height of both controllers - To detect if they are below or above waist
            float avgControllerHeight = (leftController.position.y + rightController.position.y) / 2;
            // JUMPINGJACK
            // Checks if player is not jumping and the average height of both controllers are under 1 (below waist)
            if (!isJumping && avgControllerHeight < 1)
            {
                // Then the player is in the initial jumping jack position
                jumpingJackInitial = true;
                // Check if player is coming from the second JJ phase (hands above head) - And make sure it is less than 1 second since last jump
                if(jumpingJackSecondPhase && (Time.time - timeSinceJump) < 0.5)
                {
                    // Second phase of JJ is over
                    jumpingJackSecondPhase = false;
                    // Give one point for JJ second phase
                    Debug.Log("Jumping jack2 detected!");
                    numberOfJumpingJacks2 += 1;
                }
            }
            // If the player is jumping and controllers are above 1.5 (above head)
            if (isJumping && avgControllerHeight > 1.5)
            { 
                // Then the player is getting into the second phase of JJ - Player is no longer in the initial position
                jumpingJackSecondPhase = true;
                // Check if player is coming from JJ initial position
                if (jumpingJackInitial) {
                    // It is no longer in JJ inital position
                    jumpingJackInitial = false;
                    // Give one point for JJ second phase
                    Debug.Log("Jumping jack detected!");
                    numberOfJumpingJacks += 1;
                }
            }
        }

        // Left controller
        // if the left controller moved furthar than movementThreshold than it counts as a movement
        // or if the left controller moved further than teleportThreshold than the movment was probably a teleported controller
        if (Vector3.Distance(leftController.position, previousLeftPosition) > movementThreshold || Vector3.Distance(leftController.position, previousLeftPosition) < teleportThreshold)
        {
            float leftMovement = Vector3.Distance(leftController.position, previousLeftPosition); // Calculate the distance the left controller has moved
            leftDistanceMoved += leftMovement * movementToDistance; // Increase the distanceMoved by the amount the controller has moved
            previousLeftPosition = leftController.position; // Update the previous position of the left controller
            //Debug.Log("Left controller moved " + leftDistanceMoved + " centimeters this frame.");
        }
        // Right controller
        // if the right controller moved furthar than movementThreshold than it counts as a movement
        // or if the right controller moved further than teleportThreshold than the movment was probably teleported controller
        if (Vector3.Distance(rightController.position, previousRightPosition) > movementThreshold || Vector3.Distance(rightController.position, previousRightPosition) < teleportThreshold)
        {
            float rightMovement = Vector3.Distance(rightController.position, previousRightPosition); // Calculate the distance the right controller has moved
            rightDistanceMoved += rightMovement * movementToDistance; // Increase the distanceMoved by the amount the controller has moved
            previousRightPosition = rightController.position; // Update the previous position of the right controller
            //Debug.Log("Right controller moved " + rightDistanceMoved + " centimeters this frame.");
        }
        // VR headset
        // if the VR headset moved furthar than movementThreshold than it count as a movement
        // or if the VR headset moved further than teleportThreshold than the movment was probably teleported controller
        if (Vector3.Distance(VRHeadset.position, previousHeadsetPosition) > movementThreshold || Vector3.Distance(VRHeadset.position, previousHeadsetPosition) < teleportThreshold)
        {
            float headsetMovement = Vector3.Distance(VRHeadset.position, previousHeadsetPosition); // Calculate the distance the VR headset has moved
            headsetDistanceMoved += headsetMovement * movementToDistance; // increase the distanceMoved by the amount the VR headset has moved
            previousHeadsetPosition = VRHeadset.position; // Update the previous position of the right controller
            //Debug.Log("VR headset moved " + headsetDistanceMoved + " centimeters this frame.");
        }
        // Combile all physical activity data into one value - From each tracking point and from all beneficial movements
        totalMovementData = headsetDistanceMoved + rightDistanceMoved + leftDistanceMoved + numberOfSquats * numberOfPointsForSquats + (numberOfJumpingJacks + numberOfJumpingJacks2) * numberOfPointsForJumpingJack;
        //Debug.Log(totalMovementData);
    }
}