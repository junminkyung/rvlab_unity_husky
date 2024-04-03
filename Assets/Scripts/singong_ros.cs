using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnitySensors;

public class singong_ros : Agent
{
    public ArticulationBody aBody;
    public float RotSpeed = 2.0f;
    public float LinearSpeed = 0.8f;
    public Transform Target;
    // public VelodyneSensor LiDAR;
    public RosSubscriberExample ROS;
    
    public float episodeTimeoutSeconds = 300.0f; // 에피소드의 최대 시간 (예: 60초)
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
        // 두 개의 위치 벡터 정의
        // Vector3 positionOption1 = new Vector3(-0.75f, 0.0f, -25.0f);
        // Vector3 positionOption2 = new Vector3(-7.76f, 0.0f, 25.9f);
        // 랜덤하게 하나의 위치 선택
        // Vector3 selectedPosition = Random.Range(0, 2) == 0 ? positionOption1 : positionOption2;
        // Quaternion randomRotation = Quaternion.Euler(0f, Random.Range(0, 360), 0f);
        
        aBody.TeleportRoot(new Vector3(-0.75f, 0.0f, -25.0f), Quaternion.Euler(0f, 0f, 0f));
        // aBody.TeleportRoot(selectedPosition, Quaternion.Euler(0f, 0f, 0f));
        
        episodeStartTime = Time.time; 

        Target.localPosition = new Vector3(14.9f, 1.0f, 25.7f);

        preDist = Vector3.Magnitude(Target.position - aBody.transform.position);
    }

    public override void CollectObservations(VectorSensor sensor)
    {   
        // Target/Agent의 위치 정보 수집
        sensor.AddObservation(Target.localPosition);
        // sensor.AddObservation(aBody.transform.localPosition);
        // Debug.Log($"aBody.transform.localPosition: {aBody.transform.localPosition}");
        
        sensor.AddObservation(ROS.husky_pos);
        // Debug.Log($"ROS.husky_pos: {ROS.husky_pos}");

        // Agent의 velocity 정보 수집
        // sensor.AddObservation(aBody.velocity.x);
        // sensor.AddObservation(aBody.velocity.z);
        
        sensor.AddObservation(ROS.linear_x);
        sensor.AddObservation(ROS.linear_z);
        
        // Debug.Log($"ROS.linear_x: {ROS.linear_x}");
        // Debug.Log($"ROS.linear_z: {ROS.linear_z}");
        
        float[] laserScanRanges = ROS.ranges;
        // float[] laserScanRanges = LiDAR.GetDistances();

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

        if (distance <= 1.65f)
        {
            SetReward(10.0f);
            EndEpisode();
        }
        else
        {
            float reward = preDist - distance;
            // Debug.Log($"reward: {preDist}, {distance}, {reward}");
            SetReward(reward);
            preDist = distance;
        }
        
        // Debug.Log($"distance: {distance}");
        //
        // if (distance > 43.5f && distance <= 53.5f)
        //     {
        //         // Debug.Log("stage 1"); 
        //         SetReward(0.01f);
        //     }
        //     else if (distance > 33.5f && distance <= 43.5f)
        //     {   
        //         // Debug.Log("stage 2"); 
        //         SetReward(0.02f);
        //     }
        //     else if (distance > 16f && distance <= 33.5f)
        //     {   
        //         // Debug.Log("stage 3"); 
        //         SetReward(0.03f);
        //     }
        //     else if (distance > 15f && distance <= 16f)
        //     {   
        //         // Debug.Log("stage 4"); 
        //         SetReward(0.04f);
        //     }
        //     else if (distance > 1.6f && distance <= 15f)
        //     {   
        //         // Debug.Log("stage 5"); 
        //         SetReward(0.05f);
        //     }
        //     else if (distance <= 1.6f)
        //     {   
        //         // Debug.Log("Finish");
        //         SetReward(1.0f);
        //         EndEpisode();
        //     }
        
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