namespace PironGames.MLLib.Samples.SelfDrivingCars
{
    using PironGames.MLLib.NN;
    using System.Collections.Generic;
    using UnityEngine;

    public class PhysicsAICarRelUBehaviour : MonoBehaviour, IAICar, IPositionRecorderEntity, IRemovable, IEmotable, IRaceRecorderEntity, IWaypointFollower
    {
        public BasePhysicsCarBehaviour Controller;

        public Sensor[] Sensors;

        public GameObject Emote;

        public GameObject EmoteBestAverage;

        public bool CompactNNOutputs = true;

        private const float WAYPOINT_SWITCH_DISTANCE = 4f;

        private const int NN_HIDDEN_LAYER_NEURON_COUNT = 30;

        /// <summary>
        /// 5 outputs: steer left, steer right (-1..1), throttle foward, throttle back ward(-1..1), brake (0..1)
        /// </summary>
        private const int NN_EXTENDED_OUTPUT_LAYER_NEURON_COUNT = 5;

        /// <summary>
        /// 3 outputs: steer (-1..1), throttle(-1..1), brake (0..1)
        /// </summary>
        private const int NN_COMPACT_OUTPUT_LAYER_NEURON_COUNT = 3;

        private NeuralNetwork m_brain;

        private List<float> m_nnInputs = new();

        private GateWaypoint m_spawnWaypoint;

        private GateWaypoint m_sourceWaypoint;

        private int waypointCounter = 0;

        private int lap = 0;

        private float lapTimestamp = -1;

        private float lapTime = float.NaN;

        private float waypointTimestamp = -1;

        private int m_collisionPerLap = 0;

        private bool m_scheduledRemove = false;

        private PositionRecorder m_positionRecorder = new();

        private RaceRecorder m_raceRecorder = new();

        private Rigidbody m_rb;

        public PositionRecorder PositionRecorder
        {
            get => m_positionRecorder;
        }

        public RaceRecorder RaceRecorder
        {
            get => m_raceRecorder;
        }

        public NeuralNetwork Brain
        {
            get => m_brain;
        }

        public GateWaypoint Waypoint
        {
            get => m_sourceWaypoint;
        }

        public void SetupBrain(GateWaypoint spawn, string json, float mutate, bool startRace)
        {
            if (m_brain is null)
            {
                m_brain = CreateDefaultNN();
            }

            if (!string.IsNullOrEmpty(json))
            {
                JsonUtility.FromJsonOverwrite(json, m_brain);
            }

            if (mutate > 0)
            {
                m_brain.Mutate(UnityEngine.Random.Range(0, mutate), UnityEngine.Random.Range(0, mutate));
            }

            m_sourceWaypoint = spawn;
            waypointTimestamp = Time.time;

            transform.position = m_sourceWaypoint.transform.position;
            transform.LookAt(m_sourceWaypoint.Next.transform);

            if (startRace)
            {
                m_spawnWaypoint = spawn;
                waypointCounter = 0;
                lap = 0;
                lapTimestamp = Time.time;
            }
        }

        public string SerializedBrain
        {
            get => JsonUtility.ToJson(m_brain);
        }

        public int CompareTo(object otherObj)
        {
            if (otherObj == null)
            {
                return 1;
            }

            PhysicsAICarRelUBehaviour other = otherObj as PhysicsAICarRelUBehaviour;

            if (!float.IsNaN(m_raceRecorder.AverageLapTime) && !float.IsNaN(other.m_raceRecorder.AverageLapTime))
            {
                return m_raceRecorder.AverageLapTime < other.m_raceRecorder.AverageLapTime ? 1 : -1;
            }

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

            float distance = Vector3.Distance(transform.position, m_sourceWaypoint.Next.transform.position);
            float otherDistance = Vector3.Distance(other.transform.position, other.m_sourceWaypoint.Next.transform.position);

            return distance < otherDistance ? 1 : -1;
        }

        public void Remove(float delay)
        {
            if (!m_scheduledRemove)
            {
                m_scheduledRemove = true;

                if (delay > 0)
                {
                    Destroy(gameObject, delay);
                }
                else
                {
                    Destroy(gameObject);
                }
            }
        }

        public void React(ReactionEmote emote)
        {
            switch (emote)
            {
                case ReactionEmote.Fastest:
                    Emote.SetActive(true);
                    EmoteBestAverage.SetActive(false);
                    break;

                case ReactionEmote.BestAverage:
                    Emote.SetActive(false);
                    EmoteBestAverage.SetActive(true);
                    break;

                case ReactionEmote.None:
                    Emote.SetActive(false);
                    EmoteBestAverage.SetActive(false);
                    break;
            }
        }

        void Start()
        {
            Emote?.SetActive(false);
            EmoteBestAverage?.SetActive(false);
            m_rb = GetComponentInChildren<Rigidbody>();
        }

        void Update()
        {
            m_raceRecorder.CurrentLapTime = Time.time - lapTimestamp;
        }

        void OnCollisionEnter(Collision collision)
        {
            m_collisionPerLap++;
        }

