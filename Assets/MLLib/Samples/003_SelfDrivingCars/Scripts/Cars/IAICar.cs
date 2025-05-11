namespace PironGames.MLLib.Samples.SelfDrivingCars
{
    using PironGames.MLLib.NN;
    using System;

    public interface IAICar : IComparable
    {
        string SerializedBrain { get; }

        NeuralNetwork Brain { get; }

        void SetupBrain(GateWaypoint spawn, string json, float mutate, bool startRace);
    }
}