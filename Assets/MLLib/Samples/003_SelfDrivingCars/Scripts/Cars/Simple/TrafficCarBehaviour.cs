namespace PironGames.MLLib.Samples.SelfDrivingCars
{
    using UnityEngine;

    public class TrafficCarBehaviour : MonoBehaviour
    {
        private float m_Velocity = 0;

        private bool m_velocitySet = false;

        public void SetRandomVelocity(float min, float max)
        {
            m_Velocity = 0; // Random.Range(min, max);
            m_velocitySet = true;
        }

        // Use this for initialization
        void Start()
        {
            if (!m_velocitySet)
            {
                //SetRandomVelocity(0.5f, 1.5f);
                SetRandomVelocity(0, 0);
            }
        }

        // Update is called once per frame
        void Update()
        {
            Vector3 direction = transform.forward;

            transform.position += m_Velocity * Time.deltaTime * direction;
        }
    }
}