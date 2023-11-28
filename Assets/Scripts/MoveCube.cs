using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine;

public class MoveCube : Agent
{
    // rBody라는 Rigidbody 함수 정의
    Rigidbody rBody;
    public Transform Target;
    public float Speed = 5;

    void Start()
    {
    	// rBody함수는 현재 GameObject의 Rigidbody Component를 참조
    	rBody = GetComponent<Rigidbody>(); 
    }

    public override void OnEpisodeBegin()
    {
        // Agent가 플랫폼 외부로 떨어지면(Y 좌표가 0이하가 되면), angularVelocity/velocity=0으로, 위치를 초기 좌표로 리셋
        if (this.transform.localPosition.y < 0)
        {
            this.rBody.angularVelocity = Vector3.zero;
            this.rBody.velocity = Vector3.zero;
            this.transform.localPosition = new Vector3 (1.0f, 0.0f, -0.5f);
        }

        // Target을 Random.value함수를 활용해서 새로운 무작위 위치에 이동
        Target.localPosition = new Vector3(2.0f, 0.5f, 2.0f);
    }
 

    public override void CollectObservations(VectorSensor sensor)
    {
        // Target/Agent의 위치 정보 수집
        sensor.AddObservation(Target.localPosition);
        sensor.AddObservation(this.transform.localPosition);

        // Agent의 velocity 정보 수집
        sensor.AddObservation(rBody.velocity.x);
        sensor.AddObservation(rBody.velocity.z);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        base.OnActionReceived(actionBuffers);
        var actions = actionBuffers.ContinuousActions;
        float xMove = actions[0];
        float zMove = actions[1];

        Vector3 getVel = new Vector3(xMove, 0, zMove) * Speed;
        rBody.velocity = getVel;

        // Agent와 Target사이의 거리를 측정
        float distanceToTarget = Vector3.Distance(rBody.transform.localPosition, Target.localPosition);

        // Target에 도달하는 경우 (거리가 1.42보다 작은 경우) Episode 종료
        if (distanceToTarget < 1.42f)
        {
            SetReward(1.0f);
            EndEpisode();
        }

        // 플랫폼 밖으로 나가면 Episode 종료
        if (this.transform.localPosition.y < 0)
        {
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