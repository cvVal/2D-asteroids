using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Audio
{
    /// <summary>
    /// Object pool for AudioSource components to avoid constant instantiation and destruction.
    /// Optimized for 2D audio playback.
    /// Used internally by AudioManager.
    /// </summary>
    internal class AudioSourcePool
    {
        private readonly Transform _parentTransform;
        private readonly Queue<AudioSource> _availableSources = new();
        private readonly List<AudioSource> _activeSources = new();

        public AudioSourcePool(Transform parent, int initialSize = 5)
        {
            _parentTransform = parent;

            // Pre-warm the pool
            for (var i = 0; i < initialSize; i++)
            {
                CreateNewAudioSource();
            }
        }

        /// <summary>
        /// Gets an available AudioSource from the pool or creates a new one if needed.
        /// </summary>
        public AudioSource Get()
        {
            AudioSource source;

            if (_availableSources.Count > 0)
            {
                source = _availableSources.Dequeue();
            }
            else
            {
                source = CreateNewAudioSource();
            }

            source.gameObject.SetActive(true);
            _activeSources.Add(source);
            return source;
        }

        /// <summary>
        /// Returns an AudioSource to the pool for reuse.
        /// </summary>
        public void Return(AudioSource source)
        {
            if (source == null) return;

            _activeSources.Remove(source);
            source.Stop();
            source.clip = null;
            source.volume = 1f;
            source.panStereo = 0f;
            source.gameObject.SetActive(false);

            _availableSources.Enqueue(source);
        }

        /// <summary>
        /// Updates active sources and returns finished ones to the pool.
        /// Call this from Update() in the manager.
        /// </summary>
        public void Update()
        {
            for (var i = _activeSources.Count - 1; i >= 0; i--)
            {
                var source = _activeSources[i];
                if (source != null && !source.isPlaying)
                {
                    Return(source);
                }
            }
        }

        /// <summary>
        /// Cleans up all pooled AudioSources.
        /// </summary>
        public void Dispose()
        {
            foreach (var source in _availableSources.Where(source => source != null))
            {
                Object.Destroy(source.gameObject);
            }

            foreach (var source in _activeSources.Where(source => source != null))
            {
                Object.Destroy(source.gameObject);
            }

            _availableSources.Clear();
            _activeSources.Clear();
        }

        private AudioSource CreateNewAudioSource()
        {
            var go = new GameObject("Pooled_AudioSource");
            go.transform.SetParent(_parentTransform);
            go.SetActive(false);

            var source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f; // 2D audio only
            source.loop = false;

            _availableSources.Enqueue(source);
            return source;
        }
    }
}
