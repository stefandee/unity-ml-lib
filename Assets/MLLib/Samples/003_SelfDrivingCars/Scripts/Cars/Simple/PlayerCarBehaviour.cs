namespace PironGames.MLLib.Samples.SelfDrivingCars
{
    using PironGames.MLLib.NN;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// https://www.youtube.com/watch?v=Rs_rAxEsAvI
    /// </summary>
    public class PlayerCarBehaviour : MonoBehaviour
    {
        private const int NN_HIDDEN_LAYER_NEURON_COUNT = 26;

        /// <summary>
        /// 4 outputs - forward, brake, left, right
        /// </summary>
        private const int NN_OUTPUT_LAYER_NEURON_COUNT = 4;

        /// <summary>
        /// Can this car be controlled by the player?
        /// </summary>
        public bool PlayerControlled = true;

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

        private Vector3 m_prevWp;

        private GameObject m_targetWp;

        public bool IsDead()
        {
            return m_dead;
        }

        public void SetupBrain(string json, bool mutate)
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
                m_brain.Mutate(Random.Range(0.05f, 0.25f));
            }
        }

        public string GetSerializedBrain()
        {
            return JsonUtility.ToJson(m_brain);
        }

        // Start is called before the first frame update
        void Start()
        {
            if (m_brain is null)
            {
                m_brain = CreateDefaultNN();
            }

            m_prevWp = transform.position;
            m_targetWp = GetClosestWaypoint();
        }

        // Update is called once per frame
        void Update()
        {
            if (m_dead)
            {
                return;
            }

            HandleWaypoints();

            if (PlayerControlled)
            {
                HandlePlayerControls();
            }
            else
            {
                HandleAIControls();
            }

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
            if (m_targetWp != null && Vector3.Distance(transform.position, m_targetWp.transform.position) <= 5)
            {
                m_waypoints.Add(m_targetWp.GetInstanceID(), m_targetWp);
                m_prevWp = m_targetWp.transform.position;

                m_targetWp = GetClosestWaypoint();
            }
        }

        private void HandleAIControls()
        {
            List<float> inputs = new();
            inputs.AddRange(Sensor.Readings());

            if (m_targetWp == null)
            {
                inputs.Add(1f);
            }
            else
            {
                var d1 = Vector3.Distance(m_targetWp.transform.position, transform.position);
                var d2 = Vector3.Distance(m_prevWp, m_targetWp.transform.position);
                var d = d1 / d2;

                if (d != 1)
                {
                    //Debug.Log("fadfa");
                }

                //inputs.Add(Mathf.Max(0, 1f - d));
                inputs.Add(Mathf.Min(1, d));
            }

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
            ActivationFunction[] activationFunctions = { ActivationFunction.Sigmoid, ActivationFunction.Sigmoid, ActivationFunction.Sigmoid };

            return new NeuralNetwork(new int[] { Sensor.RayCount + 1, NN_HIDDEN_LAYER_NEURON_COUNT, NN_OUTPUT_LAYER_NEURON_COUNT }, activationFunctions);
        }
    }
}