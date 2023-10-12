using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class HuskyAgent : Agent
{   
    Rigidbody rBody;

    // Start is called before the first frame update
    void Start()
    {
        rBody = GetComponent<Rigidbody>();
    }

    public Transform Target;

    public override void CollectObservations(VectorSensor sensor)
    {   
        // 목적지(Target)와 로봇의 위치 좌표
        sensor.AddObservation(Target.transform.x);
        sensor.AddObservation(Target.transform.z);
        sensor.AddObservation(rBody.transform.x);
        sensor.AddObservation(rBody.transform.z);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Wall"))
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

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {   
        var dir = Vector3.zero;
        var rot = Vector3.zero;

        var action = actionBuffers.DiscreteActions[0];

        var targetPos = transform.position;
        switch (action)
        {
            case k_NoAction:
                // do nothing
                break;
            case forward:
                targetPos = transform.forward * 1f;
                break;
            case backward:
                targetPos = transform.forward * -1f;
                break;
            case left:
                targetPos = transform.up * -1f;
                break;
            case right:
                targetPos = transform.up * 1f;
                break;
        }
        transform.Rotate(rot, Time.deltaTime * 100f);
        rBody.AddForce(dir * 0.5f, ForceMode.VelocityChange);

        AddReward(-0.1f / MaxStep)
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = -Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
    }
    
}
