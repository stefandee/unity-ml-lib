using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace PironGames.MLLib.Samples.MultiArmBandit
{

    /// <summary>
    /// Epsilon greedy simulation for the multi-arm bandit problem
    /// 
    /// https://markelsanz14.medium.com/introduction-to-reinforcement-learning-part-1-multi-armed-bandit-problem-618e8cbf9d4b
    /// </summary>
    public class MultiArmBanditSimulation : MonoBehaviour
    {
        [Header("Simulation")]
        public float Epsilon = 0.1f;
        public int SimulationAttempts = 1000;
        public float AttemptDelay = 0.1f;

        [Header("Random")]
        public bool EnableRandomizer = false;
        public int MinRandomBandits = 5;
        public int MaxRandomBandits = 10;
        public float MinSuccessRate = 0.05f;
        public float MaxSuccessRate = 1f;

        [Header("Predefined")]
        public float[] SuccessRates = { 0.1f, 0.3f, 0.05f, 0.55f, 0.4f };

        [Header("Visuals")]
        public GameObject BanditPrefab;
        public GameObject BanditsContainer;
        public TMP_Text SimulationStepText;

        private bool m_IsSimulating = false;

        private float m_SimulationTimestamp = float.NaN;

        private float[] m_SuccessRates;

        private int m_SimulationStep = 0;

        private readonly List<AttemptData> m_attemptData = new();

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (!m_IsSimulating)
            {
                return;
            }

            if (Time.time - m_SimulationTimestamp < AttemptDelay)
            {
                return;
            }

            m_SimulationTimestamp = Time.time;

            SimulationStep();
        }

        public void StartSimulation()
        {
            if (m_IsSimulating)
            {
                return;
            }

            m_IsSimulating = true;
            m_SimulationTimestamp = Time.time;
            m_SimulationStep = 0;

            // create and attach the bandits
            if (EnableRandomizer)
            {
                m_SuccessRates = GenerateSuccessRates(MinRandomBandits, MaxRandomBandits, MinSuccessRate, MaxSuccessRate);
            }
            else
            {
                m_SuccessRates = (float[])SuccessRates.Clone();
            }

            GenerateBandits(m_SuccessRates);
        }

        private void GenerateBandits(float[] successRates)
        {
            var banditCount = successRates.Length;

            m_attemptData.Clear();

            // clear the container
            RemoveAllChildren(BanditsContainer.transform);

            for(int i = 0; i < banditCount; i++)
            {
                GameObject instance = Instantiate(BanditPrefab, BanditsContainer.transform);

                var b = instance.GetComponent<OneArmBanditBehaviour>();

                if (b != null)
                {
                    var attempt = new AttemptData();

                    attempt.Reset(b, successRates[i]);

                    m_attemptData.Add(attempt);
                }
            }
        }

        private void RemoveAllChildren(Transform t)
        {
            int nbChildren = t.childCount;

            for (int i = nbChildren - 1; i >= 0; i--)
            {
                Destroy(t.GetChild(i).gameObject);
            }
        }

        private float[] GenerateSuccessRates(int minCount, int maxCount, float minSuccessRate, float maxSuccessRate)
        {
            int count = Random.Range(minCount, maxCount + 1);
            float[] result = new float[count];

            for(int i = 0; i < count; i++)
            {
                result[i] = Random.Range(minSuccessRate, maxSuccessRate);
            }

            return result;
        }

        private void SimulationStep()
        {
            bool doExplore = Random.value < Epsilon || m_SimulationStep == 0;

            if (doExplore)
            {
                // attempt a random bandit
                int randomEntryIndex = Random.Range(0, m_attemptData.Count);
                m_attemptData[randomEntryIndex].LogAttempt();
            }
            else
            {
                // attempt on the best success rate bandit
                AttemptData bestAttemptData = null;

                foreach(var attemp in m_attemptData)
                {
                    if (bestAttemptData == null || bestAttemptData.GetObservedSuccessRate() < attemp.GetObservedSuccessRate())
                    {
                        bestAttemptData = attemp;
                    }
                }

                bestAttemptData.LogAttempt();
            }

            m_SimulationStep++;

            if (m_SimulationStep >= SimulationAttempts)
            {
                m_IsSimulating = false;
            }

            SimulationStepText.text = m_SimulationStep.ToString("000");
        }
    }
}
