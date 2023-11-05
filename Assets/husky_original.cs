using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine;
using Unity.Robotics.UrdfImporter.Control;

public class husky_original : Agent
{
    // Vector3 linearVel;
    // Vector3 angularVel;
    // public float rotSpeed = 3;
    // public float linSpeed = 5;
    public ArticulationBody rBody;
    public GameObject wheelFL; // Front left wheel
    public GameObject wheelFR; // Front right wheel
    public GameObject wheelRL; // Rear left wheel
    public GameObject wheelRR; // Rear right wheel
    private ArticulationBody wA1;
    private ArticulationBody wA2;
    private ArticulationBody wA3;
    private ArticulationBody wA4;
    public Transform Target;
    public float maxLinearSpeed = 5; // m/s
    public float maxRotationSpeed = 50000; //
    private float wheelRadius = 0.033f; // meters
    private float trackWidth = 0.288f; // meters Distance between tyres
    private float forceLimit = 40;
    private float damping = 1000;
    private RotationDirection direction;
    public float episodeTimeoutSeconds = 60.0f; // 에피소드의 최대 시간 (예: 60초)

    private float episodeStartTime; // 에피소드 시작 시간
    private void ResetArticulationBody()
    {


        // 重置ArticulationBody的位置和旋转到指定坐标
        rBody.TeleportRoot(new Vector3(8f, 0.2f, 8f), Quaternion.Euler(0f, 90f, 0f));
        // 也可以重置速度，使其不受上一轮的动力学影响
        rBody.velocity = Vector3.zero;
        rBody.angularVelocity = Vector3.zero;
    }
    // private Transform Tr;
    private void SetParameters(ArticulationBody joint)
    {
        ArticulationDrive drive = joint.xDrive;
        drive.forceLimit = forceLimit;
        drive.damping = damping;
        joint.xDrive = drive;
    }
    
    private void SetSpeed(ArticulationBody joint, float wheelSpeed = float.NaN)
    {
        ArticulationDrive drive = joint.xDrive;
        if (float.IsNaN(wheelSpeed))
        {
            drive.targetVelocity = ((2 * maxLinearSpeed) / wheelRadius) * Mathf.Rad2Deg * (int)direction;
        }
        else
        {
            drive.targetVelocity = wheelSpeed;
        }
        joint.xDrive = drive;
    }
    
    public override void Initialize()
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
    public override void OnEpisodeBegin()
    {
       // If the Agent fell, zero its momentum
        episodeStartTime = Time.time;
        ResetArticulationBody();
        
        // Move the target to a new spot
        Target.localPosition = new Vector3(Random.value * 10 - 3,
                                           0.5f,
                                           Random.value * 10 - 3);
        

    }

    public override void CollectObservations(VectorSensor sensor)
    {
    // Target and Agent positions
    sensor.AddObservation(Target.localPosition);
    sensor.AddObservation(rBody.transform.localPosition);
    // sensor.AddObservation(rBody.transform.localRotation);

    // Agent velocity
    sensor.AddObservation(rBody.velocity.x);
    sensor.AddObservation(rBody.velocity.z);
    }



    private void RobotInput(float speed_, float rotSpeed_)
    {
        float speed = -speed_ * maxLinearSpeed;
        float rotSpeed = -rotSpeed_ * maxRotationSpeed;
    
        float wheelRotation = (speed / wheelRadius) * Mathf.Rad2Deg;
    
        // Calculate the differential speed for left and right wheels
        float diffSpeed = rotSpeed * trackWidth;
    
        // Calculate the target velocity for each wheel
        float leftWheelSpeed = wheelRotation - (diffSpeed / 2);
        float rightWheelSpeed = wheelRotation + (diffSpeed / 2);
    
        SetSpeed(wA1, leftWheelSpeed);
        SetSpeed(wA2, rightWheelSpeed);
        SetSpeed(wA3, leftWheelSpeed);
        SetSpeed(wA4, rightWheelSpeed);
    }
    
    public override void OnActionReceived(ActionBuffers actionBuffers) 
    {
    
        base.OnActionReceived(actionBuffers);
        var actions = actionBuffers.ContinuousActions;

        float linearVel = Mathf.Clamp(actions[0], -1, 1f);

        float angularVel = Mathf.Clamp(actions[1], -1, 1f);
        
        RobotInput(linearVel, angularVel);

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
        if (Time.time - episodeStartTime >= episodeTimeoutSeconds)
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