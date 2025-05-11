namespace PironGames.MLLib.Samples.SelfDrivingCars
{
    using UnityEngine;

    public class PhysicsPlayerCarBehaviour : MonoBehaviour
    {
        public BasePhysicsCarBehaviour Controller;

        void Update()
        {
            Controller.SetInputs(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), Input.GetAxis("Brake"));
        }
    }
}