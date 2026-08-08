using MicroEvolution.Core;
using UnityEngine;

namespace MicroEvolution.World
{
    public class CameraFollow : MonoBehaviour
    {
        public Transform Target;
        public float Smooth = 6f;
        Vector3 _velocity;

        public void Configure(Transform target)
        {
            Target = target;
            if (target != null)
                transform.position = new Vector3(target.position.x, target.position.y, -10f);
        }

        void LateUpdate()
        {
            if (Target == null && GameState.Instance != null)
                Target = GameState.Instance.PlayerTransform;
            if (Target == null) return;

            var desired = new Vector3(Target.position.x, Target.position.y, -10f);
            transform.position = Vector3.SmoothDamp(transform.position, desired, ref _velocity, 1f / Smooth);
        }
    }
}
