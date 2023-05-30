using UnityEngine;
using System.Collections;

public class ActivityMonitor : MonoBehaviour
{
    // Setup
    [Header("Setup")]
    private bool initialized = false;

    // Variables for general movements
    [Header("General movements")]
    public Transform leftController; // Left VR controller
    public Transform rightController; // Right VR controller
    public Transform VRHeadset; // VR headset

    // Previous position of each tracking point
    [Header("Previous positions")]
    private Vector3 previousLeftPosition; // Previous position of left VR controller
    private Vector3 previousRightPosition; // Previous position of right VR controller
    private Vector3 previousHeadsetPosition; // Previous position of VR headset

    // Total distance of each tracking point
    [Header("Total distance")]
    public float leftDistanceMoved = 0f; // Total distance left controller has moved
    public float rightDistanceMoved = 0f; // Total distance right controller has moved
    public float headsetDistanceMoved = 0f; // Total distance VR headset has moved

    // Movement Thresholds
    [Header("Movement Thresholds")]
    private float movementThreshold = 0.1f; // The minimum distance each tracking point needs to be move to be triggered as movement
    private float teleportThreshold = 1.0f; // The maximum distance each tracking point can move to be triggered as movement - To avoid gaining points when teleported on disconnect

    // Convertion
    [Header("Convertion")]
    private float movementToDistance = 100f; // Convert tracking point data to real life distance: 100f = 1cm, 0.05f = 5cm, 0.1f = 10cm

    // Beneficial movements
    [Header("Beneficial movements")]
    public float totalMovementData = 0f; // All physical activity data combined into one value - From each tracking point and from all beneficial movements
    [SerializeField] public float playersHeight = 1.7f; // Height of headset when user is standing up, used for movement detection
    [HideInInspector] public float heightDelta; // How far player's head is from his standing upright position
    private bool squatReset = false; // Check if user got up to standing up position between beneficial movements
    [SerializeField] private float squatCoolDown = 5.0f; // Cool down time needed between beneficial movement
    private float timeSinceSquat = 0f; // Time elapsed since the last beneficial movment detection

    // Squatting
    [Header("Squatting")]
    public int numberOfSquats = 0; // Number of successful squats
    public int numberOfPointsForSquats = 1000; // Number of points for each successful squat
    [SerializeField] private float squatThreshold = 0.3f; // Percent of players height needed to travel down with the headset - 0.3 = 30% of players height - Larger number = deeper squat
    [HideInInspector] public float lookingDown; // The amout the player is looking down
    [SerializeField] private float squatHeadRotationThreshold = 0.5f; // The amount player can look down while performing a squat - 0 = looking straight forward , 1 = looking straight down

    // Side jack
    [Header("Side jack")]
    public float handHeightThreshold = 0.6f; // The amount the player has to raise the hand to detect side jack
    public int numberOfSideJacksL = 0; // Number of successful left side jacks
    public int numberOfSideJacksR = 0; // Number of successful right side jacks
    public int numberOfPointsForSideJack = 250; // Points given for each successful side jack
    private bool sideJackReset = false; // Player needs to tilt head back close to initial position and put hand down between each side jack
    [HideInInspector] public float rotationDelta = 0; // Check if headset rotation is to left or right

    // Jumping
    [Header("Jumping")]
    [HideInInspector] public bool isJumping = false; // Detect when player is jumping
    public float timeSinceJump = 0f; // How long is it since the player finished a jump

    // Jumping jack
    [Header("Jumping jack")]
    [SerializeField] private float heightToDetectJump = 0.1f; // Threshold to detect a jump
    private float maxHandsDifference = 0.2f; // Max allowed y-axis difference on player hands while performing jumping jacks
    [HideInInspector] public float handsHeightDifference; // Check the difference on left and right hand height
    private bool jumpingJackInitial = false; // Player is standing upright with hands down 
    private bool jumpingJackSecondPhase = false; // player is standing upright with hands up
    public int numberOfJumpingJacks = 0; // Number of successful jumping jacks from initial position (hands below waist)
    public int numberOfJumpingJacks2 = 0; // Number of successful jumping jacks from second position (hand above head)
    public int numberOfPointsForJumpingJack = 250; // Number of points given for each successful jumping jack
    public float timeSinceJumpingJack = 0f; // Check how long time since last jumping jack
    public bool isInJJ = false;

   // Beneficial Movement Icons
   [Header("Icons")]
    public Material whiteIcon; // White color is default
    public Material greenIcon; // Green color when movement is triggered
    public SpriteRenderer squatIcon; // Squat icon location
    public SpriteRenderer jumpingJackIcon; // Jumping jack icon location
    public SpriteRenderer sideJackIcon; // Side jack icon location
    public Sprite RightSide; // Reference to right side sprite
    public Sprite LeftSide; // Reference to left side sprite

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
        // To prevent tracking points to gain points when teleported from initial positions
        if (!initialized)
        {
            return;
        }

