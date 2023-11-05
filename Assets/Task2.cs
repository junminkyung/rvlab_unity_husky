using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.Robotics.UrdfImporter.Control;

public class HuskyDrive : Agent
{
    public ArticulationBody aBody;
    public GameObject wheelFL; // Front left wheel
    public GameObject wheelFR; // Front right wheel
    public GameObject wheelRL; // Rear left wheel
    public GameObject wheelRR; // Rear right wheel
    private ArticulationBody wA1;
    private ArticulationBody wA2;
    private ArticulationBody wA3;
    private ArticulationBody wA4;
    // public BoxCollider bumper;
    // private float _SpeedSensitivity = 1;
    public Transform Target;
    public float maxLinearSpeed = 1000.0f; 
    public float maxRotSpeed;
    private float wheelRadius = 0.330f; // meters
    private float forceLimit = 50;
    private float damping = 100;
    private float trackWidth = 0.670f;
    private RotationDirection direction;
    public float episodeTimeoutSeconds = 60.0f; // 에피소드의 최대 시간 (예: 60초)

    private float episodeStartTime; // 에피소드 시작 시간
    private void ResetArticulationBody()
    {
        // 重置ArticulationBody的位置和旋转到指定坐标
        aBody.TeleportRoot(new Vector3(-5.5f, 1.5f, 5.0f), Quaternion.Euler(0f, 90f, 0f));
        // 也可以重置速度，使其不受上一轮的动力学影响
        aBody.velocity = Vector3.zero;
        aBody.angularVelocity = Vector3.zero;
    }

    private void SetParameters(ArticulationBody joint)
    {
        ArticulationDrive drive = joint.xDrive;
        drive.forceLimit = forceLimit;
        drive.damping = damping;
        joint.xDrive = drive;
    }

    private void wheel_start()
    {
        wA1 = wheelFL.GetComponent<ArticulationBody>();
        wA2 = wheelFR.GetComponent<ArticulationBody>();
        wA3 = wheelRL.GetComponent<ArticulationBody>();
        wA4 = wheelRR.GetComponent<ArticulationBody>();
        SetParameters(wA1);
        SetParameters(wA2);
        SetParameters(wA3);
        SetParameters(wA4);
    }

    public override void Initialize()
    {   
        maxRotSpeed = Mathf.Rad2Deg * (maxLinearSpeed / wheelRadius);
        // bumper = GetComponent<BoxCollider>(); 
        wheel_start();
    }
    

    public override void OnEpisodeBegin()
    {
       // If the Agent fell, zero its momentum
        episodeStartTime = Time.time;
        ResetArticulationBody();
        
        // Move the target to a new spot
        Target.localPosition = new Vector3(Random.value * 10 - 5,
                                           1.5f,
                                           Random.value * 10 - 5);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Target and Agent positions
        sensor.AddObservation(Target.localPosition);
        sensor.AddObservation(aBody.transform.localPosition);

        // Agent velocity
        sensor.AddObservation(aBody.velocity.x);
        sensor.AddObservation(aBody.velocity.z);
    }

    void Drive(ArticulationBody joint, float wheelSpeed = float.NaN)
    {
        ArticulationDrive drive = joint.xDrive;
        drive.targetVelocity = wheelSpeed;
        joint.xDrive = drive;
    }

    void RL(float _Forward, float _Rot)
    {   
        float LinearSpeed = _Forward * maxLinearSpeed;
        float RotSpeed = _Rot * maxRotSpeed;
        Debug.Log($"LinearSpeed: {LinearSpeed}");
        Debug.Log($"RotSpeed: {RotSpeed}");

        float L_Rot = LinearSpeed  - (RotSpeed * trackWidth * 0.5f);
        float R_Rot = LinearSpeed  + (RotSpeed * trackWidth * 0.5f);
        Debug.Log($"L_Rot: {L_Rot}");
        Debug.Log($"R_Rot: {R_Rot}");

        Drive(wA1, L_Rot);
        Drive(wA2, R_Rot);
        Drive(wA3, L_Rot);
        Drive(wA4, R_Rot);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers) 
    {
        base.OnActionReceived(actionBuffers);

        float moveX = Mathf.Clamp(actionBuffers.ContinuousActions[0], -1f, 1f);
        float moveZ = Mathf.Clamp(actionBuffers.ContinuousActions[1], -1f, 1f);

        // Drive 메서드에 연속적인 행동 값 전달
        RL(moveZ, moveX);

        // Rewards
        float distanceToTarget = Vector3.Distance(aBody.transform.localPosition, Target.localPosition);

        // Reached target
        if (distanceToTarget < 1.4f)
        {
            SetReward(1.0f);
            EndEpisode();
        }

        if (Time.time - episodeStartTime >= episodeTimeoutSeconds)
        {
            EndEpisode();
        }

        AddReward(-0.01f);
    }

    void OnCollisionEnter(Collision coll)
    {
        // if (coll.collider.CompareTag("Target"))
        // {
        //     AddReward(1.0f);
        //     EndEpisode();
        // }

        if (coll.collider.CompareTag("Wall"))
        {   
            Debug.Log("Hit Wall");
            AddReward(-0.1f);
            EndEpisode();
        }

        if (coll.collider.CompareTag("Partition"))
        {   
            Debug.Log("Hit Partition");
            AddReward(-0.1f);
            EndEpisode();
        }
    }
    
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
    }
}