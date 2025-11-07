using System;
using System.Collections.Generic;
using Core;
using UnityEngine;
using UnityEngine.Pool;
using Utility;
using Random = UnityEngine.Random;

namespace Managers
{
    [Serializable]
    public struct SpawnEntry
    {
        public Asteroid prefab;
        public int count;
    }

    public class AsteroidManager : MonoBehaviour
    {
        [Header("Wave Composition")]
        [Tooltip("List of asteroid prefabs and how many to spawn for the base wave.")]
        [SerializeField] private List<SpawnEntry> initialWave = new();

        [Header("Fragmentation")]
        [SerializeField] private float fragmentSpreadAngle = 25f;

        private readonly Dictionary<Asteroid, ObjectPool<Asteroid>> _pools = new();
        private readonly HashSet<Asteroid> _active = new();

        private ObjectPool<Asteroid> GetOrCreatePool(Asteroid prefab)
        {
            if (!prefab)
            {
                Debug.LogError("AsteroidManager: Missing prefab in wave configuration.");
                return null;
            }

            if (_pools.TryGetValue(prefab, out var pool))
                return pool;

            pool = new ObjectPool<Asteroid>(
                createFunc: () =>
                {
                    var asteroid = Instantiate(prefab);

                    // Parent to manager only if this manager is a scene instance
                    if (gameObject.scene.IsValid())
                    {
                        asteroid.transform.SetParent(transform, false);
                    }

                    asteroid.Manager = this;
                    asteroid.PrefabAsset = prefab;
                    asteroid.gameObject.SetActive(false);
                    return asteroid;
                },
                actionOnGet: asteroid =>
                {
                    _active.Add(asteroid);
                    asteroid.gameObject.SetActive(true);
                },
                actionOnRelease: asteroid =>
                {
                    // Reset transient state before returning to pool
                    asteroid.SetVelocity(Vector2.zero);
                    asteroid.gameObject.SetActive(false);
                    _active.Remove(asteroid);
                    if (_active.Count == 0)
                    {
                        EventManager.TriggerWaveComplete();
                    }
                },
                actionOnDestroy: asteroid => Destroy(asteroid.gameObject),
                collectionCheck: true,
                defaultCapacity: 8,
                maxSize: 128
            );

            _pools[prefab] = pool;
            return pool;
        }

        public void StartNewWave(int multiplier = 1)
        {
            if (initialWave == null || initialWave.Count == 0)
            {
                Debug.LogWarning("AsteroidManager: No wave entries configured. Add entries in 'initialWave'.");
                return;
            }

            foreach (var entry in initialWave)
            {
                var pool = GetOrCreatePool(entry.prefab);
                if (pool == null) continue;

                var total = Mathf.Max(0, entry.count) * Mathf.Max(1, multiplier);
                for (var i = 0; i < total; i++)
                {
                    var ast = pool.Get();
                    ast.transform.position = RandomEdgePosition();

                    var cfg = ast.Config;
                    var dir = Random.insideUnitCircle.normalized;
                    var speed = cfg != null ? cfg.BaseSpeed : 2f;
                    ast.SetVelocity(dir * speed);
                }
            }
        }

        public void HandleBulletHit(Asteroid asteroid, GameObject bullet)
        {
            if (bullet) Destroy(bullet);

            EventManager.TriggerPointsScored(asteroid.Config.Points);

            // Derive heading from current velocity (fallback to random)
            var hasRb = asteroid.TryGetComponent<Rigidbody2D>(out var body);
            var heading = hasRb && body.linearVelocity.sqrMagnitude > 0.001f
                ? body.linearVelocity.normalized
                : Random.insideUnitCircle.normalized;

            // Release the hit asteroid
            var destroyedPosition = asteroid.transform.position;
            Release(asteroid);

            EventManager.TriggerEntityDestroyed(destroyedPosition, EffectKeys.GeneralExplosion);

            // Spawn fragments based on the asteroid's config
            var asteroidCfg = asteroid.Config;
            if (!asteroidCfg || !asteroidCfg.FragmentPrefab || asteroidCfg.FragmentsCount <= 0)
                return;

            SpawnFragments(asteroid.transform.position, heading, asteroidCfg);
        }

        private void SpawnFragments(Vector2 position, Vector2 baseDir, AsteroidConfig cfg)
        {
            var count = Mathf.Max(0, cfg.FragmentsCount);
            if (count == 0) return;

            // Spread fragments around baseDir across [-spread, +spread]
            var spread = fragmentSpreadAngle;
            var step = count > 1 ? (2f * spread) / (count - 1) : 0f;
            var start = -spread;

            var fragPool = GetOrCreatePool(cfg.FragmentPrefab);
            var fragSpeed = cfg.FragmentPrefab.Config != null ? cfg.FragmentPrefab.Config.BaseSpeed : 2f;

            for (var i = 0; i < count; i++)
            {
                var angle = start + step * i;
                var dir = cfg.InheritHeading
                    ? Rotate2D(baseDir, angle).normalized
                    : Random.insideUnitCircle.normalized;

                var frag = fragPool.Get();
                frag.transform.position = position;
                frag.SetVelocity(dir * fragSpeed);
            }
        }

        private static Vector2 Rotate2D(Vector2 v, float degrees)
        {
            var rad = degrees * Mathf.Deg2Rad;
            var sin = Mathf.Sin(rad);
            var cos = Mathf.Cos(rad);
            return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
        }

        private void Release(Asteroid ast)
        {
            var prefab = ast.PrefabAsset;
            if (prefab != null && _pools.TryGetValue(prefab, out var pool))
            {
                pool.Release(ast);
            }
            else
            {
                // Fallback: destroy if we somehow don't have a pool
                Destroy(ast.gameObject);
            }
        }

        public static Vector2 RandomEdgePosition()
        {
            var edge = Random.Range(0, 4);
            return edge switch
            {
                // Top, Bottom, Left, Right
                0 => new Vector2(
                    Random.Range(-Constants.ScreenWidth, Constants.ScreenWidth),
                    Constants.ScreenHeight
                ),
                1 => new Vector2(
                    Random.Range(-Constants.ScreenWidth, Constants.ScreenWidth),
                    -Constants.ScreenHeight
                ),
                2 => new Vector2(
                    -Constants.ScreenWidth,
                    Random.Range(-Constants.ScreenHeight, Constants.ScreenHeight)
                ),
                3 => new Vector2(
                    Constants.ScreenWidth,
                    Random.Range(-Constants.ScreenHeight, Constants.ScreenHeight)
                ),
                _ => Vector2.zero
            };
        }
    }
}
