namespace PironGames.MLLib.Samples.SelfDrivingCars
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public interface IRaceRecorderEntity
    {
        RaceRecorder RaceRecorder { get; }
    }

    public class LapStats
    {
        public int Lap;
        public float Time;
        public float Crashes;
        public string BrainKey;

        public LapStats()
        {
        }

        public LapStats(LapStats other)
        {
            Lap = other.Lap;
            Time = other.Time;
            Crashes = other.Crashes;
            BrainKey = other.BrainKey;
        }

        public void CopyFrom(LapStats other)
        {
            Lap = other.Lap;
            Time = other.Time;
            Crashes = other.Crashes;
            BrainKey = other.BrainKey;
        }
    }

    public delegate void TimeNotify(float time);  // delegate
    public delegate void LapStatsNotify(List<LapStats> list);  // delegate

    public class RaceRecorder
    {
        public List<LapStats> Stats = new();

        public LapStats BestLap { get => m_bestLap; }

        private LapStats m_bestLap = new() { Time = float.NaN };

        public event TimeNotify OnAverageLapTimeChanged;

        public event TimeNotify OnCurrentLapTimeChanged;

        public event LapStatsNotify OnStatsChanged;

        private float m_currentLapTime = float.NaN;

        private float m_prevAverageLapTime = float.NaN;

        //private float m_totalLapTime = float.NaN;

        private float m_averageLapTime = float.NaN;

        public void AddLapStats(int lap, float time, int crashes, string brainKey)
        {
            if (float.IsNaN(m_bestLap.Time))
            {
                m_bestLap.Time = time;
            }
            else
            {
                m_bestLap.Time = MathF.Min(m_bestLap.Time, time);
            }

            m_bestLap.Lap = lap;
            m_bestLap.Crashes = crashes;
            m_bestLap.BrainKey = brainKey;

            var lapStats = new LapStats() { Lap = lap, Time = time, Crashes = crashes, BrainKey = brainKey };
            Stats.Add(lapStats);

            // process average lap time
            var fastestLapTime = float.MaxValue;
            var slowestLapTime = float.MinValue;
            float totalTime = 0;

            foreach (var s in Stats)
            {
                fastestLapTime = Mathf.Min(s.Time, fastestLapTime);
                slowestLapTime = Mathf.Max(s.Time, slowestLapTime);
                totalTime += s.Time;
            }

            if (Stats.Count > 2)
            {
                totalTime -= (fastestLapTime + slowestLapTime);
                m_averageLapTime = totalTime / (Stats.Count - 2);
            }
            else
            {
                m_averageLapTime = totalTime / Stats.Count;
            }

            //if (float.IsNaN(m_totalLapTime))
            //{
            //    m_totalLapTime = time;
            //    m_averageLapTime = time;
            //}
            //else
            //{
            //    m_totalLapTime += time;
            //    m_averageLapTime = m_totalLapTime / Stats.Count;
            //}

            if (m_averageLapTime != m_prevAverageLapTime)
            {
                OnAverageLapTimeChanged?.Invoke(m_averageLapTime);
                m_prevAverageLapTime = m_averageLapTime;
            }

            // process the best 5 lap times
            if (OnStatsChanged != null)
            {
                OnStatsChanged?.Invoke(GetBestLaps());
            }
        }

        public List<LapStats> GetBestLaps(int count = 5)
        {
            var bestLaps = new List<LapStats>();
            bestLaps.AddRange(Stats);
            bestLaps.Sort(delegate (LapStats lap1, LapStats lap2)
            {
                if (lap1.Time < lap2.Time) return -1;
                if (lap1.Time > lap2.Time) return 1;

                return 0;
            });

            return new List<LapStats>(bestLaps.Take(count));
        }

        public float AverageLapTime
        {
            get => m_averageLapTime;
            //{
            //    if (Stats.Count == 0)
            //    {
            //        return float.NaN;
            //    }

            //    float averageTime = 0;

            //    foreach (var lapStats in Stats)
            //    {
            //        averageTime += lapStats.Time;
            //    }

            //    var avg = averageTime / Stats.Count;

            //    return avg;
            //}
        }

        public LapStats LastLap { get => Stats.Count > 0 ? Stats.Last() : null; }

        public float CurrentLapTime
        {
            get => m_currentLapTime;
            set
            {
                var prev = m_currentLapTime;

                m_currentLapTime = value;

                if (prev != value)
                {
                    OnCurrentLapTimeChanged?.Invoke(value);
                }
            }
        }

        public void Reset()
        {
            Stats.Clear();
            m_bestLap.Time = float.NaN;

            m_currentLapTime = float.NaN;

            m_prevAverageLapTime = float.NaN;

            m_averageLapTime = float.NaN;
        }
    }
}