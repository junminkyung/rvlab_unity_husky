
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosColor = RosMessageTypes.UnityRoboticsDemo.UnityColorMsg;



public class sub_Test : MonoBehaviour
{

    public GameObject cube;
    // Start is called before the first frame update
    void Start()
    {
        ROSConnection.GetOrCreateInstance().Subscribe<RosColor>("color",ColorChange);
    }

    // Update is called once per frame
    void ColorChange(RosColor colorMessage)
    {

        cube.GetComponent<Renderer>().material.color = new Color32((byte)colorMessage.r, (byte)colorMessage.g,
            (byte)colorMessage.b, (byte)colorMessage.a);

    }
}
