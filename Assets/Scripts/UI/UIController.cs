using Managers;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Utility;

namespace UI
{
    public class UIController : MonoBehaviour
    {
        private UIBaseState _currentState;
        private UIGameOverState _gameOverState;
        private UIGameWinState _gameWinState;

        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private Image[] livesImages;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private GameObject gameWinPanel;

        private PlayerInput _playerInput;
        private InputAction _interactAction;

        private void Awake()
        {
            _gameOverState = new UIGameOverState(this);
            _gameWinState = new UIGameWinState(this);

            _playerInput = GameObject
                .FindGameObjectWithTag(Constants.GameManagerTag)
                .GetComponent<PlayerInput>();
        }

        private void OnEnable()
        {
            EventManager.OnScoreChanged += HandleScoreChanged;
            EventManager.OnLivesChanged += HandleLivesChanged;
            EventManager.OnGameEnd += HandleGameEnd;
            EventManager.OnGameWin += HandleGameWin;

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
            EventManager.OnGameWin -= HandleGameWin;

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
            gameOverPanel.SetActive(true);
            SwitchToUIActionMap();

            _currentState = _gameOverState;
            _currentState.EnterState();
        }

        private void HandleGameWin()
        {
            gameWinPanel.SetActive(true);
            SwitchToUIActionMap();

            _currentState = _gameWinState;
            _currentState.EnterState();
        }

        public void RestartGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
