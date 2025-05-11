namespace PironGames.MLLib.Samples.SelfDrivingCars
{
    using PironGames.MLLib.Samples.Common;
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Experiment-based reinforced learning simulation manager.
    /// 
    /// Uses a genetic algorithm-like approach to select the best cars to promote to next simulation phase.
    /// 
    /// Still needs improvements.
    /// </summary>
    public class RLSimulationManager : MonoBehaviour
    {
        public GameObject CarPrefab;

        public GameObject CarContainer;

        public GateWaypoint SpawnPoint;

        public float LoiteringTime = 10f; // seconds

        public float LoiteringRadius = 0.5f; // Unity units

        public float CarGenerationDelay = 0.5f;

        public float ExperimentDurationShort = 300f;

        public float ExperimentDurationLong = 600f;

        public string ExperimentKey = "experiment_nn_xx_xx_xx";

        public bool AllowMutations = true;

        public bool RespawnLoiteringCars = false;

        public int MaxCars = 25;

        //private const string BRAIN_KEY = "best_brain_for_track01";
        //private const string BRAIN_KEY = "best_brain_for_track01";

        private List<IAICar> m_cars = new();

        private float timeStampCarGeneration;

        private float timeStampExperimentStart;

        private bool m_firstCar = true;

        private int carCount = 0;

        private BestLap m_fastestLap = new();

        private BestLap m_bestAverageLap = new();

        private BestLap m_bestNotFinished = new();

        private List<ExperimentResult> m_experimentResults = new();

        private int m_experimentIndex = int.MinValue;

        // Use this for initialization
        void Start()
        {
            timeStampCarGeneration = -1; // timeStampSaveDelay = Time.time;
            timeStampExperimentStart = Time.time;

            if (PlayerPrefs.HasKey(ExperimentKey))
            {
                var experiment = JsonUtility.FromJson<Experiment>(PlayerPrefs.GetString(ExperimentKey));
                m_experimentResults = new(experiment.Results);

                foreach (var e in m_experimentResults)
                {
                    m_experimentIndex = Mathf.Max(e.Index, m_experimentIndex);
                }

                Debug.Log($"Experiments found {m_experimentResults.Count}");
            }
        }

        // Update is called once per frame
        void Update()
        {
            ManageExperiment();

            var prevFastestCar = m_fastestLap.Car ?? m_bestNotFinished.Car;
            var prevBestAverageCar = m_bestAverageLap.Car;

            FindBestCars();

            FastestCarReaction(prevFastestCar, m_fastestLap.Car ?? m_bestNotFinished.Car);
            BestAverageCarReaction(prevBestAverageCar, m_bestAverageLap.Car);

            GenerateNewCar();
            CheckCarsState();
        }

        void ManageExperiment()
        {
            if (m_fastestLap.Stats == null && Time.time - timeStampExperimentStart >= ExperimentDurationShort ||
                Time.time - timeStampExperimentStart >= ExperimentDurationLong)
            {

                // check if the experiment is worth saving (there's at least a car that has finished a lap
                if (m_bestAverageLap.Stats != null || m_fastestLap.Stats != null)
                {
                    Debug.Log($"Experiment {m_experimentIndex} will be saved");

                    var experimentResult = new ExperimentResult()
                    {
                        BestAverageBrainKey = m_bestAverageLap.Stats != null ? m_bestAverageLap.Stats.BrainKey : "",
                        FastestBrainKey = m_fastestLap.Stats != null ? m_fastestLap.Stats.BrainKey : "",
                        Ongoing = false,
                        Index = m_experimentIndex++
                    };

                    m_experimentResults.Add(experimentResult);

                    var experiment = new Experiment() { Results = m_experimentResults.ToArray() };
                    var experimentJson = JsonUtility.ToJson(experiment);

                    PlayerPrefs.SetString(ExperimentKey, experimentJson);
                    PlayerPrefs.Save();
                }
                else
                {
                    Debug.Log("Experiment failed, no cars completed first lap");
                }

                Debug.Log("----------------------\nNext Experiment\n----------------------\n");
                ResetExperiment();
            }
        }

        void ResetExperiment()
        {
            timeStampCarGeneration = -1;
            timeStampExperimentStart = Time.time;
            m_fastestLap = new();
            m_bestAverageLap = new();
            m_bestNotFinished = new();

            m_firstCar = true;
            carCount = 0;

            foreach (var car in m_cars)
            {
                if (car is IRemovable removable)
                {
                    removable.Remove(-1);
                }
            }

            m_cars = new();
        }

        void GenerateNewCar()
        {
            if (!(timeStampCarGeneration == -1 || Time.time - timeStampCarGeneration > CarGenerationDelay) || m_cars.Count >= MaxCars)
            {
                return;
            }

            // generate a new car and use the best car ai
            GameObject car = Instantiate(CarPrefab, CarContainer.transform);
            IAICar aiCar = car.GetComponent<IAICar>();

            car.transform.position = SpawnPoint.transform.position;
            car.name = $"Car {carCount++}";

            if (aiCar is not null)
            {
                aiCar.SetupBrain(SpawnPoint, GetBrainKey(), !m_firstCar && AllowMutations ? 0.25f : 0, true);
                m_firstCar = false;

                SetCarStartingDirection(car, SpawnPoint);
            }

            m_cars.Add(aiCar);

            timeStampCarGeneration = Time.time;
        }

        void CheckCarsState()
        {
            //if (Time.time - timeStampSaveDelay < SaveAIDelay)
            //{
            //    return;
            //}

            //if (m_fastestLap.Stats != null && !string.IsNullOrEmpty(m_fastestLap.Stats.BrainKey))
            //{
            //    PlayerPrefs.SetString(BrainKey, m_fastestLap.Stats.BrainKey);
            //    PlayerPrefs.Save();
            //}

            if (!RespawnLoiteringCars)
            {
                m_cars.RemoveAll(car =>
                {
                    if (car is IPositionRecorderEntity recorder && car is IRemovable removable && (recorder.PositionRecorder.IsLoitering(LoiteringTime, LoiteringRadius) || recorder.PositionRecorder.OutOfBounds()))
                    {
                        removable.Remove(3f);

                        return true;
                    }

                    return false;
                });
            }
            else
            {
                foreach (var car in m_cars)
                {
                    if (car is IWaypointFollower wpFollower && car is MonoBehaviour carGO && car is IPositionRecorderEntity recorder && (recorder.PositionRecorder.IsLoitering(LoiteringTime, LoiteringRadius) || recorder.PositionRecorder.OutOfBounds()))
                    {
                        Debug.Log("Respawn car");
                        SetCarStartingDirection(carGO.gameObject, wpFollower.Waypoint);

                        if (car is IAICar aiCar)
                        {
                            aiCar.SetupBrain(wpFollower.Waypoint, GetBrainKey(), !m_firstCar && AllowMutations ? 0.01f : 0, false);
                        }

                        recorder.PositionRecorder.Clear();

                        if (car is IRaceRecorderEntity raceRecorder)
                        {
                            raceRecorder.RaceRecorder.Reset();
                        }
                    }
                }
            }

            //timeStampSaveDelay = Time.time;
        }

        void FindBestCars()
        {
            foreach (var car in m_cars)
            {
                if (car is not IRaceRecorderEntity recorder)
                {
                    continue;
                }

                if (recorder.RaceRecorder.BestLap == null)
                {
                    continue;
                }

                if (float.IsNaN(recorder.RaceRecorder.BestLap.Time))
                {
                    continue;
                }

                if (m_fastestLap.Stats == null || m_fastestLap.Stats.Time > recorder.RaceRecorder.BestLap.Time)
                {
                    m_fastestLap.Car = car;

                    if (m_fastestLap.Stats == null)
                    {
                        m_fastestLap.Stats = new(recorder.RaceRecorder.BestLap);
                    }
                    else
                    {
                        m_fastestLap.Stats.CopyFrom(recorder.RaceRecorder.BestLap);
                    }
                }

                if (!float.IsNaN(recorder.RaceRecorder.AverageLapTime) && (m_bestAverageLap.Stats == null || m_bestAverageLap.Stats.Time > recorder.RaceRecorder.AverageLapTime))
                {
                    m_bestAverageLap.Car = car;

                    if (m_bestAverageLap.Stats == null)
                    {
                        m_bestAverageLap.Stats = new() { Time = recorder.RaceRecorder.AverageLapTime };
                    }
                    else
                    {
                        m_bestAverageLap.Stats.Time = recorder.RaceRecorder.AverageLapTime;
                    }
                }
            }

            if (m_bestAverageLap.Car == null && m_fastestLap.Car == null && m_cars.Count > 0)
            {
                try
                {
                    m_cars.Sort();
                }
                catch (Exception)
                {
                }

                m_bestNotFinished.Car = m_cars[m_cars.Count - 1];

                if (m_bestNotFinished.Stats == null)
                {
                    m_bestNotFinished.Stats = new() { BrainKey = m_bestNotFinished.Car.SerializedBrain };
                }
                else
                {
                    m_bestNotFinished.Stats.BrainKey = m_bestNotFinished.Car.SerializedBrain;
                }
            }
            else
            {
                m_bestNotFinished.Car = null;
                m_bestNotFinished.Stats = null;
            }
        }

        void SetCarStartingDirection(GameObject car, GateWaypoint wp)
        {
            if (wp.Next == null)
            {
                return;
            }

            car.transform.LookAt(wp.Next.transform, Vector3.up);
            car.transform.position = wp.Spawn.transform.position;
            //car.transform.Rotate(new Vector3(0, 180, 0));
        }

        void FastestCarReaction(IAICar prevBestCar, IAICar bestCar)
        {
            if (bestCar != prevBestCar)
            {
                if (bestCar is IEmotable bestCarEmote)
                {
                    bestCarEmote.React(ReactionEmote.Fastest);
                }

                if (prevBestCar is IEmotable prevBestCarEmote)
                {
                    prevBestCarEmote.React(ReactionEmote.None);
                }
            }
        }

        void BestAverageCarReaction(IAICar prev, IAICar best)
        {
            if (best != prev)
            {
                if (best is IEmotable bestCarEmote)
                {
                    bestCarEmote.React(ReactionEmote.BestAverage);
                }

                if (prev is IEmotable prevBestCarEmote)
                {
                    prevBestCarEmote.React(ReactionEmote.None);
                }
            }
        }

        string GetBrainKey()
        {
            if (m_fastestLap.Stats != null && !string.IsNullOrEmpty(m_fastestLap.Stats.BrainKey))
            {
                Debug.Log("best brain (at least one lap)");
                return m_fastestLap.Stats.BrainKey;
            }

            if (m_bestNotFinished.Stats != null)
            {
                Debug.Log("best lap car (lap0)");
                return m_bestNotFinished.Stats.BrainKey;
            }

            //return PlayerPrefs.HasKey(BrainKey) ? PlayerPrefs.GetString(BrainKey, "") : null;
            return null;
        }
    }
}