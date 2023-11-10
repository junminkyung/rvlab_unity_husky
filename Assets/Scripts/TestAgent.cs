using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnitySensors;

public class TestAgent : Agent
{
    public ArticulationBody aBody;
    public float RotSpeed = 3;
    public float LinearSpeed = 5;
    public Transform Target;
    // public VelodyneSensor LiDAR;

    // public GameObject temp;
    // [SerializeField] private Collider front_bumper;
    
    void Start()
    {
    	aBody = GetComponent<ArticulationBody>(); 
        // LiDAR = GetComponent<VelodyneSensor>();
        // front_bumper = temp.GetComponent<Collider>();
    }

    public override void OnEpisodeBegin()
    {
        aBody.angularVelocity = Vector3.zero;
        aBody.velocity = Vector3.zero;
        // aBody.TeleportRoot(new Vector3(-3.3f, 0.5f, 3.3f), Quaternion.Euler(0f, 180f, 0f));

        // Agent를 노이즈를 활용해서 (-3.3, 0.5, 3.3)주위의 새로운 무작위 위치에 이동
        float noiseMagnitude = 1.0f; // 노이즈 크기 조절
        Vector3 randomNoise = new Vector3(Random.Range(-noiseMagnitude, noiseMagnitude), 0, Random.Range(-noiseMagnitude, noiseMagnitude));
        Vector3 randomPosition = new Vector3(-6.0f, 0.2f, 6.0f) + randomNoise;
        Quaternion randomRotation = Quaternion.Euler(0f, Random.Range(0, 360), 0f);
        aBody.TeleportRoot(randomPosition, randomRotation);


        // Target을 Random함수를 활용해서 새로운 무작위 위치에 이동
        Target.localPosition = new Vector3(Random.Range(-6.0f, 6.0f), 0.5f, Random.Range(-6.0f, 6.0f));
    }

    public override void CollectObservations(VectorSensor sensor)
    {   
        // // Velodyne LiDAR 센서에서 수집한 관측 정보를 추가
        // LiDAR.CompleteJob(); // 센서 작업이 완료될 때까지 기다립니다.
        // for (int i = 0; i < LiDAR.pointsNum; i++)
        // {
        //     float distance = LiDAR.distances[i];
        //     Vector3 point = LiDAR.points[i];
        //     float intensity = LiDAR.intensities[i];

        //     // 디버그 메시지로 관측 정보 출력
        //     Debug.Log($"LiDAR Observation {i}: Distance = {distance}, Point = {point}, Intensity = {intensity}");

        //     // 관측 정보를 ML-Agents 관측 정보로 추가
        //     sensor.AddObservation(distance);
        //     sensor.AddObservation(point);
        //     sensor.AddObservation(intensity);
        // }

        // Target/Agent의 위치 정보 수집
        sensor.AddObservation(Target.localPosition);
        sensor.AddObservation(aBody.transform.localPosition);

        // Agent의 velocity 정보 수집
        sensor.AddObservation(aBody.velocity.x);
        sensor.AddObservation(aBody.velocity.z);
    }

    private void Drive(float Input_Linear_Vel, float Input_Angular_Vel)
    {
        aBody.velocity = aBody.transform.forward * Input_Linear_Vel * LinearSpeed;
        aBody.angularVelocity = aBody.transform.up * Input_Angular_Vel * RotSpeed;
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        base.OnActionReceived(actionBuffers);
        var actions = actionBuffers.ContinuousActions;
        float LinearVel = Mathf.Clamp(actions[0], -1.0f, 1.0f);
        float AngularVel = Mathf.Clamp(actions[1], -1.0f, 1.0f);

        Drive(LinearVel, AngularVel);

        // Agent와 Target사이의 거리를 측정
        float distanceToTarget = Vector3.Distance(aBody.transform.localPosition, Target.localPosition);

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

    void OnCollisionEnter(Collision coll)
    {
        if (coll.gameObject.CompareTag("Wall"))
        {   
            Debug.Log("Hit Wall");
            // AddReward(-0.1f);
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