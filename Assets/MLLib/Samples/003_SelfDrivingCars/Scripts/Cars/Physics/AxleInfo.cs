namespace PironGames.MLLib.Samples.SelfDrivingCars
{
    using UnityEngine;

    [System.Serializable]
    public class AxleInfo
    {
        public WheelCollider leftWheel;
        public WheelCollider rightWheel;
        public GameObject leftWheelVisual;
        public GameObject rightWheelVisual;
        public bool motor; // is this wheel attached to motor?
        public bool steering; // does this wheel apply steer angle?
    }
}