        // Get currentHeight of the player - headset from ground
        float currentHeight = VRHeadset.position.y;
        // Get how far player's head is from his standing upright position
        heightDelta = currentHeight - playersHeight;
        // Calculate the height difference between the hands of the player
        handsHeightDifference = Mathf.Abs(leftController.position.y - rightController.position.y);
        // Looking down calculates how much down headset is facing (0 = facing forward / 1 = facing down) - VRHeadset.forward gets current orientation of headset
        lookingDown = Vector3.Dot(VRHeadset.forward, Vector3.down);
        // Get current rotation of the headset
        Quaternion currentRotation = VRHeadset.transform.rotation;
        // Calculate how much headset rotation has changed from a initial rotation position - Rotations to left and right
        rotationDelta = Mathf.Abs(currentRotation.eulerAngles.z - Quaternion.identity.eulerAngles.z);
        // Calculate the height difference between the hands of the player (Right hand above = negative number, Left hand above = positive number) 
        float handHeightDelta = leftController.position.y - rightController.position.y;

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
        // Makes sure that player has to go to initial standing position between each squat movement
        // Only goes into this if movementReset is false
        if (heightDelta > -0.1 && heightDelta < 0.1 && !squatReset)
        {
            Debug.Log("Squat Reset!");
            // Set squat icon back to white
            squatIcon.material = whiteIcon;
            squatReset = true; // Player can now perform a new squat movement 
        }

        // Check if the player is jumping - heightDelta = 0 is standing upright - 0.1 is little above that
        if (heightDelta > heightToDetectJump)
        {
            // Player is in the air
            isJumping = true;
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

        // SQUAT
        // Check if the user has performed a beneficial movement
        // First check for a cooldown between each beneficial movement
        // Skip this check if player has got into their initial standing position (Then they dont have to wait for the cooldown timer)
        if (!squatReset && timeSinceSquat < squatCoolDown)
        {
            timeSinceSquat += Time.deltaTime;
        }
        else if (currentHeight > 0 && heightDelta < 0 && Mathf.Abs(heightDelta) > playersHeight * squatThreshold && lookingDown < squatHeadRotationThreshold)
        {
            // Current height is higher than 0 (Becomes 0 when headset disconnects / looses connection)
            // heightDelta checkes distance from players initial standing position (above 0) or (below 0)
            // If the user moves down by squat threshold (certain % of the players height) then it is detected as a squat
            // Checks how much the headset is facing down - The head can be slightliy facing down, but not over the squatHeadRotationThreshold

            // Set squat icon to green
            squatIcon.material = greenIcon;
            // Is false until player's head is close to standin up position
            squatReset = false;
            // Reset the timer of the beneficial movement cool down
            timeSinceSquat = 0.0f;
            numberOfSquats += 1;
            Debug.Log("Squat Detected!");
        }

        // SIDE JACK
        // Checks if side jack is resetted, if head is tilting to left side, and if the right hand is up (oppisite hand to side)
        if (sideJackReset && rotationDelta > 20 && rotationDelta < 90 && handHeightDelta < -handHeightThreshold)
        {
            // When left side is triggered, change icon to left side and color green
            sideJackIcon.sprite = LeftSide; 
            sideJackIcon.material = greenIcon;
            // Player will have to reset between each side jack
            sideJackReset = false;
            // Side jack to left side was detected
            numberOfSideJacksL += 1;
            Debug.Log("Side jack left side detected!");
        }
        // Checks if side jack is resetted, if head is tilting to right side, and if the left hand is up (oppisite hand to side)
        else if (sideJackReset && rotationDelta < 340 && rotationDelta > 300 && handHeightDelta > handHeightThreshold)
        {
            // When right side is triggered, change icon to right side and color green
            sideJackIcon.sprite = RightSide;
            sideJackIcon.material = greenIcon;
            // Player will have to reset between each side jack
            sideJackReset = false;
            // Side jack to right side detected
            numberOfSideJacksR += 1;
            Debug.Log("Side jack right side detected!");
        }
        // If side jack has not been resetted already, if head is close to initial rotation and if the hands are close to eachother again
        else if (!sideJackReset && (rotationDelta < 10 || rotationDelta > 350) && Mathf.Abs(handHeightDelta) < 0.2)
        {
            // Side jack icon becomes white
            sideJackIcon.material = whiteIcon;
            // Side jack gets a reset
            sideJackReset = true;
            Debug.Log("Side jack resetted");
        }

        // JUMPING JACK
        // First check if the players hands are within similar height while performing jumping jack
        if (handsHeightDifference < maxHandsDifference)
        {
            // Calculates the average height of both controllers - To detect if they are below or above waist
            float avgControllerHeight = (leftController.position.y + rightController.position.y) / 2;
            // Checks if player is not jumping and the average height of both controllers are under 1 (below waist)
            if (!isJumping && avgControllerHeight < 1)
            {
                // Then the player is in the initial jumping jack position
                jumpingJackInitial = true;
                // Check if player is coming from the second JJ phase (hands above head) - And make sure it is less than 1 second since last jump
                if(jumpingJackSecondPhase && (Time.time - timeSinceJump) < 0.5)
                {
                    // Jumping jack icon becomes green
                    if (!isInJJ)
                    {
                        StartCoroutine(JJIcon());
                    }
                    // Reset timer since last jumping jack
                    timeSinceJumpingJack = Time.time - timeSinceJumpingJack;
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
                    // Jumping jack icon becomes green
                    if (!isInJJ)
                    {
                        StartCoroutine(JJIcon());
                    }
                    // Reset timer since last jumping jack
                    timeSinceJumpingJack = Time.time - timeSinceJumpingJack;
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
        totalMovementData = headsetDistanceMoved + rightDistanceMoved + leftDistanceMoved + numberOfSquats * numberOfPointsForSquats + (numberOfJumpingJacks + numberOfJumpingJacks2) * numberOfPointsForJumpingJack + (numberOfSideJacksL + numberOfSideJacksR) * numberOfPointsForSideJack;
    }

    IEnumerator JJIcon()
    {
        isInJJ = true;
        jumpingJackIcon.material = greenIcon;
        // Wait for 1 second
        yield return new WaitForSeconds(1f);
        isInJJ = false;
        jumpingJackIcon.material = whiteIcon;
    }
}