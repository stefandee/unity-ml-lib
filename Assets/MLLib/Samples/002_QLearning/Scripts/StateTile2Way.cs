using System.Collections;
using TMPro;
using UnityEngine;

namespace PironGames.MLLib.Samples.QLearning
{
    public class StateTile2Way : MonoBehaviour
    {
        [SerializeField]
        private string TextFormat = "N2";

        [SerializeField]
        public GameObject Ground;

        [SerializeField]
        private TMP_Text LeftReward;

        [SerializeField]
        private TMP_Text RightReward;

        private void Start()
        {
            ResetLabels();
        }

        public void UpdateRewards(float left, float right)
        {
            LeftReward.text = left.ToString(TextFormat);
            RightReward.text = right.ToString(TextFormat);
        }

        public void ResetLabels()
        {
            LeftReward.text = "";
            RightReward.text = "";
        }
    }
}