        void OnTriggerEnter(Collider other)
        {
            var wpTrigger = other.gameObject.GetComponent<WaypointTrigger>();

            if (wpTrigger == null || m_sourceWaypoint == null || wpTrigger.Waypoint != m_sourceWaypoint.Next)
            {
                return;
            }

            var dot = Vector3.Dot(transform.forward, wpTrigger.transform.forward);

            if (dot > 0)
            {
                m_sourceWaypoint = m_sourceWaypoint.Next;

                waypointTimestamp = Time.time;

                waypointCounter++;

                if (m_sourceWaypoint == m_spawnWaypoint)
                {
                    lapTime = Time.time - lapTimestamp;
                    lapTimestamp = Time.time;

                    m_raceRecorder.AddLapStats(lap, lapTime, m_collisionPerLap, SerializedBrain);

                    Debug.Log($"{gameObject.name} lap time {lapTime} (lap {lap}), average: {m_raceRecorder.AverageLapTime}");

                    waypointCounter = 0;
                    m_collisionPerLap = 0;
                    lap++;

                    // TODO might be worth evaluating the cost/gradient and adjust?
                    m_brain.Mutate(UnityEngine.Random.Range(0, 0.005f), UnityEngine.Random.Range(0, 0.005f));
                }
            }
        }

        void FixedUpdate()
        {
            if (m_scheduledRemove)
            {
                return;
            }

            m_positionRecorder.Record(Time.time, transform.position);

            HandleWaypoints();

            ProcessNeuralNetwork();
        }

        private NeuralNetwork CreateDefaultNN()
        {
            ActivationFunction[] activationFunctions = { ActivationFunction.ReLU, ActivationFunction.ReLU };

            // input layer: sensors count + distance to next waypoint + dot product toward next waypoint + slope
            var result = new NeuralNetwork(new int[] { SensorsRayCount + 4, NN_HIDDEN_LAYER_NEURON_COUNT, CompactNNOutputs ? NN_COMPACT_OUTPUT_LAYER_NEURON_COUNT : NN_EXTENDED_OUTPUT_LAYER_NEURON_COUNT }, activationFunctions);

            return result;
        }

        private int SensorsRayCount
        {
            get
            {
                int result = 0;

                foreach (var sensor in Sensors)
                {
                    result += sensor.RayCount;
                }

                return result;
            }
        }

        private void ProcessNeuralNetwork()
        {
            m_nnInputs.Clear();

            // setup the sensor inputs
            foreach (var sensor in Sensors)
            {
                m_nnInputs.AddRange(sensor.Readings());
            }

            // setup the distance input
            var d1 = Vector3.Distance(m_sourceWaypoint.Next.transform.position, transform.position);
            var d2 = Vector3.Distance(m_sourceWaypoint.transform.position, m_sourceWaypoint.Next.transform.position);
            var d = d1 / d2;

            // m_nnInputs.Add(Mathf.Min(1, d));

            if (d1 > d2)
            {
                m_nnInputs.Add(Mathf.Max(-1, -d2 / d1));
            }
            else
            {
                m_nnInputs.Add(Mathf.Min(1, d));
            }

            // setup the direction toward the gate trigger
            // var dot = Vector3.Dot((m_sourceWaypoint.Next.transform.position - transform.position).normalized, (m_sourceWaypoint.Next.transform.position - m_sourceWaypoint.transform.position).normalized);
            var dotGateTrigger = Vector3.Dot(transform.forward, m_sourceWaypoint.Next.Trigger.transform.forward);

            m_nnInputs.Add(dotGateTrigger);

            // setup the slope
            var dotUp = Vector3.Dot(transform.up, Vector3.up);
            m_nnInputs.Add(dotUp);

            // setup the linear velocity percentage
            m_nnInputs.Add(Mathf.Max(1, m_rb.linearVelocity.magnitude / 30f));

            // process the brain
            m_brain.FeedForward(m_nnInputs.ToArray());

            var outputs = m_brain.FinalOutputs();

            if (CompactNNOutputs)
            {
                Controller.SetInputs(Mathf.Clamp(outputs[0], -1, 1), Mathf.Clamp(outputs[1], -1, 1), Mathf.Clamp(outputs[2], 0, 1));
            }
            else
            {
                Controller.SetInputs(Mathf.Clamp(outputs[0] - outputs[1], -1, 1), Mathf.Clamp(outputs[2] - outputs[3], -1, 1), Mathf.Max(0, outputs[4]));
            }
        }

        private void HandleWaypoints()
        {
            if (m_sourceWaypoint is null)
            {
                return;
            }

            // spent too much time between these 2 waypoints, time to do a mutation
            if (Time.time - waypointTimestamp > m_sourceWaypoint.Time)
            {
                //Debug.Log("Waypoint mutation triggered");
                waypointTimestamp = Time.time;
                m_brain.Mutate(UnityEngine.Random.Range(0, 0.25f), UnityEngine.Random.Range(0, 0.25f));
            }
        }
    }
}