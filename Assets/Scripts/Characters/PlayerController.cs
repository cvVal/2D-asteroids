using UnityEngine;

namespace Characters
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 1f;

        [SerializeField] private float rotateSpeed = 120f;

        [SerializeField] private float maxSpeed = 4f;

        [Header("Shooting Settings")]
        [SerializeField] private GameObject bulletPrefab;

        private Rigidbody2D _rigidbody2D;
        private float _rotationAmount;
        private bool _thrustHeld;

        private void Awake()
        {
            _rigidbody2D = GetComponent<Rigidbody2D>();
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

        // ----- Event-driven: these are called from GameManager based on PlayerInput events -----
        public void SetThrust(bool held)
        {
            _thrustHeld = held;
        }

        public void SetRotation(float axis)
        {
            _rotationAmount = Mathf.Clamp(axis, -1f, 1f);
        }

        public void OnShoot()
        {
            Shoot();
        }

        private void Shoot()
        {
            Instantiate(bulletPrefab, transform.position, transform.rotation);
        }
        // -----------------------------------------------------------------------------------------
    }
}
