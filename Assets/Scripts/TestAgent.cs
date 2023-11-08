using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.Robotics.UrdfImporter.Control;

public class TestAgent : Agent
{   
    public ArticulationBody aBody;
    public float maxLinearSpeed = 5.0f; // m/s
    private float _wheelRadius = 0.330f; // m
    public float _maxrotspeed; // m/s
    private float _damping = 400; // N*s/m
    private float _SpeedSensitivity = 1;
    private float _DampingSensitivity = 1;
    // public Transform WallE;
    // public Transform WallN;
    // public Transform WallS;
    // public Transform WallW;
    private float forceLimit = 500;
    public GameObject wheelFL; // Front left wheel
    public GameObject wheelFR; // Front right wheel
    public GameObject wheelRL; // Rear left wheel
    public GameObject wheelRR; // Rear right wheel
    private ArticulationBody wA1;
    private ArticulationBody wA2;
    private ArticulationBody wA3;
    private ArticulationBody wA4;
    private RotationDirection direction;
    public float episodeTimeoutSeconds = 60.0f; // 에피소드의 최대 시간 (예: 60초)

    private float episodeStartTime; // 에피소드 시작 시간

    void Start()
    {
        _maxrotspeed = Mathf.Rad2Deg * maxLinearSpeed / _wheelRadius;
        aBody = GetComponent<ArticulationBody>(); 
        wA1 = wheelFL.GetComponent<ArticulationBody>();
        wA2 = wheelFR.GetComponent<ArticulationBody>();
        wA3 = wheelRL.GetComponent<ArticulationBody>();
        wA4 = wheelRR.GetComponent<ArticulationBody>();
        Set_Properties(wA1);
        Set_Properties(wA2);
        Set_Properties(wA3);
        Set_Properties(wA4);
    }

    void Drive(ArticulationBody joint, float wheelSpeed = float.NaN)
    {
        ArticulationDrive drive = joint.xDrive;
        if (float.IsNaN(wheelSpeed))
        {
            drive.targetVelocity = ((2 * maxLinearSpeed) / _wheelRadius) * Mathf.Rad2Deg * (int)direction;
        }
        else
        {
            drive.targetVelocity = wheelSpeed;
        }
        joint.xDrive = drive;
    }

    void RL(float _Forward, float _Rot)
    {   
        float max_1 = Mathf.Max(Mathf.Abs(_Forward) + Mathf.Abs(_Rot), 1.0f);
        float L_Rot = (_Forward + _Rot) * _maxrotspeed / max_1 * _SpeedSensitivity;
        float R_Rot = (_Forward - _Rot) * _maxrotspeed / max_1 * _SpeedSensitivity;
        Drive(wA1, L_Rot);
        Drive(wA2, R_Rot);
        Drive(wA3, L_Rot);
        Drive(wA4, R_Rot);
    }

    void Set_Properties(ArticulationBody joint)
    {
        ArticulationDrive drive = joint.xDrive;
        drive.damping = _damping * _DampingSensitivity;
        joint.xDrive = drive;
        drive.forceLimit = forceLimit;
    }

    public Transform Target;
    public override void OnEpisodeBegin()
    {   
        // aBody.transform.localPosition = new Vector3 (-6.7f, 1.5f, 6.7f);
        Target.localPosition = new Vector3(Random.value * 10 - 5,
                                           2.0f,
                                           Random.value * 10 - 5);

        aBody.TeleportRoot(new Vector3 (-6.7f, 1.5f, 6.7f), Quaternion.Euler(0f, 90f, 0f));
        aBody.angularVelocity = Vector3.zero;
        aBody.velocity = Vector3.zero;
    }

    public override void CollectObservations(VectorSensor sensor)
    {   
        // 목적지(Target)와 로봇(Agent)의 위치 좌표
        sensor.AddObservation(Target.localPosition);
        sensor.AddObservation(aBody.transform.localPosition);

        // 로봇(Agent)의 속도 정보 수집
        sensor.AddObservation(aBody.velocity.x);
        sensor.AddObservation(aBody.velocity.z);
    }

    // private void OnCollisionEnter(Collision collision)
    // {
    //     if(collision.gameObject.CompareTag("Wall"))
    //     {   //벽에 닿으면 AddReward -0.5점
    //         Debug.Log("Hit Wall");
    //         AddReward(-0.1f);
    //         EndEpisode();
    //     }

    // }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {   
        base.OnActionReceived(actionBuffers);

        float moveX = Mathf.Clamp(actionBuffers.ContinuousActions[0], -1f, 1f);
        float moveZ = Mathf.Clamp(actionBuffers.ContinuousActions[1], -1f, 1f);

        // Drive 메서드에 연속적인 행동 값 전달
        RL(moveZ, moveX);

        // Agent와 Target사이의 거리를 측정
        float distanceToTarget = Vector3.Distance(aBody.transform.localPosition, Target.localPosition);
        // float distanceToWallE = Vector3.Distance(aBody.transform.localPosition, WallE.localPosition);
        // float distanceToWallN = Vector3.Distance(aBody.transform.localPosition, WallN.localPosition);
        // float distanceToWallS = Vector3.Distance(aBody.transform.localPosition, WallS.localPosition);
        // float distanceToWallW = Vector3.Distance(aBody.transform.localPosition, WallW.localPosition);

        // Target에 도달하는 경우 (거리가 1.4보다 작은 경우) Episode 종료
        if (distanceToTarget < 1.8f)
        {
            SetReward(1.0f);
            EndEpisode();
        }

        // if (distanceToWallE < 1.0f)
        // {
        //     SetReward(-0.1f);
        // }
        // else if (distanceToWallN < 0.1f)
        // {
        //     SetReward(-0.1f);
        // }
        // else if (distanceToWallS < 0.1f)
        // {
        //     SetReward(-0.1f);
        // }
        // else if (distanceToWallW < 0.1f)
        // {
        //     SetReward(-0.1f);
        // }

        // if (Time.time - episodeStartTime >= episodeTimeoutSeconds)
        // {
        //     EndEpisode();
        // }
        
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // continuous action
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
    }

}
