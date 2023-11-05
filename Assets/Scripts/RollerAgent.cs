using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class RollerAgent : Agent
{
    
    // aBody라는 Rigidbody 함수 정의
    Vector3 linearVel;
    Vector3 angularVel;
    public float rotSpeed = 3;
    public float linSpeed = 5;
    public ArticulationBody aBody;
    
    void Start()
    {
    	// aBody함수는 현재 GameObject의 Rigidbody Component를 참조
    	aBody = GetComponent<ArticulationBody>(); 
    }

    // Target이라는 public Transform 함수를 선언하여 차후 Inspector 윈도우에서 지정 
    public Transform Target;
    public override void OnEpisodeBegin()
    {   
        aBody.TeleportRoot(new Vector3(0f, 0.5f, 0f), Quaternion.Euler(0f, 90f, 0f));
        aBody.velocity = Vector3.zero;
        aBody.angularVelocity = Vector3.zero;

        // Target을 Random.value함수를 활용해서 새로운 무작위 위치에 이동
        Target.localPosition = new Vector3(Random.value * 8 - 4, 0.5f, Random.value * 8 - 4);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Target/Agent의 위치 정보 수집
        sensor.AddObservation(Target.localPosition);
        sensor.AddObservation(this.transform.localPosition);

        // Agent의 velocity 정보 수집
        sensor.AddObservation(aBody.velocity.x);
        sensor.AddObservation(aBody.velocity.z);
    }

    private void controller(float inputLinVel, float inputAngVel)
    {
        linearVel = aBody.transform.forward * inputLinVel * linSpeed;
        angularVel = aBody.transform.up * inputAngVel * rotSpeed;
        aBody.velocity = linearVel;
        aBody.angularVelocity = angularVel;
    }

    // public float forceMultiplier = 500;
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
    
    	base.OnActionReceived(actionBuffers);
        var actions = actionBuffers.ContinuousActions;

        float linearVel = Mathf.Clamp(actions[0], -1, 1f);
        // linearVel = (linearVel + 1) / 2; 
        float angularVel = Mathf.Clamp(actions[1], -1, 1f);
        
        controller(linearVel, angularVel);

        // Agent와 Target사이의 거리를 측정
        float distanceToTarget = Vector3.Distance(this.transform.localPosition, Target.localPosition);

        // Target에 도달하는 경우 (거리가 1.42보다 작은 경우) Episode 종료
        if (distanceToTarget < 1.42)
        {
            SetReward(1.0f);
            EndEpisode();
        }

        // 플랫폼 밖으로 나가면 Episode 종료
        if (this.transform.localPosition.y < 0)
        {   
            SetReward(-0.1f);
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