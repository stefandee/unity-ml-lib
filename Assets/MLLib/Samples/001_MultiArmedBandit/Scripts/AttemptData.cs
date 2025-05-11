namespace PironGames.MLLib.Samples.MultiArmBandit
{
    public class AttemptData
    {
        public float SuccessRate = 0;
        
        private int Total = 0;

        private int ObservedSuccessRate = 0;

        private OneArmBanditBehaviour Behaviour;

        private float m_ObservedSuccessRate = 1;

        public float GetObservedSuccessRate()
        {
            return m_ObservedSuccessRate;
        }

        public void LogAttempt()
        {
            Total++;

            bool success = UnityEngine.Random.value <= SuccessRate;

            if (success)
            {
                ObservedSuccessRate++;
            }

            m_ObservedSuccessRate = Total > 0 ? (float)ObservedSuccessRate / Total : 1;

            if (Behaviour != null)
            {
                Behaviour.UpdateObservedSuccessRate(m_ObservedSuccessRate);
            }
        }

        public void Reset(OneArmBanditBehaviour b, float successRate)
        {
            Total = 0;
            ObservedSuccessRate = 0;
            m_ObservedSuccessRate = 1;
            SuccessRate = successRate;
            Behaviour = b;

            if (Behaviour != null)
            {
                Behaviour.Reset(SuccessRate, m_ObservedSuccessRate);
            }
        }
    }
}
