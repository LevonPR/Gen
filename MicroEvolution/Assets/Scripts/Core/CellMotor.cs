using UnityEngine;

namespace MicroEvolution.Core
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class CellMotor : MonoBehaviour
    {
        public float MaxSpeed = 5f;
        public float Acceleration = 18f;
        public float Drag = 2.8f;

        Rigidbody2D _rb;
        Vector2 _desired;

        void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;
            _rb.drag = Drag;
            _rb.angularDrag = 4f;
            _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        }

        public void SetDesiredVelocity(Vector2 desired)
        {
            _desired = Vector2.ClampMagnitude(desired, MaxSpeed);
        }

        void FixedUpdate()
        {
            var delta = _desired - _rb.velocity;
            _rb.AddForce(delta * Acceleration, ForceMode2D.Force);

            if (_rb.velocity.sqrMagnitude > 0.05f)
            {
                var angle = Mathf.Atan2(_rb.velocity.y, _rb.velocity.x) * Mathf.Rad2Deg - 90f;
                var target = Quaternion.Euler(0f, 0f, angle);
                transform.rotation = Quaternion.Lerp(transform.rotation, target, 8f * Time.fixedDeltaTime);
            }
        }

        public Vector2 Velocity => _rb != null ? _rb.velocity : Vector2.zero;
    }
}
