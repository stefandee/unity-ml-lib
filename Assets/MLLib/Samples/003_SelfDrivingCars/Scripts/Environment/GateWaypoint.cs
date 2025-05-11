namespace PironGames.MLLib.Samples.SelfDrivingCars
{
    using UnityEngine;

    public class GateWaypoint : MonoBehaviour, IWaypoint
    {
        public GateWaypoint Next;
        public float Time = 10;
        public GameObject Spawn;
        public WaypointTrigger Trigger;
    }
}