using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine;


public class Husky : Agent
{
    Vector3 linearVel;
    Vector3 angularVel;
    public float rotSpeed = 3;
    public float linSpeed = 5;
    public ArticulationBody rBody;

    public Transform Target;
    // private Vector3 lastPosition; // 保存上一帧的位置
    // private float lastDistance; // 上一帧与目标的距离
    // private float rewardThreshold = 0.2f; // 奖励阈值，可根据需要调整
    private void ResetArticulationBody()
    {


        // 重置ArticulationBody的位置和旋转到指定坐标
        // rBody.transform.localPosition = new Vector3( 0, 0, 0);
        // rBody.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
        rBody.TeleportRoot(new Vector3(8f, 0.2f, 8f), Quaternion.Euler(0f, 90f, 0f));
        // 也可以重置速度，使其不受上一轮的动力学影响
        rBody.velocity = Vector3.zero;
        rBody.angularVelocity = Vector3.zero;
    }
    // private Transform Tr;
    public override void OnEpisodeBegin()
    {
       // If the Agent fell, zero its momentum
        // lastPosition = transform.position;
        // lastDistance = Vector3.Distance(rBody.transform.localPosition, Target.localPosition);
        ResetArticulationBody();
        // rBody.transform.localPosition = new Vector3( 0, -0.48f, 0);
        
        // Move the target to a new spot
        Target.localPosition = new Vector3(Random.value * 10 - 3,
                                           0.5f,
                                           Random.value * 10 - 3);

        // Debug.Log(rBody.angularVelocity);
        // Debug.Log(rBody.velocity);
        // Debug.Log(rBody.transform.localPosition);

    }
 

    public override void CollectObservations(VectorSensor sensor)
    {
    // Debug.Log("CollectObservations run");
    // Target and Agent positions
    sensor.AddObservation(Target.localPosition);
    sensor.AddObservation(rBody.transform.localPosition);
    // sensor.AddObservation(rBody.transform.localRotation);
    // Debug.Log()
    // Agent velocity
    sensor.AddObservation(rBody.velocity.x);
    sensor.AddObservation(rBody.velocity.z);
    }
    // public float forceMultiplier = 10;
    private void controller(float inputLinVel, float inputAngVel)
    {
        linearVel = rBody.transform.forward * inputLinVel * linSpeed;
        angularVel = rBody.transform.up * inputAngVel * rotSpeed;
        rBody.velocity = linearVel;
        rBody.angularVelocity = angularVel;
    }
    public override void OnActionReceived(ActionBuffers actionBuffers) 
    {
    // Debug.Log("OnActionReceived run");
    // Actions, size = 2
    
    base.OnActionReceived(actionBuffers);
    var actions = actionBuffers.ContinuousActions;

    float linearVel = Mathf.Clamp(actions[0], -1, 1f);
    // linearVel = (linearVel + 1) / 2; 
    float angularVel = Mathf.Clamp(actions[1], -1, 1f);
    
    controller(linearVel, angularVel);

    // Rewards
    float distanceToTarget = Vector3.Distance(rBody.transform.localPosition, Target.localPosition);
    
    // Reached target
    if (distanceToTarget < 1.4f)
    {
        SetReward(1.0f);
        EndEpisode();
    }

    // Fell off platform
    if (rBody.transform.localPosition.y < 0.1f)
    {
        EndEpisode();
        SetReward(-1.0f);
    }
    
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Vertical");
        continuousActionsOut[1] = Input.GetAxis("Horizontal");
    }
}