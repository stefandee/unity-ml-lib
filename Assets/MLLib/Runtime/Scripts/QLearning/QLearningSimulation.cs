namespace PironGames.MLLib.QLearning
{
    using PironGames.MLLib.Common;
    using UnityEngine;
    using UnityEngine.Assertions;

    public delegate bool QLearningIsFinalState(int state);

    public delegate int QLearningNextState(int state, int action);

    public delegate int QLearningInitialValue(int state, int action);

    public class QLearningSimulation : ISimulation
    {
        private int m_stateCount = 0;

        private int m_actionCount = 0;

        private float[,] m_qValues;

        private float[] m_rewards;

        private float m_epsilon = 0.1f;

        private float m_discount = 0.9f;

        private int m_currentState;

        private int m_startState;

        private int m_currentStep;

        private QLearningIsFinalState m_delegateIsFinalState;

        private QLearningNextState m_delegateNextState;

        private QLearningInitialValue m_delegateInitialValue;

        public int StateCount
        {
            get => m_stateCount;
        }

        public int ActionCount
        {
            get => m_actionCount;
        }

        public int CurrentStep
        {
            get => m_currentStep;
        }

        public bool IsValid
        {
            get => m_stateCount > 0 && m_actionCount > 0;
        }

        public int CurrentState
        {
            get => m_currentState;
        }

        public bool IsCurrentStateFinal
        {
            get => m_delegateIsFinalState(m_currentState);
        }

        public float Epsilon
        {
            get => m_epsilon;
            set => m_epsilon = Mathf.Clamp(value, 0, 1);
        }

        public void Init(int stateCount, int actionCount, float[] rewards, float epsilon, float discount, int startState, QLearningIsFinalState delegateFinalState, QLearningNextState delegateNextState, QLearningInitialValue delegateInitialValue)
        {
            m_stateCount = Mathf.Max(stateCount, 0);
            m_actionCount = Mathf.Max(actionCount, 0);

            m_startState = m_currentState = Mathf.Clamp(startState, 0, m_stateCount);

            m_delegateIsFinalState = delegateFinalState;
            m_delegateNextState = delegateNextState;
            m_delegateInitialValue = delegateInitialValue;

            InitQValues(m_stateCount, m_actionCount);

            Epsilon = epsilon;
            m_discount = Mathf.Clamp(discount, 0, 1);

            m_rewards = (float[])rewards.Clone();

            ResetEpisode();
        }

        public void ResetEpisode()
        {
            m_currentState = m_startState;
            m_currentStep = 0;
        }

        public void Step()
        {
            if (!IsValid)
            {
                return;
            }

            if (m_delegateIsFinalState(m_currentState))
            {
                return;
            }

            int action = SelectAction(m_epsilon, m_currentState);

            var (reward, nextState) = ApplyAction(action, m_currentState);

            if (m_delegateIsFinalState(nextState))
            {
                m_qValues[m_currentState, action] = reward;
            }
            else
            {
                m_qValues[m_currentState, action] = reward + m_discount * MaxQValue(nextState);
            }

            m_currentState = nextState;

            m_currentStep++;
        }

        public float GetQValue(int state, int action)
        {
            Assert.IsTrue(state >= 0, $"State index {state} must be > 0!");
            Assert.IsTrue(state < m_stateCount, $"State index {state} must be {m_stateCount}");

            state = Mathf.Clamp(state, 0, m_stateCount - 1);

            Assert.IsTrue(action >= 0, $"Action index {action} must be > 0!");
            Assert.IsTrue(action < m_actionCount, $"Action index {action} must be < {m_actionCount}");

            action = Mathf.Clamp(action, 0, m_actionCount - 1);

            return m_qValues[state, action];
        }

        protected int SelectAction(float epsilon, int state)
        {
            var rndValue = UnityEngine.Random.value;

            if (rndValue < epsilon)
            {
                int randomAction = UnityEngine.Random.Range(0, m_actionCount);

                return randomAction;
            }

            // select the action with the maximum q value (e.g. that gives the max reward)
            return MaxQValueIndex(state);
        }

        protected (float reward, int nextState) ApplyAction(int action, int state)
        {
            if (state >= m_stateCount || state < 0)
            {
                throw new System.Exception();
            }

            if (action >= m_actionCount || action < 0)
            {
                throw new System.Exception();
            }

            int nextState = m_delegateNextState(state, action);

            return (reward: m_rewards[nextState], nextState);
        }

        private void InitQValues(int stateCount, int actionCount)
        {
            m_qValues = new float[stateCount, actionCount];

            for (int state = 0; state < stateCount; state++)
            {
                for (int action = 0; action < actionCount; action++)
                {
                    m_qValues[state, action] = m_delegateInitialValue(state, action);
                }
            }
        }

        private float MaxQValue(int state)
        {
            float max = float.MinValue;

            for (int action = 0; action < m_actionCount; action++)
            {
                float qValue = m_qValues[state, action];

                if (max < qValue)
                {
                    max = qValue;
                }
            }

            return max;
        }

        private int MaxQValueIndex(int state)
        {
            float max = float.MinValue;
            int maxIndex = -1;

            for (int action = 0; action < m_actionCount; action++)
            {
                float qValue = m_qValues[state, action];

                if (max < qValue)
                {
                    maxIndex = action;
                    max = qValue;
                }
            }

            return maxIndex;
        }

    }
}
