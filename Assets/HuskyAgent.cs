using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class HuskyAgent : Agent
{   
    Rigidbody rBody;

    const int stop = 0;  // do nothing!
    const int forward = 1;
    const int backward = 2;
    const int left = 3;
    const int right = 4;

    // Start is called before the first frame update
    void Start()
    {
        rBody = GetComponent<Rigidbody>();
    }

    public Transform Target;

    public override void CollectObservations(VectorSensor sensor)
    {   
        // 목적지(Target)와 로봇의 위치 좌표
        sensor.AddObservation(Target.transform.position.x);
        sensor.AddObservation(Target.transform.position.z);
        sensor.AddObservation(rBody.transform.position.x);
        sensor.AddObservation(rBody.transform.position.z);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("WallE") || collision.gameObject.CompareTag("WallN") || collision.gameObject.CompareTag("WallS") || collision.gameObject.CompareTag("WallW"))
        {   //벽에 닿으면 AddReward -0.5점
            Debug.Log("Hit Wall");
            AddReward(-0.5f);
        }
        else if (collision.gameObject.CompareTag("Target"))
        {   // 골에 닿으면 AddReward +1점
            Debug.Log("Hit Goal");
            AddReward(1.0f);
            //에피소드 종료
            EndEpisode();
        }
    }

    public float forceMultiplier = 10;
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {   
        // var dir = Vector3.zero;
        // var rot = Vector3.zero;

        // var action = actionBuffers.DiscreteActions[0];

        // // var targetPos = transform.position;
        // switch (action)
        // {
        //     case stop:
        //         // do nothing
        //         break;
        //     case forward:
        //         dir = transform.forward * 1f;
        //         break;
        //     case backward:
        //         dir = transform.forward * -1f;
        //         break;
        //     case left:
        //         dir = transform.up * -1f;
        //         break;
        //     case right:
        //         dir = transform.up * 1f;
        //         break;
        // }
        // transform.Rotate(rot, Time.deltaTime * 100f);
        // rBody.AddForce(dir * 0.5f, ForceMode.VelocityChange);

        // AddReward(-0.1f / MaxStep);

        var dir = Vector3.zero;
        var rot = Vector3.zero;

        var continuousActions = actionBuffers.ContinuousActions;

        // 첫 번째 요소로 왼쪽 및 오른쪽 회전을 설정
        float rotation = continuousActions[0];
        rot = transform.up * rotation;

        // 나머지 요소를 사용하여 이동 방향 설정
        float move = continuousActions[1];
        dir = transform.forward * move;

        transform.Rotate(rot, Time.deltaTime * 100f);
        rBody.AddForce(dir * 0.5f, ForceMode.VelocityChange);

        AddReward(-0.1f / MaxStep);

        // Rewards
        float distanceToTarget = Vector3.Distance(this.transform.localPosition, Target.localPosition);

        // Reached target
        if (distanceToTarget < 1.0f)
        {
            SetReward(1.0f);
            EndEpisode();
        }

    }

    public override void OnEpisodeBegin()
    {
        rBody.transform.localPosition = new Vector3(-6.7f, 1.5f, 6.7f);
        Target.transform.localPosition = new Vector3(6.5f, 2.0f, -6.5f);

        rBody.velocity = Vector3.zero;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // Discrete action

        // var discreteActionsOut = actionsOut.DiscreteActions;
        // discreteActionsOut[0] = stop;

        // if (Input.GetKey(KeyCode.D))
        // {
        //     discreteActionsOut[0] = right;
        // }
        // else if (Input.GetKey(KeyCode.W))
        // {
        //     discreteActionsOut[0] = forward;
        // }
        // else if (Input.GetKey(KeyCode.A))
        // {
        //     discreteActionsOut[0] = left;
        // }
        // else if (Input.GetKey(KeyCode.S))
        // {
        //     discreteActionsOut[0] = backward;
        // }

        // continuous action
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
    }
    
}
