using System;
using System.Collections.Generic;
using UnityEngine;
using Utility;

namespace ScriptableObjects.Audio
{
    /// <summary>
    /// ScriptableObject that holds all audio configuration data (music + SFX).
    /// Allows for easy modification and reuse across different scenes.
    /// </summary>
    [CreateAssetMenu(fileName = "AudioConfiguration", menuName = "Audio/Audio Configuration", order = 0)]
    public class AudioConfiguration : ScriptableObject
    {
        [Serializable]
        public class AudioEntry
        {
            [Tooltip("Unique identifier for this audio (use Utility.SoundType enum).")]
            public EffectKey key = EffectKey.None;

            [Tooltip("Audio clips to play randomly when this audio is triggered.")]
            public AudioClip[] clips;

            [Range(0f, 1f)]
            [Tooltip("Volume multiplier for this specific audio.")]
            public float volume = 1f;

            [Tooltip("Optional description/notes for this audio entry.")]
            [TextArea(2, 3)]
            public string notes;
        }

        [Tooltip("All available audio (music and SFX) mapped by SoundType.")]
        [SerializeField] private AudioEntry[] audioEntries;

        public IReadOnlyList<AudioEntry> AudioEntries => audioEntries;

        /// <summary>
        /// Validates that all entries have valid types and at least one clip.
        /// </summary>
        private void OnValidate()
        {
            if (audioEntries == null) return;

            var types = new HashSet<EffectKey>();
            foreach (var entry in audioEntries)
            {
                if (entry == null) continue;

                if (entry.key == EffectKey.None)
                {
                    Debug.LogWarning($"AudioConfiguration '{name}': Found entry with SoundType.None.", this);
                    continue;
                }

                if (!types.Add(entry.key))
                {
                    Debug.LogWarning($"AudioConfiguration '{name}': Duplicate SoundType '{entry.key}' found.", this);
                }

                if (entry.clips == null || entry.clips.Length == 0)
                {
                    Debug.LogWarning($"AudioConfiguration '{name}': Entry '{entry.key}' has no audio clips.", this);
                }
            }
        }
    }
}
