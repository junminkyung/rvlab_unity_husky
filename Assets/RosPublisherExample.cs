using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.UnityRoboticsDemo;

/// <summary>
///
/// </summary>
public class RosPublisherExample : MonoBehaviour
{
    ROSConnection ros;
    public string topicName = "cmd_vel";

    // The game object
    public GameObject husky;
    // Publish the cube's position and rotation every N seconds
    public float publishMessageFrequency = 0.5f;

    // Used to determine how much time has elapsed since the last message was published
    private float timeElapsed;

    void Start()
    {
        // start the ROS connection
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<PosRotMsg>(topicName);
    }

    private void Update()
    {
        timeElapsed += Time.deltaTime;

        if (timeElapsed > publishMessageFrequency)
        {
            husky.transform.rotation = Random.rotation;

            PosRotMsg huskyPos = new PosRotMsg(
                husky.transform.position.x,
                husky.transform.position.y,
                husky.transform.position.z,
                husky.transform.rotation.x,
                husky.transform.rotation.y,
                husky.transform.rotation.z,
                husky.transform.rotation.w
            );

            // Finally send the message to server_endpoint.py running in ROS
            ros.Publish(topicName, huskyPos);

            timeElapsed = 0;
        }
    }
}