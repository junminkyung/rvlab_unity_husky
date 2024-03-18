using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnitySensors;

public class singong_ros_long : Agent
{
    public ArticulationBody aBody;
    public float RotSpeed = 1.2f;
    public float LinearSpeed = 0.5f;
    public Transform Target;
    public VelodyneSensor LiDAR;
    // public RosSubscriberExample ROS;
    
    public float episodeTimeoutSeconds = 200.0f; // 에피소드의 최대 시간 (예: 60초)
    private float episodeStartTime; // 에피소드 시작 시간

    public float preDist;
    
    void Start()
    {
    	aBody = GetComponent<ArticulationBody>(); 

    }

    public override void OnEpisodeBegin()
    {
        aBody.angularVelocity = Vector3.zero;
        aBody.velocity = Vector3.zero;
        aBody.TeleportRoot(new Vector3(21.0f, 0.0f, -3.5f), Quaternion.Euler(0f, -90f, 0f));

        episodeStartTime = Time.time; 

        // Target을 Random함수를 활용해서 새로운 무작위 위치에 이동
        Target.localPosition = new Vector3(-20.7f, 0.0f, 29.8f);

        preDist = Vector3.Magnitude(Target.position - aBody.transform.position);
    }

    public override void CollectObservations(VectorSensor sensor)
    {   
        // Target/Agent의 위치 정보 수집
        sensor.AddObservation(Target.localPosition);
        sensor.AddObservation(aBody.transform.localPosition);
        // Debug.Log($"aBody.transform.localPosition: {aBody.transform.localPosition}");
        
        // sensor.AddObservation(ROS.husky_pos);
        // Debug.Log($"ROS.husky_pos: {ROS.husky_pos}");

        // Agent의 velocity 정보 수집
        sensor.AddObservation(aBody.velocity.x);
        sensor.AddObservation(aBody.velocity.z);
        
        // sensor.AddObservation(ROS.linear_x);
        // sensor.AddObservation(ROS.linear_z);
        
        // Debug.Log($"ROS.linear_x: {ROS.linear_x}");
        // Debug.Log($"ROS.linear_z: {ROS.linear_z}");
        
        // float[] laserScanRanges = ROS.ranges;
        float[] laserScanRanges = LiDAR.GetDistances();

        foreach (float range in laserScanRanges)
        {
            sensor.AddObservation(range);
            // Debug.Log($"range: {range}");
        }
    }

    private void Drive(float Input_Linear_Vel, float Input_Angular_Vel)
    {
        aBody.velocity = aBody.transform.forward * Input_Linear_Vel * LinearSpeed;
        aBody.angularVelocity = aBody.transform.up * Input_Angular_Vel * RotSpeed;
    }
    
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        base.OnActionReceived(actionBuffers);
        var actions = actionBuffers.ContinuousActions;
        float LinearVel = Mathf.Clamp(actions[0], -1.0f, 1.0f);
        float AngularVel = Mathf.Clamp(actions[1], -1.0f, 1.0f);

        Drive(LinearVel, AngularVel);

        // Agent와 Target사이의 거리를 측정
        float distance = Vector3.Magnitude(Target.position - aBody.transform.position);
		// Debug.Log($"distance: {distance}");
        if (distance <= 1.5f)
        {
            SetReward(10.0f);
            EndEpisode();
        }
        else
        {
            float reward = preDist - distance;
            // Debug.Log($"reward: {preDist}, {distance
            // }, {reward}");
            SetReward(reward);
            preDist = distance;
        }
        
        if (Time.time - episodeStartTime >= episodeTimeoutSeconds)
        {
            EndEpisode();
        }
        
        // AddReward(-0.01f);
    }

    void OnCollisionEnter(Collision coll)
    {
        if (coll.gameObject.CompareTag("Wall"))
        {   
            // Debug.Log("Hit Wall");
            SetReward(-1.0f);
            EndEpisode();
        }

    }
    
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Vertical");
        continuousActionsOut[1] = Input.GetAxis("Horizontal");
    }
}