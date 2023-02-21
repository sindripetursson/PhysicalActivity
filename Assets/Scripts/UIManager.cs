using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    // Access the Activity Monitor
    private ActivityMonitor activityMonitor;

    // All UI elements
    public TextMeshProUGUI physicalActivityText;
    public TextMeshProUGUI headsetActivityText;
    public TextMeshProUGUI rightActivityText;
    public TextMeshProUGUI leftActivityText;
    public TextMeshProUGUI squatsActivityText;
    public TextMeshProUGUI jumpingJacksActivityText;
    public TextMeshProUGUI playersHeightText;
    public TextMeshProUGUI debugInfoJumpText;
    public TextMeshProUGUI debugInfoHandsText;

    // Physical activity bar
    public Image PhysicalActivityBar; // Access the bar
    private float currentFillAmount; // Track of current fill amount
    private float targetFillAmount; // Target fill amount
    [SerializeField] private int pointsNeededForFullActivityBar = 10000; // Points needed to fill up the physical activity bar
    private float rememberPhysicalActivityPoints = 0; // Used when the physical activity bar is resetted
    private bool isGettingAReset = false; // Used while the physical activity bar is getting a reset

    // Rewards
    public AudioSource audioSorce; // Sorce of the audio
    public AudioClip sound; // The sound played on a full activity bar
    public TextMeshProUGUI fullBarText; // Text displayed on activity bar when full
    private string[] complementWords = { "Nice!", "Great!", "Awesome!", "Excellent!", "Amazing!", "Active!", "Strong!", "Well done!" }; // 8 complements


    void Start()
    {
        GameObject activityMonitorGameObject = GameObject.Find("ActivityMonitor"); // Find the Activity Monitor in the scene
        activityMonitor = activityMonitorGameObject.GetComponent<ActivityMonitor>(); // Access the Activity Monitor

        // Initialize variables for the physical activity bar to 0
        currentFillAmount = 0f;
        targetFillAmount = 0f;

        // Set AudioClip for audio source
        audioSorce.clip = sound;
    }

    void Update()
    {
        // Get values each tracking point from the ActivityMonitor and display them in the UI
        physicalActivityText.text = "Physical activity points: " + (int)activityMonitor.totalMovementData;
        headsetActivityText.text = "Headset - Points " + (int)activityMonitor.headsetDistanceMoved + " - Position: " + activityMonitor.VRHeadset.position;
        rightActivityText.text = "Right hand - Points: " + (int)activityMonitor.rightDistanceMoved + " - Position: " + activityMonitor.rightController.position;
        leftActivityText.text = "Left hand - Points: " + (int)activityMonitor.leftDistanceMoved + " - Position: " + activityMonitor.leftController.position;
        // Display the number of squats and the number of squats multiplied with the points given for each
        squatsActivityText.text = "Squats: " + activityMonitor.numberOfSquats + " - Reward: " + activityMonitor.numberOfSquats*activityMonitor.numberOfPointsForSquats;
        // Display the number of jumping jacks
        jumpingJacksActivityText.text = "Jumping jacks: " + activityMonitor.numberOfJumpingJacks + " - Jumping jacks2: " + activityMonitor.numberOfJumpingJacks2 + " - Reward: " + ((activityMonitor.numberOfJumpingJacks + activityMonitor.numberOfJumpingJacks2) * activityMonitor.numberOfPointsForJumpingJack);
        // Extra stuff used for debugging
        playersHeightText.text = "Height: " + activityMonitor.playersHeight.ToString("F3");
        debugInfoJumpText.text = "heightdelta: " + activityMonitor.heightDelta.ToString("F3") + " - isJumping: " + activityMonitor.isJumping;
        debugInfoHandsText.text = "handsHeightDifference: " + activityMonitor.handsHeightDifference;

        // Update the physical activity bar
        UpdatePhysicalActivityBar();
    }

    void UpdatePhysicalActivityBar()
    {
        // Stop checking for physical activity bar updates while the bar is getting a reset
        if (!isGettingAReset)
        {
            // Calculate the targetFillAmount based on totalMovementData, minus the PhysicalActivity points when it was last resetted (So the bar gets a reset to 0 while the totalMovementData holds the total value)
            // Devided by pointsNeededForFullActivityBar, giving number between 0 and 1
            targetFillAmount = (activityMonitor.totalMovementData - rememberPhysicalActivityPoints) / pointsNeededForFullActivityBar;
            // If the targetFillAmount is significantly different from the currentFillAmount, interpolate towards the target fill amount using Lerp - Making it more smooth
            if (Mathf.Abs(targetFillAmount - currentFillAmount) > 0.001f)
            {
                // Interpolate the current fill amount towards the target fill amount over time - The last parameter is the speed of the interpolation
                currentFillAmount = Mathf.Lerp(currentFillAmount, targetFillAmount, Time.deltaTime * 5f);
            }
            else
            {
                // If the targetFillAmount is close to the currentFillAmount, set the targetFillAmount to the targetFillAmount directly
                currentFillAmount = targetFillAmount;
            }
            // Update the PhysicalActivityBar with the currentFillAmount
            PhysicalActivityBar.fillAmount = currentFillAmount;

            // A full physival activity bar is = 1
            if (PhysicalActivityBar.fillAmount >= 1)
            {
                Debug.Log("Filled");
                // TODO: Give the user a reward in game
                int randomRewardText = Random.Range(0, 8); // 0-7, One random complement word
                audioSorce.Play(); // Play a reward sound
                fullBarText.text = complementWords[randomRewardText]; // Display one of the complement words
                isGettingAReset = true; // Start to reset the bar
                // Reset the physical activity bar, without resetting the totalMovementData
                StartCoroutine(ResetPhysicalActivityBar());
            }
        }
    }

    private IEnumerator ResetPhysicalActivityBar()
    {
        yield return new WaitForSeconds(2); // Time until the bar gets resetted - Seconds will depend on how long the reward will take
        // Store the value of totalMovementData when the bar was resetted - Used when the physical activity bar is calculated again after each reset
        fullBarText.text = "";
        rememberPhysicalActivityPoints = activityMonitor.totalMovementData;
        float elapsedTime = 0f; // Tracks the time for the reset lerp
        float startFillAmount = 1f; // Goes from full bar
        float endFillAmount = 0f; // To a empty bar
        while(elapsedTime < 1f) // In 1 second
        {
            PhysicalActivityBar.fillAmount = Mathf.Lerp(startFillAmount, endFillAmount, elapsedTime / 1f);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        // Reset all physical activity bar variables
        currentFillAmount = 0f;
        targetFillAmount = 0f;
        PhysicalActivityBar.fillAmount = endFillAmount;
        // The reset is over - The player can start filling it up again
        isGettingAReset = false;
        Debug.Log("Physical activity bar resetted");
    }

}
