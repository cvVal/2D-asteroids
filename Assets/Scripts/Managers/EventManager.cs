using System;
using UnityEngine;
using Utility;

namespace Managers
{
    public static class EventManager
    {
        // Score Events
        public static event Action<int> OnScoreChanged;
        public static event Action<int> OnPointsScored;

        // Player Events
        public static event Action OnPlayerDeath;

        // Game State Events
        public static event Action OnGameWin;
        public static event Action OnGameEnd;
        public static event Action<int> OnLivesChanged;

        // Wave Events
        public static event Action OnWaveComplete;

        // Generalized entity destruction event: position + effect key (allows reuse across entity types)
        public static event Action<Vector2, EffectKey> OnEntityDestroyed;

        // Score Event Triggers
        public static void TriggerScoreChanged(int newScore) =>
            OnScoreChanged?.Invoke(newScore);

        public static void TriggerPointsScored(int points) =>
            OnPointsScored?.Invoke(points);

        // Player Event Triggers
        public static void TriggerPlayerDeath() =>
            OnPlayerDeath?.Invoke();

        // Game State Event Triggers
        public static void TriggerGameWin() =>
            OnGameWin?.Invoke();

        public static void TriggerGameEnd() =>
            OnGameEnd?.Invoke();

        public static void TriggerLivesChanged(int remainingLives) =>
            OnLivesChanged?.Invoke(remainingLives);

        // Wave Event Triggers
        public static void TriggerWaveComplete() =>
            OnWaveComplete?.Invoke();

        // Generalized entity destruction trigger
        public static void TriggerEntityDestroyed(Vector2 position, EffectKey effectKey)
        {
            OnEntityDestroyed?.Invoke(position, effectKey);
        }
    }
}
