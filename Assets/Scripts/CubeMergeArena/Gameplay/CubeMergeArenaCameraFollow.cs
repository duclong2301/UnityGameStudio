using UnityEngine;

namespace CubeMergeArena.Gameplay
{
    public sealed class CubeMergeArenaCameraFollow : MonoBehaviour
    {
        [SerializeField] private CubeSnake target;
        [SerializeField] private Vector3 landscapeOffset = new Vector3(0f, 18.2f, -13.65f);
        [SerializeField] private Vector3 portraitOffset = new Vector3(0f, 25.48f, -19.11f);
        [SerializeField, Range(0.01f, 1f)] private float followLerp = 0.3f;
        [SerializeField] private float fieldOfView = 45f;

        public void Initialize(CubeSnake followTarget)
        {
            target = followTarget;
            ApplyImmediate();
        }

        private void LateUpdate()
        {
            if (target == null || !target.IsAlive) return;

            var offset = Screen.height > Screen.width ? portraitOffset : landscapeOffset;
            var targetPosition = target.HeadPosition + offset;
            transform.position = Vector3.Lerp(transform.position, targetPosition, followLerp);
            transform.LookAt(target.HeadPosition, Vector3.up);

            if (TryGetComponent(out Camera camera))
            {
                camera.fieldOfView = fieldOfView;
            }
        }

        private void ApplyImmediate()
        {
            if (target == null || !target.IsAlive) return;

            var offset = Screen.height > Screen.width ? portraitOffset : landscapeOffset;
            transform.position = target.HeadPosition + offset;
            transform.LookAt(target.HeadPosition, Vector3.up);
        }
    }
}
