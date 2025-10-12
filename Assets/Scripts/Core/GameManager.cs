using System;
using Characters;
using UnityEngine;
using UnityEngine.InputSystem;
using Utility;

namespace Core
{
    public class GameManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerInput playerInput;

        [SerializeField] private PlayerController player;

        [SerializeField] private AsteroidManager asteroidManager;

        [Header("Player Spawning")]
        [Tooltip(
            "Prefab to instantiate for the player. If a PlayerController exists in scene, it will be used instead.")]
        [SerializeField] private PlayerController playerPrefab;

        [Tooltip("Optional spawn point. If null, will use world origin (0,0,0).")]
        [SerializeField] private Transform playerSpawnPoint;

        [Tooltip("Number of lives the player starts with.")]
        [SerializeField] private int startingLives = 3;

        [Tooltip("Seconds of invincibility after spawn.")]
        [SerializeField] private float spawnInvincibilityTime = 2f;

        [Header("Waves")]
        [SerializeField] private int initialWaveMultiplier = 1;

        [SerializeField] private int waveMultiplierIncrement = 1;

        [Header("Win Condition")]
        [Tooltip(
            "If > 0, reaching and clearing this wave ends the game with a win. 0 disables the limit (infinite waves).")]
        [SerializeField] private int maxLevel;

        public event Action OnGameWon;
        public event Action<int> OnLivesChanged; // passes remaining lives
        public event Action OnGameOver;

        private int _currentWave = 1;
        private bool _gameWon;
        private int _lives;
        private bool _isRespawning;
        private bool _gameOver;

        private InputAction _rotateAction;
        private InputAction _thrustAction;
        private InputAction _shootAction;

        private Vector3 _lastPlayerPosition;
        private Quaternion _lastPlayerRotation;
        private bool _hasLastPlayerPosition;

        private void Awake()
        {
            _gameWon = false;
            _gameOver = false;
            _currentWave = 1;
            _lives = Mathf.Max(0, startingLives);

            if (!playerInput)
            {
                playerInput = GetComponent<PlayerInput>();
                if (!playerInput)
                {
                    Debug.LogError("GameManager: PlayerInput component is required on the GameManager GameObject.");
                }
            }

            if (!asteroidManager || !asteroidManager.gameObject.scene.IsValid())
            {
                var sceneMgr = FindFirstObjectByType<AsteroidManager>();
                if (sceneMgr && sceneMgr.gameObject.scene.IsValid())
                {
                    asteroidManager = sceneMgr;
                }
                else if (asteroidManager)
                {
                    // A prefab asset was assigned; instantiate it into the scene
                    asteroidManager = Instantiate(asteroidManager);
                    asteroidManager.name = "AsteroidManager (Runtime)";
                }
                else
                {
                    Debug.LogError("GameManager: AsteroidManager not found in scene and none assigned.");
                }
            }

            // Ensure a player exists (use scene instance if present; otherwise instantiate prefab)
            _hasLastPlayerPosition = false;
            if (playerSpawnPoint)
            {
                _lastPlayerPosition = playerSpawnPoint.position;
                _lastPlayerRotation = playerSpawnPoint.rotation;
                _hasLastPlayerPosition = true;
            }

            if (!player)
            {
                player = FindFirstObjectByType<PlayerController>();
            }

            if (!player)
            {
                if (playerPrefab)
                {
                    if (_lives > 0)
                    {
                        // Initial spawn: no invincibility blink, don't use last position (use spawn point or origin)
                        SpawnPlayer(applyInvincibility: false, useLastPosition: false);
                    }
                    else
                    {
                        Debug.LogWarning("GameManager: Starting lives are zero; skipping player spawn.");
                        TriggerGameOver();
                    }
                }
                else
                {
                    Debug.LogError("GameManager: No PlayerController in scene and no playerPrefab assigned.");
                }
            }
            else
            {
                HookPlayerEvents(player);
                ResetPlayerForSpawn(player, applyInvincibility: false, useLastPosition: false);
            }

            // Start first wave using multiplier (only if not already at/above max)
            if (!_gameWon && (maxLevel <= 0 || _currentWave <= maxLevel))
            {
                asteroidManager?.StartNewWave(Mathf.Max(1, initialWaveMultiplier));
            }

            // notify UI of lives at start
            OnLivesChanged?.Invoke(_lives);
        }

        private void OnEnable()
        {
            if (asteroidManager)
            {
                asteroidManager.OnWaveCleared += HandleWaveCleared;
            }

            if (!playerInput || !playerInput.actions) return;

            // Expect actions named "Rotate" (Value/Axes) and "Thrust" (Button) and "Shoot" (Button)
            _rotateAction = playerInput.actions.FindAction(Constants.RotateActionName, throwIfNotFound: false);
            _thrustAction = playerInput.actions.FindAction(Constants.ThrustActionName, throwIfNotFound: false);
            _shootAction = playerInput.actions.FindAction(Constants.ShootActionName, throwIfNotFound: false);

            if (_rotateAction != null)
            {
                _rotateAction.performed += OnRotatePerformed;
                _rotateAction.canceled += OnRotateCanceled;
                _rotateAction.Enable();
            }
            else
            {
                Debug.LogError("GameManager: 'Rotate' action not found in PlayerInput actions.");
            }

            if (_thrustAction != null)
            {
                _thrustAction.started += OnThrustStarted; // press begins
                _thrustAction.canceled += OnThrustCanceled; // release ends
                _thrustAction.Enable();
            }
            else
            {
                Debug.LogError("GameManager: 'Thrust' action not found in PlayerInput actions.");
            }

            if (_shootAction != null)
            {
                _shootAction.performed += OnShootPerformed; // press
                _shootAction.Enable();
            }
            else
            {
                Debug.LogError("GameManager: 'Shoot' action not found in PlayerInput actions.");
            }
        }

