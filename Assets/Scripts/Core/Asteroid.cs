using Managers;
using ScriptableObjects.Asteroid;
using UnityEngine;
using Utility;

namespace Core
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class Asteroid : MonoBehaviour
    {
        private Rigidbody2D _rigidbody2D;

        [SerializeField] private AsteroidConfig config;
        
        public AsteroidConfig Config => config;

        // Back-reference to the prefab type used by the pool
        public Asteroid PrefabAsset { get; set; }

        public AsteroidManager Manager { get; set; }

        private void Awake()
        {
            _rigidbody2D = GetComponent<Rigidbody2D>();
        }

        public void SetVelocity(Vector2 velocity)
        {
            _rigidbody2D.linearVelocity = velocity;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag(Constants.BulletTag)) return;
            
            Manager?.HandleBulletHit(this, other.gameObject);
        }
    }
}
