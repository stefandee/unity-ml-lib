namespace PironGames.MLLib.Samples.SelfDrivingCars
{
    using System.Collections.Generic;
    using UnityEngine;

    public class BasePhysicsCarBehaviour : MonoBehaviour
    {
        public List<AxleInfo> AxleInfos; // the information about each individual axle
        public float MaxMotorTorque; // maximum torque the motor can apply to wheel
        public float MaxBrakeTorque; // maximum torque the motor can apply to wheel
        public float MaxSteeringAngle; // maximum steer angle the wheel can have

        [Tooltip("The vehicle's speed when the physics engine can use different amount of sub-steps (in m/s).")]
        public float CriticalSpeed = 5f;
        [Tooltip("Simulation sub-steps when the speed is above critical.")]
        public int StepsBelow = 5;
        [Tooltip("Simulation sub-steps when the speed is below critical.")]
        public int StepsAbove = 1;

        private float m_steer = 0;
        private float m_motor = 0;
        private float m_brake = 0;

        void Start()
        {
            AxleInfos[0].rightWheel.ConfigureVehicleSubsteps(CriticalSpeed, StepsBelow, StepsAbove);
        }

        // finds the corresponding visual wheel
        // correctly applies the transform
        public void ApplyLocalPositionToVisuals(WheelCollider collider, GameObject visual, bool left = false)
        {
            Transform visualWheel = visual.transform;

            Vector3 position;
            Quaternion rotation;
            collider.GetWorldPose(out position, out rotation);

            visualWheel.transform.position = position;
            visualWheel.transform.rotation = rotation; // * Quaternion.Euler(0, 180, 0);

            if (left)
            {
                visualWheel.transform.localRotation *= Quaternion.AngleAxis(180f, Vector3.up);
            }
        }

        public void SetInputs(float steer, float motor, float brake)
        {
            m_steer = Mathf.Clamp(steer, -1, 1);
            m_motor = Mathf.Clamp(motor, -1, 1);
            m_brake = Mathf.Clamp01(brake);
        }

        public void Update()
        {
            float motor = MaxMotorTorque * m_motor;
            float steering = MaxSteeringAngle * m_steer;

            float brake = MaxBrakeTorque * m_brake;
            bool braking = m_brake > 0;

            foreach (AxleInfo axleInfo in AxleInfos)
            {
                if (axleInfo.steering)
                {
                    axleInfo.leftWheel.steerAngle = steering;
                    axleInfo.rightWheel.steerAngle = steering;
                }

                if (braking)
                {
                    axleInfo.leftWheel.brakeTorque = brake;
                    axleInfo.rightWheel.brakeTorque = brake;
                }
                else
                {
                    axleInfo.leftWheel.brakeTorque = 0;
                    axleInfo.rightWheel.brakeTorque = 0;
                }

                if (axleInfo.motor && !braking)
                {
                    axleInfo.leftWheel.motorTorque = motor;
                    axleInfo.rightWheel.motorTorque = motor;
                }

                ApplyLocalPositionToVisuals(axleInfo.leftWheel, axleInfo.leftWheelVisual, true);
                ApplyLocalPositionToVisuals(axleInfo.rightWheel, axleInfo.rightWheelVisual);
            }
        }
    }
}