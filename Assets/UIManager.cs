using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    private ActivityMonitor activityMonitor;

    public TextMeshProUGUI physicalActivityText;
    public TextMeshProUGUI headsetActivityText;
    public TextMeshProUGUI rightActivityText;
    public TextMeshProUGUI leftActivityText;

    // Start is called before the first frame update
    void Start()
    {
        GameObject activityMonitorGameObject = GameObject.Find("ActivityMonitor");
        activityMonitor = activityMonitorGameObject.GetComponent<ActivityMonitor>();
    }

    // Update is called once per frame
    void Update()
    {
        physicalActivityText.text = "Physical activity points: " + (int)activityMonitor.totalMovementData;
        headsetActivityText.text = "Headset: " + (int)activityMonitor.headsetDistanceMoved;
        rightActivityText.text = "Right hand: " + (int)activityMonitor.rightDistanceMoved;
        leftActivityText.text = "Left hand: " + (int)activityMonitor.leftDistanceMoved;
    }
}
