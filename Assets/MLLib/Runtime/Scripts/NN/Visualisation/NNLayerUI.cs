namespace PironGames.MLLib.NN.Visualisation
{
    using PironGames.MLLib.NN;
    using System.Collections.Generic;
    using UnityEngine;

    public class NNLayerUI : MonoBehaviour
    {
        public GameObject NNNodePrefab;

        private Level m_level;

        private Dictionary<int, NNNodeUI> m_nodeIndex;

        private bool m_useInputs = true;

        public void SetLayer(Level level, bool useInputs = true)
        {
            Cleanup();

            m_level = level;
            m_useInputs = useInputs;

            var values = m_useInputs ? m_level.m_inputs : m_level.m_outputs;

            for (int i = 0, len = values.Length; i < len; i++)
            {
                GameObject node = Instantiate(NNNodePrefab, transform);
                NNNodeUI nodeUI = node.GetComponent<NNNodeUI>();

                m_nodeIndex.Add(i, nodeUI);

                if (nodeUI is not null)
                {
                    nodeUI.SetValues(i, values[i], 0);
                }
            }
        }

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            var values = m_useInputs ? m_level.m_inputs : m_level.m_outputs;

            foreach (var key in m_nodeIndex.Keys)
            {
                if (m_nodeIndex.TryGetValue(key, out var nodeUI))
                {
                    nodeUI.SetValues(key, values[key], 0);
                }
            }
        }

        private void Cleanup()
        {
            if (m_nodeIndex != null)
            {
                foreach (var node in m_nodeIndex.Values)
                {
                    Destroy(node.gameObject);
                }
            }

            m_nodeIndex = new();
        }
    }
}