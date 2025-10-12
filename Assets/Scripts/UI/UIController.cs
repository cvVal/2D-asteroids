using Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UIController : MonoBehaviour
    {
        private UIBaseState _currentState;

        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private Image[] livesImages;

        private void OnEnable()
        {
            EventManager.OnScoreChanged += HandleScoreChanged;
            EventManager.OnLivesChanged += HandleLivesChanged;
        }

        private void OnDisable()
        {
            EventManager.OnScoreChanged -= HandleScoreChanged;
            EventManager.OnLivesChanged -= HandleLivesChanged;
        }

        private void HandleScoreChanged(int newScore)
        {
            scoreText.text = $"Score: {newScore:D7}";
        }
        
        private void HandleLivesChanged(int remainingLives)
        {
            var maxLives = Mathf.Clamp(remainingLives, 0, livesImages.Length);
            for (var i = 0; i < livesImages.Length; i++)
            {
                livesImages[i].enabled = i < maxLives;
            }
        }
    }
}
