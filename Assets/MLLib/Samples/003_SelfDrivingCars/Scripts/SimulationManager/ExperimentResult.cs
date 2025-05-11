namespace PironGames.MLLib.Samples.SelfDrivingCars
{
    using System;
    using UnityEngine;

    [Serializable]
    public class Experiment
    {
        [SerializeField]
        public ExperimentResult[] Results;

        public Experiment()
        {
        }
    }

    [Serializable]
    public class ExperimentResult
    {
        [SerializeField]
        public string FastestBrainKey;

        [SerializeField]
        public string BestAverageBrainKey;

        [SerializeField]
        public int Index;

        [SerializeField]
        public bool Ongoing;
    }
}
