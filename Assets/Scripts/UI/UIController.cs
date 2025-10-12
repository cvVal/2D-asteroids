using Core;
using TMPro;
using UnityEngine;

namespace UI
{
    public class UIController : MonoBehaviour
    {
        private UIBaseState _currentState;

        [SerializeField] private TextMeshProUGUI scoreText;

        private void OnEnable()
        {
            EventManager.OnScoreChanged += HandleChangeScore;
        }

        private void OnDisable()
        {
            EventManager.OnScoreChanged -= HandleChangeScore;
        }

        private void HandleChangeScore(int newScore)
        {
            scoreText.text = $"Score: {newScore:D7}";
        }
    }
}
