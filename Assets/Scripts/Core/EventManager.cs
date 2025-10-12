using UnityEngine.Events;

namespace Core
{
    public static class EventManager
    {
        public static event UnityAction<int> OnChangeScore;
        
        public static void RaiseChangeScore(int newScore) =>
            OnChangeScore?.Invoke(newScore);
    }
}
