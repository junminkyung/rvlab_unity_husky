
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.UnityRoboticsDemo;



public class test_Scrips : MonoBehaviour
{
    ROSConnection ros;
    public string topicName = "pos_rot";

    public GameObject cube;
    public float publishMessageFrequency = 0.5f;

    private float timeElapsed;
    
    // Start is called before the first frame update
    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<PosRotMsg>(topicName);
    }

    // Update is called once per frame
    private void Update()
    {
        timeElapsed += Time.deltaTime;

        if (timeElapsed > publishMessageFrequency)
        {
            cube.transform.rotation = Random.rotation;

            PosRotMsg cubePos = new PosRotMsg(
            cube.transform.position.x,
            cube.transform.position.y,
            cube.transform.position.z,
            cube.transform.rotation.x,
            cube.transform.rotation.y,
            cube.transform.rotation.z,
            cube.transform.rotation.w
                );
            
            ros.Publish(topicName, cubePos);
            timeElapsed = 0;

        }

    }
    
    
}
