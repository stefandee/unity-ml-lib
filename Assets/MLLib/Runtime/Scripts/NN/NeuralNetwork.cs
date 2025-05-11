using System;
using System.Collections.Generic;
using UnityEngine;

namespace PironGames.MLLib.NN
{
    [Serializable]
    public class NeuralNetwork
    {
        [SerializeField]
        public List<Level> m_levels = new();

        public NeuralNetwork(int[] neuronCounts, ActivationFunction[] activationFunctions)
        {
            for (var i = 0; i < neuronCounts.Length - 1; i++)
            {
                m_levels.Add(new Level(neuronCounts[i], neuronCounts[i + 1], activationFunctions[i]));
            }
        }

        public void Mutate(float tWeight = 1f, float tBias = 1f)
        {
            foreach (var level in m_levels)
            {
                for (var i = 0; i < level.m_biases.Length; i++)
                {
                    level.m_biases[i] = Mathf.Lerp(
                        level.m_biases[i],
                        UnityEngine.Random.Range(-1f, 1f),
                        tWeight
                    );
                }

                for (var i = 0; i < level.m_weights.Length; i++)
                {
                    level.m_weights[i] = Mathf.Lerp(
                        level.m_weights[i],
                        UnityEngine.Random.Range(-1f, 1f),
                        tBias
                    );
                }

                //for (var i = 0; i < level.m_weights.GetLength(0); i++)
                //{
                //    for (var j = 0; j < level.m_weights.GetLength(1); j++)
                //    {
                //        level.m_weights[i, j] = Mathf.Lerp(
                //            level.m_weights[i, j],
                //            UnityEngine.Random.Range(-1f, 1f),
                //            amount
                //        );
                //    }
                //}
            }
        }

        public void FeedForward(float[] givenInputs)
        {
            if (m_levels.Count == 0)
            {
                UnityEngine.Debug.Log("No levels defined for this neural network!");

                return;
            }

            m_levels[0].FeedForward(givenInputs);

            for (var i = 1; i < m_levels.Count; i++)
            {
                // feed the previous level outputs into the inputs of the current level
                m_levels[i].FeedForward(m_levels[i - 1].m_outputs);
            }
        }

        public void Reset()
        {
            for (var i = 0; i < m_levels[0].m_inputs.Length; i++)
            {
                m_levels[0].m_inputs[i] = 0;
            }

            for (var i = 0; i < m_levels[m_levels.Count - 1].m_outputs.Length; i++)
            {
                m_levels[m_levels.Count - 1].m_outputs[i] = 0;
            }
        }

        public float[] FinalOutputs()
        {
            return m_levels[m_levels.Count - 1].m_outputs;
        }
    }
}