namespace PironGames.MLLib.Samples.SelfDrivingCars
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using PironGames.MLLib.NN;

    /// <summary>
    /// Logic for car that is able to drive itself around a track
    /// and avoid other cars
    /// </summary>
    public class AICarBehaviour : MonoBehaviour, IComparable
    {
        private const int NN_HIDDEN_LAYER_NEURON_COUNT = 50;
        private const float WAYPOINT_SWITCH_DISTANCE = 0.25f;

        /// <summary>
        /// 4 outputs - forward, brake, left, right
        /// </summary>
        private const int NN_OUTPUT_LAYER_NEURON_COUNT = 4;

        /// <summary>
        /// Max speed of the vehicle, in km/h
        /// </summary>
        public float MaxSpeed = 30;

        public float MaxReverseSpeed = 10;

        /// <summary>
        /// Meters/second
        /// </summary>
        public float Acceleration = 5;

        public float Steer = 20;

        public float ForwardAcceleration = 2f;

        public float BrakeAcceleration = -2f;

        public Sensor Sensor;

        private Vector3 m_VelocityVector = Vector3.forward;

        private float m_Velocity = 0;

        private bool m_Steering = false;

        private float m_SteerValue = 0;

        private float m_CurrentAcceleration = 0;

        private bool m_isAccelerating = false;

        private float m_appliedAcceleration = 0;

        private NeuralNetwork m_brain;

        private bool m_dead = false;

        private Dictionary<int, GameObject> m_waypoints = new();

        private SimpleWaypoint m_spawnWaypoint;

        private SimpleWaypoint m_sourceWaypoint;

        private float m_lifeTimestamp;

        private int waypointCounter = 0;
        private int lap = 0;

        public bool IsDead()
        {
            return m_dead;
        }

        public NeuralNetwork GetBrain()
        {
            return m_brain;
        }

        public void SetupBrain(SimpleWaypoint spawn, string json, bool mutate)
        {
            if (m_brain is null)
            {
                m_brain = CreateDefaultNN();
            }

            if (!string.IsNullOrEmpty(json))
            {
                JsonUtility.FromJsonOverwrite(json, m_brain);
            }

            //m_brain.Reset();

            if (mutate)
            {
                m_brain.Mutate(UnityEngine.Random.Range(0, 0.25f), UnityEngine.Random.Range(0, 0.25f));
            }

            m_spawnWaypoint = spawn;
            m_sourceWaypoint = spawn;

            transform.position = m_sourceWaypoint.transform.position;
            transform.LookAt(m_sourceWaypoint.transform);

            waypointCounter = 0;
            lap = 0;

            ResetLifeTime();
        }

        public string GetSerializedBrain()
        {
            return JsonUtility.ToJson(m_brain);
        }

        public bool IsOld()
        {
            return Time.time - m_lifeTimestamp >= m_sourceWaypoint.MinTime * 2;
        }

        public int CompareTo(object otherObj)
        {
            if (otherObj == null)
            {
                return 1;
            }

            AICarBehaviour other = otherObj as AICarBehaviour;

            //if (other == this)
            //{
            //    return 0;
            //}    

            if (lap > other.lap)
            {
                return 1;
            }

            if (lap < other.lap)
            {
                return -1;
            }

            if (waypointCounter > other.waypointCounter)
            {
                return 1;
            }

            if (waypointCounter < other.waypointCounter)
            {
                return -1;
            }

            float distance = Vector3.Distance(transform.position, m_sourceWaypoint.NextWaypoint.transform.position);
            float otherDistance = Vector3.Distance(other.transform.position, other.m_sourceWaypoint.NextWaypoint.transform.position);

            return distance < otherDistance ? 1 : -1;
        }

        // Start is called before the first frame update
        void Start()
        {
            if (m_brain is null)
            {
                m_brain = CreateDefaultNN();
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (m_dead)
            {
                return;
            }

            HandleWaypoints();

            HandleAIControls();

            if (!m_isAccelerating)
            {
                m_CurrentAcceleration = Mathf.Lerp(m_CurrentAcceleration, 0, 0.1f);
            }
            else
            {
                m_CurrentAcceleration = m_appliedAcceleration;
                LimitAcceleration();
            }

            Vector3 direction = transform.forward;

            m_Velocity += Time.deltaTime * m_CurrentAcceleration / 1000f;
            m_Velocity = Mathf.Clamp(m_Velocity, -MaxReverseSpeed, MaxSpeed);

            transform.position += m_Velocity * Time.deltaTime * direction;

            // avoid steering with no velocity
            if (m_Steering && Mathf.Abs(m_Velocity) > 0)
            {
                transform.Rotate(Vector3.up, m_SteerValue * Time.deltaTime);
            }
        }

        void OnCollisionEnter(Collision collision)
        {
            m_dead = true;
            Destroy(GetComponent<Rigidbody>());
            Destroy(GetComponent<Sensor>());
        }

        private void OnTriggerEnter(Collider other)
        {
            m_dead = true;
        }

        private void HandlePlayerControls()
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                m_appliedAcceleration = ForwardAcceleration;

                m_isAccelerating = true;
            }

            if (Input.GetKeyUp(KeyCode.UpArrow))
            {
                m_isAccelerating = false;
            }

            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                m_appliedAcceleration = BrakeAcceleration;

                m_isAccelerating = true;
            }

            if (Input.GetKeyUp(KeyCode.DownArrow))
            {
                m_isAccelerating = false;
            }

            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                m_Steering = true;
                m_SteerValue = -Steer;
            }

            if (Input.GetKeyUp(KeyCode.LeftArrow))
            {
                m_Steering = false;
                m_SteerValue = 0;
            }

            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                m_Steering = true;
                m_SteerValue = Steer;
            }

            if (Input.GetKeyUp(KeyCode.RightArrow))
            {
                m_Steering = false;
                m_SteerValue = 0;
            }
        }

        private GameObject GetClosestWaypoint()
        {
            var waypoints = GameObject.FindGameObjectsWithTag("Waypoint");

            var minDistance = float.MaxValue;
            GameObject result = null;

            foreach (var wp in waypoints)
            {
                if (m_waypoints.ContainsKey(wp.GetInstanceID()))
                {
                    continue;
                }

                var distance = Vector3.Distance(transform.position, wp.transform.position);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    result = wp;
                }
            }

            return result;
        }

        private void HandleWaypoints()
        {
            if (m_sourceWaypoint is null)
            {
                return;
            }

            float distance = Vector3.Distance(transform.position, m_sourceWaypoint.NextWaypoint.transform.position);

            if (distance <= WAYPOINT_SWITCH_DISTANCE)
            {
                m_sourceWaypoint = m_sourceWaypoint.NextWaypoint;

                waypointCounter++;

                if (m_sourceWaypoint == m_spawnWaypoint)
                {
                    waypointCounter = 0;
                    lap++;
                }

                ResetLifeTime();
            }
        }

        private void ResetLifeTime()
        {
            m_lifeTimestamp = Time.time;
        }

        private void HandleAIControls()
        {
            List<float> inputs = new();

            // setup the sensor inputs
            inputs.AddRange(Sensor.Readings());

            // setup the distance input
            var d1 = Vector3.Distance(m_sourceWaypoint.NextWaypoint.transform.position, transform.position);
            var d2 = Vector3.Distance(m_sourceWaypoint.transform.position, m_sourceWaypoint.NextWaypoint.transform.position);
            var d = d1 / d2;

            inputs.Add(Mathf.Min(1, d));

            // process the brain
            m_brain.FeedForward(inputs.ToArray());

            var outputs = m_brain.FinalOutputs();

            var forward = outputs[0] == 1;
            var reverse = outputs[1] == 1;

            m_isAccelerating = false;
            m_appliedAcceleration = 0;

            if (forward && !reverse)
            {
                m_appliedAcceleration = ForwardAcceleration;
                m_isAccelerating = true;
            }
            else if (reverse && !forward)
            {
                m_appliedAcceleration = BrakeAcceleration;
                m_isAccelerating = true;
            }

            var left = outputs[2] == 1;
            var right = outputs[3] == 1;

            m_SteerValue = 0;
            m_Steering = false;

            if (left && !right)
            {
                m_SteerValue = -Steer;
                m_Steering = true;
            }
            else if (!left && right)
            {
                m_SteerValue = Steer;
                m_Steering = true;
            }
        }

        private void LimitAcceleration()
        {
            if (m_CurrentAcceleration > 0)
            {
                m_CurrentAcceleration = Mathf.Min(m_CurrentAcceleration, Acceleration);
            }
            else
            {
                m_CurrentAcceleration = Mathf.Max(m_CurrentAcceleration, -Acceleration);
            }
        }

        private NeuralNetwork CreateDefaultNN()
        {
            ActivationFunction[] activationFunctions = { ActivationFunction.Sigmoid, ActivationFunction.BinaryStep };

            return new NeuralNetwork(new int[] { Sensor.RayCount + 1, NN_HIDDEN_LAYER_NEURON_COUNT, NN_OUTPUT_LAYER_NEURON_COUNT }, activationFunctions);
        }
    }
}