namespace PironGames.MLLib.Samples.SelfDrivingCars
{
    using System.Collections.Generic;
    using UnityEngine;

    public class Sensor : MonoBehaviour
    {
        private const string ROAD_OBSTACLE_MASK = "RoadObstacle";

        private class RawReadings
        {
            /// <summary>
            /// Position of the hit point (not used)
            /// </summary>
            public Vector3 Pos;

            /// <summary>
            /// Distance from origin to the hit point
            /// </summary>
            public float Distance;

            public bool Valid = false;
        }

        public int RayCount = 3;

        public float RayLength = 5;

        public float RaySpread = 90f / 4;

        private RawReadings[] m_rawReadings;

        private List<Vector3> m_directions = new();

        private bool m_active = true;

        private int m_layerMask;

        public float[] Readings()
        {
            var result = new float[RayCount];

            for (var i = 0; i < RayCount; i++)
            {
                if (m_rawReadings[i].Valid)
                {
                    // the closer the collision, the higher the value
                    result[i] = 1.0f - m_rawReadings[i].Distance / RayLength;
                }
                else
                {
                    result[i] = 0;
                }
            }

            return result;
        }

        public void DisableSensor()
        {
            m_active = false;
        }

        // Use this for initialization
        void Start()
        {
            m_rawReadings = new RawReadings[RayCount];

            for (var i = 0; i < m_rawReadings.Length; i++)
            {
                m_rawReadings[i] = new RawReadings();
            }

            m_layerMask = LayerMask.GetMask(ROAD_OBSTACLE_MASK);
        }


        // Update is called once per frame
        void Update()
        {
            if (!m_active)
            {
                return;
            }

            //Vector3 forward = transform.forward * RayLength;
            //Debug.DrawRay(transform.position, forward, Color.green);
            m_directions.Clear();

            if (RayCount > 1)
            {
                for (int i = 0; i < RayCount; i++)
                {
                    Vector3 forward = Quaternion.AngleAxis(-RaySpread / 2f + (i * RaySpread) / (RayCount - 1), transform.up) * (RayLength * transform.forward);

                    m_directions.Add(forward);
                }
            }
            else
            {
                Vector3 forward = transform.forward * RayLength;
                m_directions.Add(forward);
            }

            for (var i = 0; i < m_directions.Count; i++)
            {
                Color debugColor = Color.green;

                var hits = Physics.RaycastAll(transform.position, m_directions[i], RayLength, m_layerMask);
                var hitIndex = GetClosestHitIndex(hits);

                if (hitIndex != -1)
                {
                    m_rawReadings[i].Valid = true;
                    m_rawReadings[i].Distance = hits[hitIndex].distance;
                    debugColor = Color.red;
                }
                else
                {
                    m_rawReadings[i].Valid = false;
                }

                Debug.DrawRay(transform.position, m_directions[i], debugColor);
            }
        }

        private int GetClosestHitIndex(RaycastHit[] hits)
        {
            int result = -1;
            float minDistance = float.MaxValue;

            for (var i = 0; i < hits.Length; i++)
            {
                // ignore self hits
                if (hits[i].collider.transform == transform)
                {
                    continue;
                }

                // ignore other player cars and the road tile
                //var layer = hits[i].collider.gameObject.layer;

                //if (layer == 8 || layer == 5)
                //{
                //    continue;
                //}    

                if (hits[i].distance < minDistance)
                {
                    minDistance = hits[i].distance;
                    result = i;
                }
            }

            return result;
        }
    }
}