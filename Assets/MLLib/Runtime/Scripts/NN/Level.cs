using System;
using UnityEngine;

namespace PironGames.MLLib.NN
{
    [Serializable]
    public class Level
    {
        [SerializeField]
        public float[] m_inputs;

        [SerializeField]
        public float[] m_outputs;

        [SerializeField]
        public float[] m_biases;

        [SerializeField]
        public float[] m_weights;

        [SerializeField]
        public ActivationFunction m_activation;

        private Func<float, float> m_activationFunc;

        public Level(int inputs, int outputs, ActivationFunction activation)
        {
            m_inputs = new float[inputs];
            m_outputs = new float[outputs];
            m_biases = new float[outputs];

            m_activation = activation;
            m_activationFunc = NNActivationFunctions.FunctionFromEnum(m_activation);

            m_weights = new float[inputs * outputs];

            RandomizeWeights();
        }

        public void FeedForward(float[] givenInputs)
        {
            for (var i = 0; i < m_inputs.Length; i++)
            {
                m_inputs[i] = givenInputs[i];
            }

            var weightsIndex = 0;

            for (var i = 0; i < m_outputs.Length; i++)
            {
                float sum = 0;

                for (var j = 0; j < m_inputs.Length; j++)
                {
                    sum += m_inputs[j] * m_weights[weightsIndex++];
                }

                // turn the output on or off
                // m_outputs[i] = sum > m_biases[i] ? 1 : 0;

                sum += m_biases[i];
                m_outputs[i] = m_activationFunc(sum);
            }
        }

        private void RandomizeWeights()
        {
            for(var i = 0; i < m_inputs.Length; i++)
            {
                m_inputs[i] = 0;
            }

            for(var i = 0; i < m_outputs.Length; i++)
            {
                m_outputs[i] = 0;
            }

            for(var i = 0; i < m_weights.Length; i++)
            {
                m_weights[i] = UnityEngine.Random.Range(-1f, 1f);
            }

            /*
            for (var i = 0; i < m_weights.GetLength(0); i++)
            {
                for (var j = 0; j < m_weights.GetLength(1); j++)
                {
                    m_weights[i, j] = UnityEngine.Random.Range(-1f, 1f);
                }
            }
            */

            for (var i = 0; i < m_biases.Length; i++)
            {
                m_biases[i] = UnityEngine.Random.Range(-1f, 1f);
            }
        }
    }
}