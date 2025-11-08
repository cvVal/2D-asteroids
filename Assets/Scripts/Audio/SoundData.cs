using System;
using System.Collections.Generic;
using UnityEngine;
using Utility;

namespace Audio
{
    /// <summary>
    /// ScriptableObject that holds sound configuration data.
    /// Allows for easy modification and reuse across different scenes.
    /// </summary>
    [CreateAssetMenu(fileName = "SoundData", menuName = "Audio/Sound Data", order = 1)]
    public class SoundData : ScriptableObject
    {
        [Serializable]
        public class SoundEntry
        {
            [Tooltip("Unique identifier for this sound (use Utility.EffectKey enum).")]
            public EffectKey key = EffectKey.None;

            [Tooltip("Audio clips to play randomly when this sound is triggered.")]
            public AudioClip[] clips;

            [Range(0f, 1f)]
            [Tooltip("Volume multiplier for this specific sound.")]
            public float volume = 1f;

            [Range(-1f, 1f)]
            [Tooltip("Stereo pan override (-1 = left, 0 = center, 1 = right). Only used for non-positional sounds.")]
            public float stereoPan;
        }

        [Tooltip("All available sound effects mapped by key.")]
        [SerializeField] private SoundEntry[] soundEntries;

        public IReadOnlyList<SoundEntry> SoundEntries => soundEntries;

        /// <summary>
        /// Validates that all entries have valid keys and at least one clip.
        /// </summary>
        private void OnValidate()
        {
            if (soundEntries == null) return;

            var keys = new HashSet<EffectKey>();
            foreach (var entry in soundEntries)
            {
                if (entry == null) continue;

                if (entry.key == EffectKey.None)
                {
                    Debug.LogWarning($"SoundData '{name}': Found entry with EffectKey.None.", this);
                    continue;
                }

                if (!keys.Add(entry.key))
                {
                    Debug.LogWarning($"SoundData '{name}': Duplicate key '{entry.key}' found.", this);
                }

                if (entry.clips == null || entry.clips.Length == 0)
                {
                    Debug.LogWarning($"SoundData '{name}': Entry '{entry.key}' has no audio clips.", this);
                }
            }
        }
    }
}
