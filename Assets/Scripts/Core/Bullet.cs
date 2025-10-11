using UnityEngine;

namespace Core
{
    public class Bullet : MonoBehaviour
    {
        [SerializeField] private float speed = 10f;
        [SerializeField] private float destroyTime = 1f;

        private void Start()
        {
            Destroy(gameObject, destroyTime);
            GetComponent<Rigidbody2D>()
                .linearVelocity = transform.up * speed;
        }
    }
}
