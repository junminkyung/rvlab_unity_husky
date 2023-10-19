using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class TestAgent : Agent
{   
    ArticulationBody aBody;

    public List<ArticulationBody> Wheels;
    float maxLinearSpeed = 2.0f; // m/s
    float _wheelRadius = 0.330f; // m
    float _maxrotspeed; // m/s
    float _damping = 400; // N*s/m
    float _SpeedSensitivity = 1;
    float _DampingSensitivity = 1;
    float _Sensitivity = 0.2f;

    private void Start()
    {
        _maxrotspeed = Mathf.Rad2Deg * maxLinearSpeed / _wheelRadius;
        aBody = GetComponent<ArticulationBody>(); 
    }

    void Drive(float L_Speed, float R_Speed)
    {
        for (int i = 0; i < Wheels.Count; i++)
        {
            ArticulationDrive temp = Wheels[i].xDrive;
            if (i % 2 == 0)
            {
                temp.targetVelocity = L_Speed;
            }
            else
            {
                temp.targetVelocity = R_Speed;
            }
            Wheels[i].xDrive = temp;
        }
    }

    void RL(float _Forward, float _Rot)
    {
        float max_1 = Mathf.Max(Mathf.Abs(_Forward) + Mathf.Abs(_Rot), 1.0f);
        float L_Rot = (_Forward + _Rot) * _maxrotspeed / max_1 * _SpeedSensitivity;
        float R_Rot = (_Forward - _Rot) * _maxrotspeed / max_1 * _SpeedSensitivity;
        Drive(L_Rot, R_Rot);
    }

    private void Update()
    {
        float Horizontal = Input.GetAxis("Horizontal");
        float Vertical = Input.GetAxis("Vertical");

        RL(Vertical, Horizontal);

        // if (Input.GetKeyDown(KeyCode.Space))
        // {
        //     Speed_Up();
        // }

        // if (Input.GetKeyDown(KeyCode.LeftControl))
        // {
        //     Speed_Down();
        // }
    }

    private void Set_Properties(ArticulationBody joint)
    {
        ArticulationDrive drive = joint.xDrive;
        drive.damping = _damping * _DampingSensitivity;
        joint.xDrive = drive;
    }

    public Transform Target;
    public override void OnEpisodeBegin()
    {   
        this.transform.localPosition = new Vector3 (-6.7f, 1.5f, 6.7f);
        Target.localPosition = new Vector3 (6.5f, 2.0f, -6.5f);

        aBody.angularVelocity = Vector3.zero;
        aBody.velocity = Vector3.zero;

        // 로봇의 속도 초기화
        foreach (var wheel in Wheels)
        {
            ArticulationDrive drive = wheel.xDrive;
            drive.targetVelocity = 0f;
            wheel.xDrive = drive;
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {   
        // 목적지(Target)와 로봇(Agent)의 위치 좌표
        sensor.AddObservation(Target.localPosition);
        sensor.AddObservation(this.transform.localPosition);
        // sensor.AddObservation(this.transform.rotation.x);
        // sensor.AddObservation(this.transform.rotation.y);
        // sensor.AddObservation(this.transform.rotation.z);

        // 로봇(Agent)의 속도 정보 수집
        sensor.AddObservation(aBody.velocity);
        sensor.AddObservation(aBody.angularVelocity);
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

    // public float forceMultiplier = 1;
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {   
        // 에이전트가 취한 연속적인 (continuous) 행동을 얻습니다.
        // float moveX = actionBuffers.ContinuousActions[0];
        // float moveZ = actionBuffers.ContinuousActions[1];
        float moveX = Mathf.Clamp(actionBuffers.ContinuousActions[0], -1f, 1f);
        float moveZ = Mathf.Clamp(actionBuffers.ContinuousActions[1], -1f, 1f);

        // Vector3 movement = new Vector3(moveX, 0, moveZ);
        // aBody.velocity = movement * forceMultiplier;
        
        // Drive 메서드에 연속적인 행동 값 전달
        RL(moveZ, moveX);

        // Agent와 Target사이의 거리를 측정
        float distanceToTarget = Vector3.Distance(this.transform.localPosition, Target.localPosition);

        // Target에 도달하는 경우 (거리가 1.42보다 작은 경우) Episode 종료
        if (distanceToTarget < 1.2)
        {
            SetReward(1.0f);
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // continuous action
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
    }

}
