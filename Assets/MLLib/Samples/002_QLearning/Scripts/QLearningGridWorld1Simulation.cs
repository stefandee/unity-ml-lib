namespace PironGames.MLLib.Samples.QLearning
{
    using PironGames.MLLib.QLearning;
    using System;
    using TMPro;
    using UnityEngine;

    public class QLearningGridWorld1Simulation : MonoBehaviour
    {
        [Serializable]
        private class StateDef
        {
            public StateTile2Way Visual;
            public float Reward;
        }

        [SerializeField]
        private StateDef[] StateDefinitions = new StateDef[0];

        [SerializeField]
        private QActor Actor;

        [SerializeField]
        private int StartStateIndex = 0;

        [SerializeField]
        private float Epsilon = 0.2f;

        [SerializeField]
        private float Discount = 0.9f;

        [SerializeField]
        private int Episodes = 1000;

        [SerializeField]
        private int StepsPerEpisode = 25;

        [SerializeField]
        private int ActionCount = 2;

        [SerializeField]
        private TMP_Text SimEpisodeText;

        [SerializeField]
        private TMP_Text SimStepText;

        private QLearningSimulation m_sim;

        private int m_episodeCount = 0;

        private float m_simStepTimestamp = float.NaN;

        private bool m_simulationRunning = false;

        private bool IsFinalState(int state)
        {
            return state == 0 || state == StateDefinitions.Length - 1;
        }

        private int NextState(int state, int action)
        {
            int nextState;

            if (action == 0)
            {
                nextState = state - 1;
            }
            else
            {
                nextState = state + 1;
            }

            return nextState;
        }

        private int InitialValue(int _, int __)
        {
            return 0;
        }

        public void StartSimulation()
        {
            float[] rewards = new float[StateDefinitions.Length];

            for (int i = 0; i < StateDefinitions.Length; i++)
            {
                rewards[i] = StateDefinitions[i].Reward;
            }

            m_sim = new QLearningSimulation();
            m_sim.Init(StateDefinitions.Length, ActionCount, rewards/*new float[] { -5, 0, 0, 0, 0, 0, 5 }*/, Epsilon, Discount, StartStateIndex, IsFinalState, NextState, InitialValue);
            m_sim.ResetEpisode();

            m_simStepTimestamp = Time.time;
            m_episodeCount = 0;
            m_simulationRunning = true;

            UpdateStateLabels();
            SimEpisodeText.text = $"Episode {m_episodeCount}";
            SimStepText.text = $"{m_sim.CurrentStep}/{StepsPerEpisode}";
        }

        // Start is called before the first frame update
        void Start()
        {
            SimEpisodeText.text = "";
            SimStepText.text = "";

            for (int i = 0, stateCount = StateDefinitions.Length; i < stateCount; i++)
            {
                StateDefinitions[i].Visual.ResetLabels();
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (!m_simulationRunning)
            {
                return;
            }

            if (m_episodeCount >= Episodes)
            {
                m_simulationRunning = false;
            }

            if (Actor.IsTransitioning)
            {
                return;
            }

            if (m_sim.IsCurrentStateFinal || m_sim.CurrentStep >= StepsPerEpisode)
            {
                UpdateStateLabels();
                ResetActor();
                m_sim.ResetEpisode();
                m_episodeCount++;

                SimEpisodeText.text = $"Episode {m_episodeCount}";
                SimStepText.text = $"{m_sim.CurrentStep}/{StepsPerEpisode}";
            }
            else
            {
                m_sim.Step();

                SimStepText.text = $"{m_sim.CurrentStep}/{StepsPerEpisode}";
                UpdateStateLabels();
                Actor.Transition(StateDefinitions[m_sim.CurrentState].Visual.Ground, false);
            }
        }

        private void ResetActor()
        {
            Actor.Transition(StateDefinitions[StartStateIndex].Visual.Ground, true);
        }

        private void UpdateCurrentStateLabels()
        {
            var index = m_sim.CurrentState;
            StateDefinitions[index].Visual.UpdateRewards(m_sim.GetQValue(index, 0), m_sim.GetQValue(index, 1));
        }

        private void UpdateStateLabels()
        {
            for (int i = 0, stateCount = m_sim.StateCount; i < stateCount; i++)
            {
                StateDefinitions[i].Visual.UpdateRewards(m_sim.GetQValue(i, 0), m_sim.GetQValue(i, 1));
            }
        }
    }
}