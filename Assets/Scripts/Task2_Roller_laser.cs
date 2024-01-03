using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnitySensors;

public class Task2_Roller_laser : Agent
{
    public ArticulationBody aBody;
    public float RotSpeed = 3;
    public float LinearSpeed = 5;
    public Transform Target;
    public Laser LiDAR_2D;
    
    public float episodeTimeoutSeconds = 60.0f; // 에피소드의 최대 시간 (예: 60초)
    private float episodeStartTime; // 에피소드 시작 시간
    
    void Start()
    {
    	aBody = GetComponent<ArticulationBody>(); 

    }

    public override void OnEpisodeBegin()
    {
        aBody.angularVelocity = Vector3.zero;
        aBody.velocity = Vector3.zero;
        aBody.TeleportRoot(new Vector3(-5.5f, 0.0f, 6.0f), Quaternion.Euler(0f, 90f, 0f));

        episodeStartTime = Time.time; 
        // float noiseMagnitude = 1.0f; // 노이즈 크기 조절
        // Vector3 randomNoise = new Vector3(Random.Range(-noiseMagnitude, noiseMagnitude), 0, Random.Range(-noiseMagnitude, noiseMagnitude));
        // Vector3 randomPosition = new Vector3(-5.5f, 0.0f, 6.0f) + randomNoise;
        // Quaternion randomRotation = Quaternion.Euler(0f, Random.Range(60, 120), 0f);
        // aBody.TeleportRoot(randomPosition, randomRotation);

        // Target을 Random함수를 활용해서 새로운 무작위 위치에 이동
        Target.localPosition = new Vector3(6.0f, 1.8f, -6.5f);
        // Target.localPosition = new Vector3(Random.Range(-1.0f, 7.0f), 1.8f, Random.Range(-4.5f, -7.0f));
    }

    public override void CollectObservations(VectorSensor sensor)
    {   
        // Target/Agent의 위치 정보 수집
        sensor.AddObservation(Target.localPosition);
        sensor.AddObservation(aBody.transform.localPosition);

        // Agent의 velocity 정보 수집
        sensor.AddObservation(aBody.velocity.x);
        sensor.AddObservation(aBody.velocity.z);

        float[] laserScanRanges = LiDAR_2D.GetCurrentScanRanges();
        foreach (float range in laserScanRanges)
        {
            sensor.AddObservation(range);
            // Debug.Log(range);
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
        float LinearVel = Mathf.Clamp(actions[0], 0.0f, 1.0f);
        float AngularVel = Mathf.Clamp(actions[1], -1.0f, 1.0f);

        Drive(LinearVel, AngularVel);

        // Agent와 Target사이의 거리를 측정
        float distanceToTarget = Vector3.Distance(aBody.transform.localPosition, Target.localPosition);
        // Debug.Log($"Distance to Target: {distanceToTarget}");
        
        // Target에 도달하는 경우 Episode 종료(거리에 따른 보상)
        // if (distanceToTarget > 14.0f && distanceToTarget <= 17.0f)
        // {
        //     // Debug.Log("stage 1"); // 직선주행
        //     AddReward(0.5f);
        // }
        // else if (distanceToTarget > 8.0f && distanceToTarget <= 14.0f)
        // {   
        //     // Debug.Log("stage 2"); // 코너주행
        //     AddReward(1.0f);
        // }
        // else if (distanceToTarget > 1.42f && distanceToTarget <= 8.0f)
        // {   
        //     // Debug.Log("stage 3"); // 코너 꺾고 목적지가 있는 곳으로 직선주행
        //     AddReward(1.5f);
        // }
        // else if (distanceToTarget <= 1.42f)
        // {   
        //     // Debug.Log("Finish");
        //     SetReward(10.0f);
        //     EndEpisode();
        // }
        
        // if (distanceToTarget > 13.0f && distanceToTarget <= 17.0f)
        // {
        //     // Debug.Log("stage 1"); 
        //     SetReward(0.5f);
        // }
        // else if (distanceToTarget > 9.0f && distanceToTarget <= 13.0f)
        // {   
        //     // Debug.Log("stage 2);
        //     SetReward(1.0f);
        // }
        // else if (distanceToTarget > 5.0f && distanceToTarget <= 9.0f)
        // {   
        //     // Debug.Log("stage 3"); 
        //     SetReward(1.5f);
        // }
        // else if (distanceToTarget > 1.5f && distanceToTarget <= 5.0f)
        // {   
        //     // Debug.Log("stage 3"); 
        //     SetReward(2.0f);
        // }
        // else if (distanceToTarget <= 1.5f)
        // {   
        //     // Debug.Log("Finish");
        //     SetReward(2.5f);
        //     EndEpisode();
        // }
        
        
        // if (distanceToTarget < 1.42f)
        // {   
        //     SetReward(1.0f);
        //     EndEpisode();
        // }
        
        
        // if (distanceToTarget > 14.5f && distanceToTarget <= 17.0f)
        // {
        //     // Debug.Log("stage 1"); 
        //     AddReward(0.001f);
        // }
        // else if (distanceToTarget > 13.3f && distanceToTarget <= 14.5f)
        // {   
        //     // Debug.Log("stage 2"); 
        //     AddReward(0.002f);
        // }
        // else if (distanceToTarget > 10.8f && distanceToTarget <= 13.3f)
        // {   
        //     // Debug.Log("stage 3"); 
        //     AddReward(0.003f);
        // }
        // else if (distanceToTarget > 5.5f && distanceToTarget <= 10.8f)
        // {   
        //     // Debug.Log("stage 4"); 
        //     AddReward(0.004f);
        // }
        // else if (distanceToTarget > 1.42f && distanceToTarget <= 5.5f)
        // {   
        //     // Debug.Log("stage 5"); 
        //     AddReward(0.005f);
        // }
        // else if (distanceToTarget <= 1.42f)
        // {   
        //     // Debug.Log("Finish");
        //     SetReward(10.0f);
        //     EndEpisode();
        // }
        //
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
            EndEpisode();
        }

        if (coll.gameObject.CompareTag("Partition"))
        {   
            // Debug.Log("Hit Partition");
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