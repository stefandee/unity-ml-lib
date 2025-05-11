namespace PironGames.MLLib.Samples.SelfDrivingCars
{
    using PironGames.MLLib.NN.Visualisation;
    using PironGames.MLLib.NN;
    using System.Collections.Generic;
    using UnityEngine;

    public class NeuralNetworkUI : MonoBehaviour
    {
        public GameObject NNLayerPrefab;

        private Dictionary<int, NNLayerUI> m_layerIndex;

        private IAICar m_selected;

        private NeuralNetwork m_nn;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            //if (m_selected != null && m_selected.IsDead())
            //{
            //    m_selected = null;
            //    Cleanup();
            //}

            //Check for mouse click 
            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit raycastHit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out raycastHit, 100f))
                {
                    if (raycastHit.transform != null)
                    {
                        CurrentClickedGameObject(raycastHit.transform.gameObject);
                    }
                }
            }
        }

        private void SetNN(NeuralNetwork nn)
        {
            Cleanup();

            m_nn = nn;

            int levelCount = nn.m_levels.Count;

            for (int i = 0; i < levelCount; i++)
            {
                CreateAndAddLayerUI(nn.m_levels[i], i, true);
            }

            CreateAndAddLayerUI(nn.m_levels[levelCount - 1], levelCount, false);
        }

        private void CreateAndAddLayerUI(Level level, int index, bool useInputs)
        {
            GameObject node = Instantiate(NNLayerPrefab, transform);
            NNLayerUI layerUI = node.GetComponent<NNLayerUI>();

            m_layerIndex.Add(index, layerUI);

            if (layerUI is not null)
            {
                layerUI.SetLayer(level, useInputs);
            }
        }

        private void Cleanup()
        {
            if (m_layerIndex != null)
            {
                foreach (var node in m_layerIndex.Values)
                {
                    Destroy(node.gameObject);
                }
            }

            m_layerIndex = new();
        }

        private void CurrentClickedGameObject(GameObject go)
        {
            if (go == null)
            {
                return;
            }

            IAICar car = go.GetComponent<IAICar>();

            if (car != null)
            {
                m_selected = car;
                SetNN(car.Brain);
            }
        }
    }
}