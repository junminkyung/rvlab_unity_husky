using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosScan = RosMessageTypes.Sensor.LaserScanMsg;
using RosVel = RosMessageTypes.Geometry.TwistMsg;
using RosPos = RosMessageTypes.Nav.OdometryMsg;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;


public class RosSubscriberExample : MonoBehaviour
{
    public GameObject husky;
    public float[] ranges;
    public float linear_x;
    public float linear_z;
    public Vector3 husky_linear_vel;
    public Vector3 husky_pos;
    
    void Start()
    {
        ROSConnection.GetOrCreateInstance().Subscribe<RosScan>("/scan", ScanCallback);
        ROSConnection.GetOrCreateInstance().Subscribe<RosVel>("/husky_velocity_controller/cmd_vel", VelCallback);
        ROSConnection.GetOrCreateInstance().Subscribe<RosPos>("/husky_velocity_controller/odom", OdomCallback);
    }


    void ScanCallback(RosScan laserMsg)
    {
        ranges = new float[laserMsg.ranges.Length];
        for (int i = 0; i < laserMsg.ranges.Length; i++)
        {
            // 'inf' 또는 'NaN' 값이면 유효한 값(예: d_max)으로 대체합니다.
            if (float.IsInfinity(laserMsg.ranges[i]) || float.IsNaN(laserMsg.ranges[i]))
            {
                ranges[i] = 200; // d_max는 유효한 최대 거리 값으로 설정해야 합니다.
            }
            else
            {
                ranges[i] = laserMsg.ranges[i];
            }
            // Debug.Log($"ranges[{i}]: {ranges[i]}");
        }

    }
    
    void VelCallback(RosVel velMsg)
    {   
        // husky_linear_vel = velMsg.From<FLU>();
        linear_z = (float)velMsg.linear.x;
        linear_x = (float)velMsg.linear.y;
        // linear_x = float.IsNaN((float)velMsg.linear.x) ? 0.0f : (float)velMsg.linear.x;
        // linear_z = float.IsNaN((float)velMsg.linear.y) ? 0.0f : (float)velMsg.linear.y;
        // Debug.Log($"linear_x: {linear_x}");
        // Debug.Log($"linear_z: {linear_z}");
    }
    
    void OdomCallback(RosPos odometryMsg)
    {
        // husky_pos.x = (float)odometryMsg.pose.pose.position.x;
        // husky_pos.y = (float)odometryMsg.pose.pose.position.z;
        // husky_pos.z = (float)odometryMsg.pose.pose.position.y;
        // husky_pos.x = float.IsNaN((float)odometryMsg.pose.pose.position.x) ? 0.0f : (float)odometryMsg.pose.pose.position.x;
        // husky_pos.y = float.IsNaN((float)odometryMsg.pose.pose.position.z) ? 0.0f : (float)odometryMsg.pose.pose.position.z;
        // husky_pos.z = float.IsNaN((float)odometryMsg.pose.pose.position.y) ? 0.0f : (float)odometryMsg.pose.pose.position.y;
        husky_pos = odometryMsg.pose.pose.position.From<FLU>();
        // Debug.Log($"husky_pos: {husky_pos}");
    }

}