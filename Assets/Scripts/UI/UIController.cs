using Core;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Utility;

namespace UI
{
    public class UIController : MonoBehaviour
    {
        private UIBaseState _currentState;
        private UIGameOverState _gameOverState;

        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private Image[] livesImages;
        [SerializeField] private GameObject gameOverPanel;

        private PlayerInput _playerInput;
        private InputAction _interactAction;

        private void Awake()
        {
            _gameOverState = new UIGameOverState(this);

            _playerInput = GameObject
                .FindGameObjectWithTag(Constants.GameManagerTag)
                .GetComponent<PlayerInput>();
        }

        private void OnEnable()
        {
            EventManager.OnScoreChanged += HandleScoreChanged;
            EventManager.OnLivesChanged += HandleLivesChanged;
            EventManager.OnGameEnd += HandleGameEnd;

            if (!_playerInput || !_playerInput.actions) return;

            // Setup UI input action
            _interactAction = _playerInput.actions.FindAction(Constants.InteractActionName, throwIfNotFound: false);

            if (_interactAction != null)
            {
                _interactAction.performed += HandleInteract;
            }
            else
            {
                Debug.LogError("UIController: 'Interact' action not found in PlayerInput actions.");
            }
        }

        private void OnDisable()
        {
            EventManager.OnScoreChanged -= HandleScoreChanged;
            EventManager.OnLivesChanged -= HandleLivesChanged;
            EventManager.OnGameEnd -= HandleGameEnd;

            if (_interactAction != null)
            {
                _interactAction.performed -= HandleInteract;
            }
        }

        private void SwitchToUIActionMap()
        {
            if (!_playerInput) return;

            _playerInput.SwitchCurrentActionMap(Constants.UIActionMap);
            Debug.Log("UIController: Switched to UI action map");
        }

        private void HandleInteract(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            _currentState?.SelectButton();
        }

        private void HandleScoreChanged(int newScore)
        {
            Debug.Log("UIController: Updating score display");
            scoreText.text = $"Score: {newScore:D7}";
        }

        private void HandleLivesChanged(int remainingLives)
        {
            Debug.Log("UIController: Updating lives display");
            var maxLives = Mathf.Clamp(remainingLives, 0, livesImages.Length);
            for (var i = 0; i < livesImages.Length; i++)
            {
                livesImages[i].enabled = i < maxLives;
            }
        }

        private void HandleGameEnd()
        {
            _currentState = _gameOverState;
            _currentState.EnterState();

            gameOverPanel.SetActive(true);

            SwitchToUIActionMap();
        }
    }
}
