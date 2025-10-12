using System.Globalization;
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
            EventManager.OnChangeScore += HandleChangeScore;
        }

        private void OnDisable()
        {
            EventManager.OnChangeScore -= HandleChangeScore;
        }

        private void HandleChangeScore(int newScore)
        {
            scoreText.text = $"Score: {newScore.ToString(CultureInfo.InvariantCulture)}";
        }
    }
}
