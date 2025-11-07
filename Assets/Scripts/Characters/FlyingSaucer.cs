using Core;
using Managers;
using UnityEngine;
using Utility;
using Random = UnityEngine.Random;

namespace Characters
{
    public class FlyingSaucer : MonoBehaviour
    {
        [SerializeField] private float speed = 2f;
        [SerializeField] private GameObject laserPrefab;
        [SerializeField] private int pointsWorth = 300;

        private Vector2 _currentDirection = Vector2.zero;
        private PlayerController _player;
        private GameManager _gameManager;

        private void Start()
        {
            _player = FindFirstObjectByType<PlayerController>();
            _gameManager = FindFirstObjectByType<GameManager>();

            RandomDirection();
            InvokeRepeating(nameof(ChangeYDirection), 2f, 2f);

            InvokeRepeating(nameof(ShootLaser), 1f, 1.5f);
        }

        private void Update()
        {
            transform.Translate(_currentDirection * (Time.deltaTime * speed));
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag(Constants.BulletTag))
            {
                _gameManager?.HandlePointsScored(pointsWorth);

                EventManager.TriggerEntityDestroyed(transform.position, EffectKeys.GeneralExplosion);

                Destroy(gameObject);
            }
        }

        private void RandomDirection()
        {
            var randomNumber = Random.value;

            _currentDirection = randomNumber switch
            {
                < 0.5f => Vector2.left,
                _ => Vector2.right
            };
        }

        private void ChangeYDirection()
        {
            _currentDirection.y = Random.Range(-1f, 2f);
        }

        private void ShootLaser()
        {
            if (!laserPrefab) return;

            if (_player)
            {
                var laser = Instantiate(laserPrefab, transform.position, Quaternion.identity);

                laser.transform.up = _player.transform.position - transform.position;

                var variance = Random.Range(-25f, 25f);
                laser.transform.Rotate(Vector3.forward * variance);
            }
            else
            {
                _player = FindFirstObjectByType<PlayerController>();
            }
        }
    }
}
