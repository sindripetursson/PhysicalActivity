using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

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

    void Start()
    {
        GameObject activityMonitorGameObject = GameObject.Find("ActivityMonitor"); // Find the Activity Monitor in the scene
        activityMonitor = activityMonitorGameObject.GetComponent<ActivityMonitor>(); // Access the Activity Monitor
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
    }
}
