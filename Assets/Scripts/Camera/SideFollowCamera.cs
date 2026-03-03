using UnityEngine;

namespace StarboundSprint.CameraSystem
{
    public class SideFollowCamera : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float smoothTime = 0.2f;
        [SerializeField] private Vector3 offset = new(2f, 1.2f, -10f);
        [SerializeField] private float lookAheadFactor = 1.4f;

        private Vector3 _velocity;
        private Vector3 _lastTargetPos;

        private void Start()
        {
            if (target != null)
            {
                _lastTargetPos = target.position;
            }
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            float xDelta = target.position.x - _lastTargetPos.x;
            float lookAhead = Mathf.Sign(xDelta) * Mathf.Min(Mathf.Abs(xDelta) * lookAheadFactor, 2.5f);

            Vector3 desired = new Vector3(target.position.x + offset.x + lookAhead, target.position.y + offset.y, offset.z);
            transform.position = Vector3.SmoothDamp(transform.position, desired, ref _velocity, smoothTime);
            _lastTargetPos = target.position;
        }
    }
}
