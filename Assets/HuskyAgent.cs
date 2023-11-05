using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine;
using Unity.Robotics.UrdfImporter.Control;

public class HuskyAgent : Agent
{   
    public ArticulationBody aBody;
    public float maxLinearSpeed = 5.0f; // m/s
    private float _wheelRadius = 0.330f; // m
    private float trackWidth = 0.288f; // m
    public float _maxrotspeed = 100; // m/s
    private float _damping = 100; // N*s/m
    // float _SpeedSensitivity = 1;
    private float _DampingSensitivity = 1;
    // float _Sensitivity = 0.2f;
    private float forceLimit = 50;
    
    public Transform Target;

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

    private void Drive(ArticulationBody joint, float wheelSpeed = float.NaN)
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

    private void RL(float _Forward, float _Rot)
    {
        float Speed = -_Forward * maxLinearSpeed;
        float Rot_Speed = -_Rot * _maxrotspeed;

        float wheelRotation = (Speed / _wheelRadius) * Mathf.Rad2Deg;

        float Diff_Speed = Rot_Speed * trackWidth;

        float L_Speed = wheelRotation - (Diff_Speed / 2);
        float R_Speed = wheelRotation + (Diff_Speed / 2);

        Drive(wA1, L_Speed);
        Drive(wA2, R_Speed);
        Drive(wA3, L_Speed);
        Drive(wA4, R_Speed);
    }

   private void Set_Properties(ArticulationBody joint)
    {
        ArticulationDrive drive = joint.xDrive;
        drive.damping = _damping * _DampingSensitivity;
        joint.xDrive = drive;
        drive.forceLimit = forceLimit;
    }
    
    public override void OnEpisodeBegin()
    {   
        episodeStartTime = Time.time;
        aBody.transform.localPosition = new Vector3 (-6.7f, 1.5f, 6.7f);
        Target.localPosition = new Vector3(Random.value * 10 - 5, 
                                            2.0f, 
                                            Random.value * 10 - 5);

        aBody.TeleportRoot(aBody.transform.localPosition, Quaternion.Euler(0f, 180f, 0f));
        aBody.angularVelocity = Vector3.zero;
        aBody.velocity = Vector3.zero;
    }

    public override void CollectObservations(VectorSensor sensor)
    {   
        // 목적지(Target)와 로봇(Agent)의 위치 좌표
        sensor.AddObservation(Target.localPosition);
        sensor.AddObservation(this.transform.localPosition);

        // 로봇(Agent)의 속도 정보 수집
        sensor.AddObservation(aBody.velocity.x);
        sensor.AddObservation(aBody.velocity.z);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {   
        base.OnActionReceived(actionBuffers);

        float moveX = Mathf.Clamp(actionBuffers.ContinuousActions[0], -1f, 1f);
        float moveZ = Mathf.Clamp(actionBuffers.ContinuousActions[1], -1f, 1f);

        // Drive 메서드에 연속적인 행동 값 전달
        RL(moveZ, moveX);

        // Agent와 Target사이의 거리를 측정
        float distanceToTarget = Vector3.Distance(this.transform.localPosition, Target.localPosition);

        // Target에 도달하는 경우 (거리가 1.4보다 작은 경우) Episode 종료
        if (distanceToTarget < 1.2)
        {
            SetReward(1.0f);
            EndEpisode();
        }

        if (aBody.transform.localPosition.y < 0.1f)
        {
            EndEpisode();
            SetReward(-1.0f);
        }

        if (Time.time - episodeStartTime >= episodeTimeoutSeconds)
        {
            EndEpisode();
        }

        AddReward(-0.01f);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // continuous action
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
    }

}
