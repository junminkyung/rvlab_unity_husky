using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Geometry;
using Unity.Robotics.UrdfImporter.Control;

namespace RosSharp.Control
{
    public enum ControlMode { Keyboard, ROS };

    public class WorldOrder : MonoBehaviour
    {
	public GameObject wheelFL; // Front left wheel
	public GameObject wheelFR; // Front right wheel
	public GameObject wheelRL; // Rear left wheel
	public GameObject wheelRR; // Rear right wheel

        public ControlMode mode = ControlMode.ROS;

        private ArticulationBody wA1;
        private ArticulationBody wA2;
        private ArticulationBody wA3;
        private ArticulationBody wA4;

        public float maxLinearSpeed = 2; // m/s
        public float maxRotationalSpeed = 1; //
        public float wheelRadius = 0.033f; // meters
        public float trackWidth = 0.288f; // meters Distance between tyres
        public float forceLimit = 10;
        public float damping = 10;

        public float ROSTimeout = 0.5f;
        private float lastCmdReceived = 0f;

        ROSConnection ros;
        private RotationDirection direction;
        private float rosLinear = 0f;
        private float rosAngular = 0f;

        void Start()
        {
	wA1 = wheelFL.GetComponent<ArticulationBody>();
	wA2 = wheelFR.GetComponent<ArticulationBody>();
	wA3 = wheelRL.GetComponent<ArticulationBody>();
	wA4 = wheelRR.GetComponent<ArticulationBody>();
	SetParameters(wA1);
	SetParameters(wA2);
	SetParameters(wA3);
	SetParameters(wA4);

            ros = ROSConnection.GetOrCreateInstance();
            ros.Subscribe<TwistMsg>("/joy_teleop/cmd_vel", ReceiveROSCmd);
        }

        void ReceiveROSCmd(TwistMsg cmdVel)
        {
            rosLinear = (float)cmdVel.linear.x;
            rosAngular = (float)cmdVel.angular.z * -5000.0f;
            lastCmdReceived = Time.time;
            Debug.Log($"Rotation Speed {rosAngular}");
        }

        void FixedUpdate()
        {
            if (mode == ControlMode.Keyboard)
            {
                KeyBoardUpdate();
            }
            else if (mode == ControlMode.ROS)
            {
                ROSUpdate();
            }
        }

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

	private void KeyBoardUpdate()
	{
	    float moveDirection = Input.GetAxis("Vertical");
	    float inputSpeed;
	    float inputRotationSpeed;

	    if (moveDirection > 0)
	    {
		inputSpeed = maxLinearSpeed;
	    }
	    else if (moveDirection < 0)
	    {
		inputSpeed = -maxLinearSpeed;
	    }
	    else
	    {
		inputSpeed = 0;
	    }

	    float turnDirection = Input.GetAxis("Horizontal");
	    if (turnDirection > 0)
	    {
		inputRotationSpeed = -maxRotationalSpeed;
	    }
	    else if (turnDirection < 0)
	    {
		inputRotationSpeed = maxRotationalSpeed;
	    }
	    else
	    {
		inputRotationSpeed = 0;
	    }

	    RobotInput(inputSpeed, inputRotationSpeed);
	}

        private void ROSUpdate()
        {
            if (Time.time - lastCmdReceived > ROSTimeout)
            {
                rosLinear = 0f;
                rosAngular = 0f;
            }
            RobotInput(rosLinear, -rosAngular);
        }

	private void RobotInput(float speed, float rotSpeed)
	{
	    if (speed > maxLinearSpeed)
	    {
		speed = maxLinearSpeed;
	    }
	    if (rotSpeed > maxRotationalSpeed)
	    {
		rotSpeed = maxRotationalSpeed;
	    }

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

    }
}