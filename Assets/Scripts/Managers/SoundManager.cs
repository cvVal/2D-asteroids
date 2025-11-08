using System.Collections.Generic;
using Audio;
using UnityEngine;
using Utility;

namespace Managers
{
    /// <summary>
    /// Manages 2D sound effects for the game.
    /// Uses object pooling for efficiency and ScriptableObject-based configuration for flexibility.
    /// Follows SRP by delegating pooling to AudioSourcePool and configuration to SoundData.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class SoundManager : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private SoundData soundData;

        [Header("Settings")]
        [Range(0f, 1f)]
        [SerializeField] private float masterVolume = 1f;

        [Range(0f, 2f)]
        [Tooltip("Maximum stereo pan range based on viewport position. 1.0 = full stereo width.")]
        [SerializeField] private float stereoPanRange = 1f;

        [Tooltip("Initial size of the AudioSource pool.")]
        [SerializeField] private int poolSize = 5;

        private Dictionary<EffectKey, SoundData.SoundEntry> _soundMap;
        private AudioSourcePool _audioSourcePool;
        private AudioSource _centerAudioSource;
        private Camera _mainCamera;

        private void Awake()
        {
            InitializeAudioSources();
            BuildSoundMap();
        }

        private void OnEnable()
        {
            EventManager.OnEntityDestroyed += HandleEntityDestroyed;
        }

        private void OnDisable()
        {
            EventManager.OnEntityDestroyed -= HandleEntityDestroyed;
        }

        private void Update()
        {
            _audioSourcePool?.Update();
        }

        private void OnDestroy()
        {
            _audioSourcePool?.Dispose();
        }

        public void PlaySoundAtPosition(EffectKey key, Vector2 position)
        {
            if (!TryGetSoundEntry(key, out var entry)) return;

            var clip = GetRandomClip(entry);
            if (clip == null) return;

            var volume = CalculateVolume(entry.volume);
            var pan = CalculateStereoPan(position);

            PlayClipWithPool(clip, volume, pan);
        }

        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
        }

        private void InitializeAudioSources()
        {
            _mainCamera = Camera.main;
            _centerAudioSource = GetComponent<AudioSource>();
            _centerAudioSource.playOnAwake = false;
            _centerAudioSource.spatialBlend = 0f;

            _audioSourcePool = new AudioSourcePool(transform, poolSize);
        }

        private void BuildSoundMap()
        {
            _soundMap = new Dictionary<EffectKey, SoundData.SoundEntry>();

            if (soundData == null)
            {
                Debug.LogWarning("SoundManager: No SoundData assigned. No sounds will play.", this);
                return;
            }

            foreach (var entry in soundData.SoundEntries)
            {
                if (entry == null) continue;
                if (entry.key == EffectKey.None) continue;
                if (entry.clips == null || entry.clips.Length == 0) continue;

                _soundMap[entry.key] = entry;
            }
        }

        private void HandleEntityDestroyed(Vector2 position, EffectKey effectKey)
        {
            PlaySoundAtPosition(effectKey, position);
        }

        private bool TryGetSoundEntry(EffectKey soundKey, out SoundData.SoundEntry entry)
        {
            if (soundKey == EffectKey.None)
            {
                entry = null;
                return false;
            }

            if (_soundMap == null || !_soundMap.TryGetValue(soundKey, out entry))
            {
                entry = null;
                return false;
            }

            return true;
        }

        private static AudioClip GetRandomClip(SoundData.SoundEntry entry)
        {
            if (entry.clips.Length == 1)
                return entry.clips[0];

            var index = Random.Range(0, entry.clips.Length);
            return entry.clips[index];
        }

        private float CalculateVolume(float entryVolume)
        {
            return Mathf.Clamp01(masterVolume * entryVolume);
        }

        private float CalculateStereoPan(Vector2 worldPosition)
        {
            if (_mainCamera == null)
                return 0f;

            var viewportPos = _mainCamera.WorldToViewportPoint(worldPosition);
            // Map viewport x (0 to 1) to stereo pan (-stereoPanRange to +stereoPanRange)
            var pan = (viewportPos.x - 0.5f) * 2f * stereoPanRange;
            return Mathf.Clamp(pan, -1f, 1f);
        }

        private void PlayClipWithPool(AudioClip clip, float volume, float pan)
        {
            var source = _audioSourcePool.Get();
            source.clip = clip;
            source.volume = volume;
            source.panStereo = pan;
            source.Play();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying && soundData != null)
            {
                BuildSoundMap();
            }
        }
#endif
    }
}
