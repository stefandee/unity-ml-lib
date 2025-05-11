namespace PironGames.MLLib.Samples.Common
{
    using UnityEngine;

    public class CameraFollow : MonoBehaviour
    {
        public enum UpdateStyleEnum
        {
            Fixed,
            Normal,
            Late
        }

        public Transform Target;

        public float LookDistance = 3;
        public float SmoothTime = 6f;
        public Vector3 LocalCameraOffset = new (0, 0, 0);
        public Vector3 LocalLookTargetOffset = new (0, 0, 0);

        public UpdateStyleEnum UpdateStyle = UpdateStyleEnum.Fixed;
        Vector3 currentVelocity;

        // Use this for initialization
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
            if (UpdateStyle == UpdateStyleEnum.Normal)
            {
                InternalCameraUpdate();
            }
        }

        void FixedUpdate()
        {
            if (UpdateStyle == UpdateStyleEnum.Fixed)
            {
                InternalCameraUpdate();
            }
        }

        void LateUpdate()
        {
            if (UpdateStyle == UpdateStyleEnum.Late)
            {
                InternalCameraUpdate();
            }
        }

        void InternalCameraUpdate()
        {
            if (Target)
            {
                var cameraTargetPosition = Target.TransformPoint(LocalCameraOffset);
                var cameraLookTargetPosition = Target.TransformPoint(LocalLookTargetOffset + Vector3.forward * LookDistance);

                transform.position = Vector3.Lerp(transform.position, cameraTargetPosition, Time.deltaTime * SmoothTime);
                transform.LookAt(cameraLookTargetPosition);
            }
        }
    }
}