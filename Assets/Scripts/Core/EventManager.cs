using System;

namespace Core
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
    }
}
