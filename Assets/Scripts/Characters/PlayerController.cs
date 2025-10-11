using UnityEngine;

namespace Characters
{
    public class PlayerController : MonoBehaviour
    {
        private Rigidbody2D _rigidbody2D;
        private bool _thrustHeld;
        private float _rotationAmount;

        [SerializeField] private float moveSpeed = 1f;
        [SerializeField] private float rotateSpeed = 120f;
        [SerializeField] private float maxSpeed = 4f;

        private void Awake()
        {
            _rigidbody2D = GetComponent<Rigidbody2D>();
        }

        // Event-driven: these are called from GameManager based on PlayerInput events
        public void SetThrust(bool held)
        {
            _thrustHeld = held;
        }

        public void SetRotation(float axis)
        {
            _rotationAmount = Mathf.Clamp(axis, -1f, 1f);
        }

        private void FixedUpdate()
        {
            if (_thrustHeld)
            {
                _rigidbody2D.AddForce(transform.up * moveSpeed);
            }

            _rigidbody2D.AddTorque(-_rotationAmount * rotateSpeed);

            _rigidbody2D.linearVelocity = Vector2.ClampMagnitude(_rigidbody2D.linearVelocity, maxSpeed);
        }
    }
}