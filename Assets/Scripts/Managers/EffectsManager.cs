using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using UnityEngine;

namespace Managers
{
    /// <summary>
    /// Plays VFX prefabs for named effect keys.
    /// Subscribes to EventManager.OnEntityDestroyed and instantiates the configured prefab for the key.
    /// </summary>
    public class EffectsManager : MonoBehaviour
    {
        [Serializable]
        private struct EffectMapping
        {
            public string key;
            public GameObject prefab;
            public float duration;
        }

        [Header("Effect mappings (key -> prefab)")]
        [SerializeField] private List<EffectMapping> mappings = new();

        // dictionary for fast lookup
        private Dictionary<string, (GameObject prefab, float duration)> _map;

        private void Awake()
        {
            // Build the lookup dictionary from serialized list
            _map = new Dictionary<string, (GameObject, float)>(StringComparer.Ordinal);
            if (mappings == null) return;

            foreach (var m in mappings.Where(m =>
                         !string.IsNullOrEmpty(m.key) && m.prefab != null)
                    )
            {
                _map[m.key] = (m.prefab, m.duration);
            }
        }

        private void OnEnable()
        {
            EventManager.OnEntityDestroyed += HandleEntityDestroyed;
        }

        private void OnDisable()
        {
            EventManager.OnEntityDestroyed -= HandleEntityDestroyed;
        }

        private void HandleEntityDestroyed(Vector2 position, string key)
        {
            if (string.IsNullOrEmpty(key)) return;

            if (!_map.TryGetValue(key, out var info))
            {
                // Nothing configured for this key; ignore silently
                return;
            }

            var prefab = info.prefab;
            var duration = info.duration;
            var fx = Instantiate(prefab, position, Quaternion.identity);
            Destroy(fx, duration);
        }
    }
}
