using System.Collections.Generic;
using System.Linq;
using Audio;
using UnityEngine;
using Utility;
using AudioConfiguration = ScriptableObjects.Audio.AudioConfiguration;

namespace Managers
{
    /// <summary>
    /// Centralized audio manager for music and sound effects.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class AudioManager : MonoBehaviour
    {
        #region Singleton

        public static AudioManager Instance { get; private set; }

        #endregion

        #region Inspector Fields

        [Header("Audio Configuration")]
        [SerializeField] private AudioConfiguration audioConfiguration;

        [Header("Volume Settings")]
        [Range(0f, 1f)]
        [SerializeField] private float masterVolume = 1f;

        [Range(0f, 1f)]
        [SerializeField] private float musicVolume = 0.7f;

        [Range(0f, 1f)]
        [SerializeField] private float sfxVolume = 1f;

        [Header("Pool Settings")]
        [Tooltip("Initial size of the AudioSource pool for SFX.")]
        [SerializeField] private int poolSize = 5;

        [Header("Stereo Settings")]
        [Range(0f, 2f)]
        [Tooltip("Maximum stereo pan range based on viewport position. 1.0 = full stereo width.")]
        [SerializeField] private float stereoPanRange = 1f;

        #endregion

        #region Private Fields

        private Dictionary<EffectKey, AudioConfiguration.AudioEntry> _audioMap;
        private AudioSourcePool _audioSourcePool;
        private AudioSource _musicSource;
        private Camera _mainCamera;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeSingleton();
            InitializeAudioSources();
            BuildAudioMap();
        }

        private void OnEnable()
        {
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        private void Update()
        {
            _audioSourcePool?.Update();
        }

        private void OnDestroy()
        {
            _audioSourcePool?.Dispose();

            if (Instance == this)
            {
                Instance = null;
            }
        }

        #endregion

        #region Public API - Music

        /// <summary>
        /// Plays a music track by EffectKey. Loops by default.
        /// </summary>
        /// <param name="key">The sound type to play</param>
        /// <param name="loop">Whether to loop the music</param>
        public void PlayMusic(EffectKey key, bool loop = true)
        {
            if (!TryGetAudioEntry(key, out var entry)) return;

            var clip = GetRandomClip(entry);
            if (clip == null) return;

            _musicSource.clip = clip;
            _musicSource.volume = CalculateMusicVolume(entry.volume);
            _musicSource.loop = loop;
            _musicSource.Play();
        }

        /// <summary>
        /// Stops the currently playing music.
        /// </summary>
        public void StopMusic()
        {
            _musicSource.Stop();
        }

        /// <summary>
        /// Pauses the currently playing music.
        /// </summary>
        public void PauseMusic()
        {
            _musicSource.Pause();
        }

        /// <summary>
        /// Resumes paused music.
        /// </summary>
        public void ResumeMusic()
        {
            _musicSource.UnPause();
        }

        /// <summary>
        /// Checks if music is currently playing.
        /// </summary>
        public bool IsMusicPlaying => _musicSource.isPlaying;

        #endregion

        #region Public API - Sound Effects

        /// <summary>
        /// Plays a sound effect at the center of the screen (0 pan).
        /// </summary>
        /// <param name="key">The sound type to play</param>
        public void PlaySfx(EffectKey key)
        {
            PlaySfxInternal(key, Vector2.zero, usePanning: false);
        }

        /// <summary>
        /// Plays a sound effect with stereo panning based on world position.
        /// </summary>
        /// <param name="key">The sound type to play</param>
        /// <param name="worldPosition">World position for stereo panning calculation</param>
        public void PlaySfxAtPosition(EffectKey key, Vector2 worldPosition)
        {
            PlaySfxInternal(key, worldPosition, usePanning: true);
        }

        #endregion

        #region Public API - Volume Control

        /// <summary>
        /// Sets the master volume (affects both music and SFX).
        /// </summary>
        /// <param name="volume">Volume between 0 and 1</param>
        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            UpdateMusicVolume();
        }

