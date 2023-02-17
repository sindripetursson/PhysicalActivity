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

    // Physical activity bar
    public Image PhysicalActivityBar; // Access the bar
    private float currentFillAmount; // Track of current fill amount
    private float targetFillAmount; // Target fill amount
    [SerializeField] private int pointsNeededForFullActivityBar = 10000; // Points needed to fill up the physical activity bar

    void Start()
    {
        GameObject activityMonitorGameObject = GameObject.Find("ActivityMonitor"); // Find the Activity Monitor in the scene
        activityMonitor = activityMonitorGameObject.GetComponent<ActivityMonitor>(); // Access the Activity Monitor

        // Initialize the current and target fill amounts to 0
        currentFillAmount = 0f;
        targetFillAmount = 0f;
    }

    void Update()
    {
        // Get values each tracking point from the ActivityMonitor and display them in the UI
        physicalActivityText.text = "Physical activity points: " + (int)activityMonitor.totalMovementData;
        headsetActivityText.text = "Headset: " + (int)activityMonitor.headsetDistanceMoved;
        rightActivityText.text = "Right hand: " + (int)activityMonitor.rightDistanceMoved;
        leftActivityText.text = "Left hand: " + (int)activityMonitor.leftDistanceMoved;
        // Display the number of squats and the number of squats multiplied with the points given for each
        squatsActivityText.text = "Squats: " + activityMonitor.numberOfSquats + " - Reward: " + activityMonitor.numberOfSquats*activityMonitor.numberOfPointsForSquats;

        // Update the physical activity bar
        UpdatePhysicalActivityBar();
    }

    void UpdatePhysicalActivityBar()
    {
        // Calculate the target fill amount based on the totalMovementData 
        targetFillAmount = activityMonitor.totalMovementData / pointsNeededForFullActivityBar;
        // If the target fill amount is significantly different from the current fill amount, interpolate towards the target fill amount using Lerp
        if (Mathf.Abs(targetFillAmount - currentFillAmount) > 0.001f)
        {
            // Interpolate the current fill amount towards the target fill amount over time
            currentFillAmount = Mathf.Lerp(currentFillAmount, targetFillAmount, Time.deltaTime * 5f); // The last parameter is the speed of the interpolation
        }
        else
        {
            // If the target fill amount is close to the current fill amount, set the current fill amount to the target fill amount directly
            currentFillAmount = targetFillAmount;
        }
        // Update the physical activity bar with the currentFillAmount 
        PhysicalActivityBar.fillAmount = currentFillAmount;
    }

}
