namespace PironGames.MLLib.Samples.QLearning
{
    using PironGames.MLLib.QLearning;
    using System;
    using TMPro;
    using UnityEngine;

    public class QLearningGridWorld2Simulation : MonoBehaviour
    {
        [Serializable]
        private class StateDef
        {
            public StateTileNWay Visual;
            public float Reward;
        }

        [SerializeField]
        private StateDef[] StateDefinitions = new StateDef[0];

        [SerializeField]
        private QActor Actor;

        [SerializeField]
        private int StartStateIndex = 0;

        [SerializeField]
        private int EndStateIndex = 0;

        [SerializeField]
        private float Epsilon = 0.2f;

        [SerializeField]
        private float Discount = 0.9f;

        [SerializeField]
        private int Episodes = 1000;

        [SerializeField]
        private int StepsPerEpisode = 25;

        [SerializeField]
        private int ActionCount = 4;

        [SerializeField]
        private TMP_Text SimEpisodeText;

        [SerializeField]
        private TMP_Text SimStepText;

        private QLearningSimulation m_sim;

        private int m_episodeCount = 0;

        private float m_simStepTimestamp = float.NaN;

        private bool m_simulationRunning = false;

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

            ResetActor();
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

            ResetActor();
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
                Debug.Log("<color=green>State reset</color>");

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

        private RewardData[] m_rewardData = new RewardData[4];

        private void UpdateStateRewardData(int state)
        {
            int index = 0;

            foreach(var actionIndex in Enum.GetValues(typeof(DirectionActionIndex)))
            {
                m_rewardData[index].ActionIndex = (int)actionIndex;
                m_rewardData[index].Reward = m_sim.GetQValue(state, (int)actionIndex);
                index++;
            }
        }

        private void UpdateCurrentStateLabels()
        {
            UpdateStateRewardData(m_sim.CurrentState);

            StateDefinitions[m_sim.CurrentState].Visual.UpdateRewards(m_rewardData);
        }

        private void UpdateStateLabels()
        {
            for (int i = 0, stateCount = m_sim.StateCount; i < stateCount; i++)
            {
                UpdateStateRewardData(i);

                StateDefinitions[i].Visual.UpdateRewards(m_rewardData);
            }
        }

        #region QLearning Simulation Delegates
        private bool IsFinalState(int state)
        {
            return state == EndStateIndex;
        }

        private int NextState(int state, int action)
        {
            // search for the state index
            var stateDef = StateDefinitions[state];

            var neighbor = stateDef.Visual.Neighbors[action];

            if (neighbor.State == null)
            {
                neighbor = stateDef.Visual.GetValidRandomNeighbor();
            }

            var nextState = GetStateDefIndexByNeighbor(neighbor);

            Debug.Log($"NextState: {state} -> {nextState}");

            var diff = Mathf.Abs(state - nextState);

            if (diff != 1 && diff != 4)
            {
                Debug.LogWarning($"State index diff is wrong {state} {nextState} {diff}");
            }

            return nextState;
        }

        private int InitialValue(int _, int __)
        {
            return 0;
        }
        #endregion

        private int GetStateDefIndexByNeighbor(TileNWayDef toSearch)
        {
            if (toSearch == null || toSearch.State == null)
            {
                return -1;
            }

            // TODO implement reverse lookup
            for(int i = 0, stateDefLength = StateDefinitions.Length; i < stateDefLength; i++)
            {
                if (toSearch.State.GetComponent<StateTileNWay>() == StateDefinitions[i].Visual)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}