        /// <summary>
        /// Sets the music volume.
        /// </summary>
        /// <param name="volume">Volume between 0 and 1</param>
        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            UpdateMusicVolume();
        }

        /// <summary>
        /// Sets the SFX volume.
        /// </summary>
        /// <param name="volume">Volume between 0 and 1</param>
        public void SetSfxVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
        }

        #endregion

        #region Initialization

        private void InitializeSingleton()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning(
                    $"AudioManager: Multiple instances detected. Destroying duplicate on {gameObject.name}.");
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void InitializeAudioSources()
        {
            _mainCamera = Camera.main;

            // Music source (attached to this GameObject)
            _musicSource = GetComponent<AudioSource>();
            _musicSource.playOnAwake = false;
            _musicSource.spatialBlend = 0f; // 2D audio
            _musicSource.loop = true;

            // SFX pool
            _audioSourcePool = new AudioSourcePool(transform, poolSize);
        }

        private void BuildAudioMap()
        {
            _audioMap = new Dictionary<EffectKey, AudioConfiguration.AudioEntry>();

            if (audioConfiguration == null)
            {
                Debug.LogWarning("AudioManager: No AudioConfiguration assigned. No sounds will play.", this);
                return;
            }

            foreach (var entry in audioConfiguration.AudioEntries)
            {
                if (entry == null) continue;
                if (entry.key == EffectKey.None) continue;
                if (entry.clips == null || entry.clips.Length == 0) continue;

                _audioMap[entry.key] = entry;
            }

            Debug.Log($"AudioManager: Loaded {_audioMap.Count} audio entries.");
        }

        #endregion

        #region Event Handling

        private void SubscribeToEvents()
        {
            EventManager.OnEntityDestroyed += HandleEntityDestroyed;
            EventManager.OnGameWin += HandleGameWin;
            EventManager.OnGameEnd += HandleGameEnd;
        }

        private void UnsubscribeFromEvents()
        {
            EventManager.OnEntityDestroyed -= HandleEntityDestroyed;
            EventManager.OnGameWin -= HandleGameWin;
            EventManager.OnGameEnd -= HandleGameEnd;
        }

        private void HandleEntityDestroyed(Vector2 position, EffectKey effectKey)
        {
            if (effectKey != EffectKey.None)
            {
                PlaySfxAtPosition(effectKey, position);
            }
        }

        private void HandleGameWin()
        {
            SetMusicVolume(.3f);
            PlaySfx(EffectKey.Win);
        }

        private void HandleGameEnd()
        {
            SetMusicVolume(.3f);
            PlaySfx(EffectKey.GameOver);
        }

        #endregion

        #region Helper Methods

        private void PlaySfxInternal(EffectKey key, Vector2 worldPosition, bool usePanning)
        {
            if (!TryGetAudioEntry(key, out var entry)) return;

            var clip = GetRandomClip(entry);
            if (clip == null) return;

            var volume = CalculateSfxVolume(entry.volume);
            var pan = usePanning ? CalculateStereoPan(worldPosition) : 0f;

            PlayClipWithPool(clip, volume, pan);
        }

        private bool TryGetAudioEntry(EffectKey key, out AudioConfiguration.AudioEntry entry)
        {
            if (key == EffectKey.None)
            {
                entry = null;
                return false;
            }

            if (_audioMap == null || !_audioMap.TryGetValue(key, out entry))
            {
                Debug.LogWarning($"AudioManager: SoundType '{key}' not found in configuration.");
                entry = null;
                return false;
            }

            return true;
        }

        private static AudioClip GetRandomClip(AudioConfiguration.AudioEntry entry)
        {
            if (entry.clips.Length == 1)
                return entry.clips[0];

            var index = Random.Range(0, entry.clips.Length);
            return entry.clips[index];
        }

        private float CalculateMusicVolume(float entryVolume)
        {
            return Mathf.Clamp01(masterVolume * musicVolume * entryVolume);
        }

        private float CalculateSfxVolume(float entryVolume)
        {
            return Mathf.Clamp01(masterVolume * sfxVolume * entryVolume);
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

        private void UpdateMusicVolume()
        {
            if (_musicSource == null || _musicSource.clip == null) return;

            // Update volume based on current playing clip's entry
            foreach (var entry in _audioMap.Values
                         .Where(entry => entry.clips != null)
                         .Where(entry => entry.clips.Any(clip => clip == _musicSource.clip))
                    )
            {
                _musicSource.volume = CalculateMusicVolume(entry.volume);
                return;
            }
        }

        #endregion

        #region Editor Only

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying && audioConfiguration != null)
            {
                BuildAudioMap();
            }
        }
#endif

        #endregion
    }
}
