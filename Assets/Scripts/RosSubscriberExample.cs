using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
// using RosColor = RosMessageTypes.UnityRoboticsDemo.UnityColorMsg;
using RosScan = RosMessageTypes.Sensor.LaserScanMsg;
using RosPcd = RosMessageTypes.Sensor.PointCloud2Msg;
// using RosScan = RosMessageTypes.UnityRoboticsDemo.ScanRangesMsg;


public class RosSubscriberExample : MonoBehaviour
{
    public GameObject husky;
    public float[] ranges;
    
    void Start()
    {
        // ROSConnection.GetOrCreateInstance().Subscribe<RosColor>("color", ColorChange);
        ROSConnection.GetOrCreateInstance().Subscribe<RosScan>("/scan", ScanCallback);
        // ROSConnection.GetOrCreateInstance().Subscribe<RosScan>("/velodyne_points", PcdCallback)};
    }

    // void ColorChange(RosColor colorMessage)
    // {
    //     cube.GetComponent<Renderer>().material.color = new Color32((byte)colorMessage.r, (byte)colorMessage.g, (byte)colorMessage.b, (byte)colorMessage.a);
    // }
    
    void ScanCallback(RosScan laserMsg)
    {
        ranges = new float[laserMsg.ranges.Length];
        for (int i = 0; i < laserMsg.ranges.Length; i++)
        {
            ranges[i] = laserMsg.ranges[i];
            // Debug.Log($"ranges[{i}]: {ranges[i]}");
        }
    }
    
    // void PcdCallback(RosPcd PointCloud2Msg)
    // {
    //     ranges = PointCloud2Msg.;
    // }
    
    // public float[] GetCurrentScanRanges()
    // {
    //     return ranges;
    // }
}