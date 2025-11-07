using UnityEngine;
using Utility;
using Core;
using Managers;

namespace Characters
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
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
        private bool _isThrustHeld;
        private bool _isInvincible;

        private void Awake()
        {
            _rigidbody2D = GetComponent<Rigidbody2D>();
        }

        private void FixedUpdate()
        {
            if (_isThrustHeld)
            {
                _rigidbody2D.AddForce(transform.up * moveSpeed);
            }

            _rigidbody2D.AddTorque(-_rotationAmount * rotateSpeed);

            _rigidbody2D.linearVelocity = Vector2.ClampMagnitude(_rigidbody2D.linearVelocity, maxSpeed);
        }

        // ----- Event-driven: these are called from GameManager based on PlayerInput events -----
        public void SetThrust(bool held)
        {
            _isThrustHeld = held;
        }

        public void SetRotation(float axis)
        {
            _rotationAmount = Mathf.Clamp(axis, -1f, 1f);
        }

        public void OnShoot()
        {
            if (bulletPrefab)
            {
                Instantiate(bulletPrefab, transform.position, transform.rotation);
            }
        }

        public void StartInvincibility(float duration)
        {
            CancelInvoke(nameof(EndInvincibility));

            _isInvincible = true;

            var animator = GetComponentInChildren<Animator>(true);
            if (animator)
            {
                animator.SetBool(Constants.AnimatorParamIsInvincible, true);
            }

            if (duration > 0f)
            {
                Invoke(nameof(EndInvincibility), duration);
            }
            else
            {
                EndInvincibility();
            }
        }

        public void EndInvincibility()
        {
            // If this object was destroyed, Invoke won't call; guard anyway
            var animator = GetComponentInChildren<Animator>(true);
            if (animator)
            {
                animator.SetBool(Constants.AnimatorParamIsInvincible, false);
            }

            _isInvincible = false;
        }
        // -----------------------------------------------------------------------------------------

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_isInvincible) return;

            if (other.CompareTag(Constants.AsteroidTag) 
                || other.CompareTag(Constants.EnemyLaserTag))
            {
                EventManager.TriggerPlayerDeath();
                Destroy(gameObject);
            }
            else if (other.CompareTag(Constants.EnemyTag))
            {
                EventManager.TriggerPlayerDeath();
                Destroy(gameObject);
                Destroy(other.gameObject);
            }
        }
    }
}
