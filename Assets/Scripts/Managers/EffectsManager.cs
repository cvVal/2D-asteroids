using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utility;

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
            public EffectKey key;
            public GameObject prefab;
            public float duration;
        }

        [Header("Effect mappings (key -> prefab)")]
        [SerializeField] private List<EffectMapping> mappings = new();

        // dictionary for fast lookup
        private Dictionary<EffectKey, (GameObject prefab, float duration)> _map;

        private void Awake()
        {
            // Build the lookup dictionary from serialized list
            _map = new Dictionary<EffectKey, (GameObject, float)>();
            if (mappings == null) return;

            foreach (var m in mappings.Where(m => m.key != EffectKey.None && m.prefab != null))
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

        private void HandleEntityDestroyed(Vector2 position, EffectKey key)
        {
            if (key == EffectKey.None) return;

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
