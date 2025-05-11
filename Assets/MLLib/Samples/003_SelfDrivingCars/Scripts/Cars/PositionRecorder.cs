namespace PironGames.MLLib.Samples.SelfDrivingCars
{
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Attach this to a car for certain training supervisors
    /// </summary>
    public class PositionRecorder
    {
        private struct PositionAndTime
        {
            public Vector3 Position;
            public float TimeStamp;
        }

        private LinkedList<PositionAndTime> m_positions = new();

        /// <summary>
        /// Max entities in <see cref="m_positions"/>
        /// </summary>
        private int m_maxPositions = -1;

        public PositionRecorder(int maxPositions = -1)
        {
            m_maxPositions = maxPositions;
        }

        public void Record(float timeStamp, Vector3 position)
        {
            if (m_maxPositions == 0)
            {
                return;
            }

            if (m_maxPositions > 0 && m_positions.Count >= m_maxPositions)
            {
                // remove the oldest
                m_positions.RemoveFirst();
            }

            m_positions.AddLast(new PositionAndTime() { Position = position, TimeStamp = timeStamp });
        }

        public void Clear()
        {
            m_positions.Clear();
        }

        /// <summary>
        /// Checks if the positions recorded in the last "time" period are within a certain radius
        /// 
        /// E.g. for the past 2 minutes, did we recorded positions with a radius of 5 meters?
        /// </summary>
        /// <param name="time"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        public bool IsLoitering(float time, float radius)
        {
            if (m_positions.Count <= 1)
            {
                return false;
            }

            var latestTimeStamp = m_positions.Last.Value.TimeStamp;
            var current = m_positions.Last;

            var min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            var max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            while (current != null)
            {
                //bounds.Expand(current.Value.Position);
                var curPos = current.Value.Position;

                min.x = Mathf.Min(min.x, curPos.x);
                min.y = Mathf.Min(min.y, curPos.y);
                min.z = Mathf.Min(min.z, curPos.z);

                max.x = Mathf.Max(max.x, curPos.x);
                max.y = Mathf.Max(max.y, curPos.y);
                max.z = Mathf.Max(max.z, curPos.z);

                if (latestTimeStamp - current.Value.TimeStamp >= time)
                {
                    var sqrMag = (max - min).sqrMagnitude / 4;

                    return sqrMag <= radius;
                }

                current = current.Previous;
            }

            return false;

            //// "diameter" of the bounding box
            //var sqrMag = (max - min).sqrMagnitude / 4;

            //return sqrMag <= radius;
        }

        public bool OutOfBounds()
        {
            return m_positions.Count > 0 && m_positions.Last.Value.Position.y < -10;
        }
    }
}