        private void OnDisable()
        {
            if (asteroidManager != null)
            {
                asteroidManager.OnWaveCleared -= HandleWaveCleared;
            }

            if (_rotateAction != null)
            {
                _rotateAction.performed -= OnRotatePerformed;
                _rotateAction.canceled -= OnRotateCanceled;
                _rotateAction.Disable();
            }

            if (_thrustAction != null)
            {
                _thrustAction.started -= OnThrustStarted;
                _thrustAction.canceled -= OnThrustCanceled;
                _thrustAction.Disable();
            }

            if (_shootAction != null)
            {
                _shootAction.performed -= OnShootPerformed;
                _shootAction.Disable();
            }

            UnhookPlayerEvents(player);
        }

        private void OnRotatePerformed(InputAction.CallbackContext ctx)
        {
            if (_gameOver || _isRespawning) return;

            // -1..1 from 1D Axis composite or stick X
            var value = ctx.ReadValue<float>();
            if (player) player.SetRotation(value);
        }

        private void OnRotateCanceled(InputAction.CallbackContext ctx)
        {
            if (player) player.SetRotation(0f);
        }

        private void OnThrustStarted(InputAction.CallbackContext ctx)
        {
            if (_gameOver || _isRespawning) return;

            if (player) player.SetThrust(true);
        }

        private void OnThrustCanceled(InputAction.CallbackContext ctx)
        {
            if (player) player.SetThrust(false);
        }

        private void OnShootPerformed(InputAction.CallbackContext ctx)
        {
            if (_gameOver || _isRespawning) return;

            if (player) player.OnShoot();
        }

        private void HandleWaveCleared()
        {
            if (_gameWon) return;

            _currentWave++;

            // If maxLevel > 0, winning occurs after clearing the last configured level
            if (maxLevel > 0 && _currentWave > maxLevel)
            {
                _gameWon = true;
                Debug.Log("GameManager: All waves cleared. You win!");
                OnGameWon?.Invoke();
                return;
            }

            var nextMultiplier = Mathf.Max(1, initialWaveMultiplier + (_currentWave - 1) * waveMultiplierIncrement);

            asteroidManager.StartNewWave(nextMultiplier);
        }

        private void HandlePlayerDied()
        {
            // Capture last known transform before clearing reference
            if (player)
            {
                _lastPlayerPosition = player.transform.position;
                _lastPlayerRotation = player.transform.rotation;
                _hasLastPlayerPosition = true;
            }

            UnhookPlayerEvents(player);
            player = null;

            if (_lives > 0)
            {
                _lives--;
                OnLivesChanged?.Invoke(_lives);
                Debug.Log($"GameManager: Player died. Lives remaining: {_lives}");
                if (_lives > 0)
                {
                    // Immediate respawn WITH invincibility blink at last known position
                    SpawnPlayer(applyInvincibility: true, useLastPosition: true);
                }
                else
                {
                    TriggerGameOver();
                }
            }
            else
            {
                TriggerGameOver();
            }
        }

        private void ComputeSpawnTransform(
            bool useLastPosition,
            out Vector3 position,
            out Quaternion rotation
        )
        {
            if (useLastPosition && _hasLastPlayerPosition)
            {
                position = _lastPlayerPosition;
                rotation = _lastPlayerRotation;
                return;
            }

            position = playerSpawnPoint ? playerSpawnPoint.position : Vector3.zero;
            rotation = playerSpawnPoint ? playerSpawnPoint.rotation : Quaternion.identity;
        }

        private void SpawnPlayer(bool applyInvincibility, bool useLastPosition)
        {
            if (!playerPrefab)
            {
                Debug.LogError("GameManager: Cannot spawn player without a playerPrefab assigned.");
                return;
            }

            ComputeSpawnTransform(useLastPosition, out var position, out var rotation);

            var instance = Instantiate(playerPrefab, position, rotation);

            player = instance;
            HookPlayerEvents(player);
            ResetPlayerForSpawn(player, applyInvincibility, useLastPosition);
            Debug.Log($"GameManager: Spawned player at {position}.");
        }

        private void ResetPlayerForSpawn(
            PlayerController playerController,
            bool applyInvincibility,
            bool useLastPosition
        )
        {
            ComputeSpawnTransform(useLastPosition, out var position, out var rotation);

            playerController.transform.SetPositionAndRotation(position, rotation);

            // Reset physics and input state on the instance (does not modify prefab asset)
            var rb = playerController.GetComponent<Rigidbody2D>();
            if (rb)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }

            playerController.SetRotation(0f);
            playerController.SetThrust(false);

            if (applyInvincibility && spawnInvincibilityTime > 0f)
            {
                playerController.StartInvincibility(spawnInvincibilityTime);
            }
        }

        private void HookPlayerEvents(PlayerController playerController)
        {
            if (playerController)
            {
                playerController.OnDied += HandlePlayerDied;
            }
        }

        private void UnhookPlayerEvents(PlayerController playerController)
        {
            if (playerController)
            {
                playerController.OnDied -= HandlePlayerDied;
            }
        }

        private void TriggerGameOver()
        {
            if (_gameOver) return;

            _gameOver = true;

            Debug.Log("GameManager: Game Over");

            OnGameOver?.Invoke();

            _rotateAction?.Disable();
            _thrustAction?.Disable();
            _shootAction?.Disable();
        }
    }
}
