using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Robotics.Core;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;
using RosMessageTypes.Std;

using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnitySensors;
using RosMessageTypes.Geometry; 

/// <summary>
///     This script publishes laser scan
/// </summary>
public class LaserScanPublisher : MonoBehaviour
{
    // ROS Connector
    private ROSConnection ros;
    // Variables required for ROS communication
    public string laserTopicName = "base_scan";
    public string laserLinkId = "velodyne_link";

    // Sensor
    public VelodyneSensor laser;

    // Message
    private LaserScanMsg laserScan;
    public float publishRate = 10f;

    void Start()
    {
        // Get ROS connection static instance
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<LaserScanMsg>(laserTopicName);

        // Initialize messages
        // float angleIncrement = (laser.angleMax - laser.angleMin)/(laser.samples-1);
        // float scanTime = 1f / laser.updateRate;
        // float timeIncrement = scanTime / laser.samples;
        float[] intensities = new float[laser.GetDistances().Length];
        laserScan = new LaserScanMsg
        {
            header = new HeaderMsg(Clock.GetCount(), 
                                   new TimeStamp(Clock.time), laserLinkId),
            // angle_min       = laser.angleMin,
            // angle_max       = laser.angleMax,
            // angle_increment = angleIncrement,
            // time_increment  = timeIncrement,
            // scan_time       = scanTime,
            // range_min       = laser.rangeMin,
            // range_max       = laser.rangeMax,
            // ranges          = laser.ranges,      
            // intensities     = intensities
            angle_min       = -Mathf.PI,
            angle_max       = Mathf.PI,
            angle_increment = 0.007f,
            time_increment  = 0.1f / 897.0f,
            scan_time       = 0.1f,
            range_min       = 0.0f,
            range_max       = 20.0f,
            ranges          = laser.GetDistances(),
            intensities     = intensities
        };

        InvokeRepeating("PublishScan", 1f, 1f/publishRate);
    }

    void Update()
    {
    }

    private void PublishScan()
    {   
        laserScan.header = new HeaderMsg(
            Clock.GetCount(), new TimeStamp(Clock.time), laserLinkId
        );
        laserScan.ranges = laser.GetDistances();

        ros.Publish(laserTopicName, laserScan);
    }
}
