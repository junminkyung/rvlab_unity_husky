using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnitySensors;

public class Task2_laser_addobstacles : Agent
{
    public ArticulationBody aBody;
    public float RotSpeed = 3;
    public float LinearSpeed = 5;
    public Transform Target;
    public Laser LiDAR_2D;
    
    void Start()
    {
    	aBody = GetComponent<ArticulationBody>(); 

    }

    public override void OnEpisodeBegin()
    {
        aBody.angularVelocity = Vector3.zero;
        aBody.velocity = Vector3.zero;
        // aBody.TeleportRoot(new Vector3(-5.5f, 0.0f, 6.0f), Quaternion.Euler(0f, 90f, 0f));

        float noiseMagnitude = 1.0f; // 노이즈 크기 조절
        Vector3 randomNoise = new Vector3(Random.Range(-noiseMagnitude, noiseMagnitude), 0, Random.Range(-noiseMagnitude, noiseMagnitude));
        Vector3 randomPosition = new Vector3(-5.5f, 0.0f, 6.0f) + randomNoise;
        Quaternion randomRotation = Quaternion.Euler(0f, Random.Range(60, 120), 0f);
        aBody.TeleportRoot(randomPosition, randomRotation);

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
        float LinearVel = Mathf.Clamp(actions[0], -1.0f, 1.0f);
        float AngularVel = Mathf.Clamp(actions[1], -1.0f, 1.0f);

        Drive(LinearVel, AngularVel);

        // Agent와 Target사이의 거리를 측정
        float distanceToTarget = Vector3.Distance(aBody.transform.localPosition, Target.localPosition);
   
        // Target에 도달하는 경우 (거리가 1.42보다 작은 경우) Episode 종료
        if (distanceToTarget < 1.42f)
        {   
            SetReward(1.0f);
            EndEpisode();
        }
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
        
        if (coll.gameObject.CompareTag("Obstacle"))
        {   
            // Debug.Log("Hit Obstacle");
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