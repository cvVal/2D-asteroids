using UnityEngine;

namespace Core
{
    public class EnemyLaser : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private float lifeTime = 2.5f;
        
        private void Start()
        {
            GetComponent<Rigidbody2D>().linearVelocity = transform.up * moveSpeed;
            Destroy(gameObject, lifeTime);
        }
    }
}
