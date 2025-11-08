using UnityEngine;

namespace ScriptableObjects.Asteroid
{
    [CreateAssetMenu(fileName = "AsteroidConfig", menuName = "Asteroids/Asteroid Config", order = 0)]
    public class AsteroidConfig : ScriptableObject
    {
        [Header("Movement")]
        [SerializeField] private float baseSpeed = 2f;

        [Header("Fragmentation")]
        [Tooltip("Prefab to spawn when this asteroid is destroyed. Leave null for no fragments.")]
        [SerializeField] private Core.Asteroid fragmentPrefab;

        [Tooltip("How many fragments to spawn when destroyed.")]
        [Min(0)] [SerializeField] private int fragmentsCount = 2;

        [Tooltip("If true, fragments split around the parent's current heading; otherwise use random heading.")]
        [SerializeField] private bool inheritHeading = true;

        [Header("Points")]
        [Min(0)] [SerializeField] private int points = 50;

        public float BaseSpeed => baseSpeed;
        public Core.Asteroid FragmentPrefab => fragmentPrefab;
        public int FragmentsCount => fragmentsCount;
        public bool InheritHeading => inheritHeading;
        public int Points => points;
    }
}
