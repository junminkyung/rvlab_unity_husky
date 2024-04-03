using System;
using UnityEngine;
using RosMessageTypes.Geometry;
using RosMessageTypes.Std;
using Unity.Robotics.Core;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;

/// <summary>
///     This script publishes robot stamped twist
///     with respect to the local robot frame
/// </summary>
public class TwistMsgPublisher : MonoBehaviour
{
    // ROS Connector
    private ROSConnection ros;
    // Variables required for ROS communication
    public string twistTopicName = "joy_teleop/cmd_vel";  // Change the topic name if needed

    // Transform
    public Transform publishedTransform;
    private Vector3 previousPosition;
    private Vector3 previousRotation;
    private Vector3 linearVelocity;
    private Vector3 angularVelocity;

    // Message
    private TwistMsg twist;
    // private string frameId = "joy_teleop";  // Change the frame ID if needed
    public float publishRate = 1f;
    private float deltaTime;

    void Start()
    {
        // Get ROS connection static instance
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<TwistMsg>(twistTopicName);

        previousPosition = publishedTransform.position;
        previousRotation = publishedTransform.rotation.eulerAngles;

        // Initialize message
        twist = new TwistMsg();

        deltaTime = 1f / publishRate;
        InvokeRepeating("PublishTwist", 1f, deltaTime);
    }

    private void PublishTwist()
    {
        // Linear
        linearVelocity = (publishedTransform.position - previousPosition) / deltaTime;
        linearVelocity = publishedTransform.InverseTransformDirection(linearVelocity);
        previousPosition = publishedTransform.position;

        // Angular
        angularVelocity = (publishedTransform.rotation.eulerAngles - previousRotation) / deltaTime * Mathf.Deg2Rad;
        angularVelocity = publishedTransform.InverseTransformDirection(angularVelocity);
        // Using Vector3 euler angles instead of Quaternion to compute angular velocity
        // Need to adjust the result
        angularVelocity = -angularVelocity;
        previousRotation = publishedTransform.rotation.eulerAngles;

		// angularVelocity.x = Mathf.Clamp(angularVelocity.x, -1.2f, 1.2f);
    	// angularVelocity.y = Mathf.Clamp(angularVelocity.y, -1.2f, 1.2f);
    	// angularVelocity.z = Mathf.Clamp(angularVelocity.z, -1.2f, 1.2f);

        twist.linear = linearVelocity.To<FLU>();
        twist.angular = angularVelocity.To<FLU>();

        ros.Publish(twistTopicName, twist);
    }
}