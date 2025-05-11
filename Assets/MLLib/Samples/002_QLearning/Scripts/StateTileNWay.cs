using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace PironGames.MLLib.Samples.QLearning
{
    [Serializable]
    public class TileNWayDef
    {
        [SerializeField]
        public GameObject State;

        [SerializeField]
        public TMP_Text RewardLabel;
    }

    public struct RewardData
    {
        public float Reward;
        public int ActionIndex;
    }

    public class StateTileNWay : MonoBehaviour
    {
        [SerializeField]
        public GameObject Ground;

        [SerializeField]
        private string TextFormat = "N2";

        [SerializeField]
        public TileNWayDef[] Neighbors = new TileNWayDef[0];

        private void Start()
        {
            ResetLabels();
        }

        public void UpdateRewards(RewardData[] rewardData)
        {
            foreach(var entry in rewardData)
            {
                if (Neighbors[entry.ActionIndex].State == null)
                {
                    Neighbors[entry.ActionIndex].RewardLabel.text = "";
                }
                else
                {
                    Neighbors[entry.ActionIndex].RewardLabel.text = entry.Reward.ToString(TextFormat);
                }
            }
        }

        public void ResetLabels()
        {
            foreach(var neighbor in Neighbors)
            {
                neighbor.RewardLabel.text = "";
            }
        }

        public TileNWayDef GetValidRandomNeighbor()
        {
            List<TileNWayDef> validNeighbors = new();

            foreach(var neighbor in Neighbors)
            {
                if (neighbor.State != null)
                {
                    validNeighbors.Add(neighbor);
                }
            }

            return validNeighbors.Count > 0 ? validNeighbors[UnityEngine.Random.Range(0, validNeighbors.Count - 1)] : null;
        }
    }
}