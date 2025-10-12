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

        [Header("Waves")]
        [SerializeField] private int initialWaveMultiplier = 1;

        [SerializeField] private int waveMultiplierIncrement = 1;

        private int _currentWave = 1;

        private InputAction _rotateAction;
        private InputAction _thrustAction;
        private InputAction _shootAction;

        private void Awake()
        {
            if (!player)
            {
                player = FindFirstObjectByType<PlayerController>();
                if (!player)
                {
                    Debug.LogError("GameManager: No PlayerController found in scene.");
                }
            }

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

            // Start first wave using multiplier
            asteroidManager?.StartNewWave(Mathf.Max(1, initialWaveMultiplier));
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
        }

        private void OnRotatePerformed(InputAction.CallbackContext ctx)
        {
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
            if (player) player.SetThrust(true);
        }

        private void OnThrustCanceled(InputAction.CallbackContext ctx)
        {
            if (player) player.SetThrust(false);
        }

        private void OnShootPerformed(InputAction.CallbackContext ctx)
        {
            if (player) player.OnShoot();
        }

        private void HandleWaveCleared()
        {
            _currentWave++;

            var nextMultiplier = Mathf.Max(1, initialWaveMultiplier + (_currentWave - 1) * waveMultiplierIncrement);

            asteroidManager.StartNewWave(nextMultiplier);
        }
    }
}
