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

        private InputAction _rotateAction;
        private InputAction _thrustAction;
        private InputAction _shootAction;

        private void Awake()
        {
            player = FindFirstObjectByType<PlayerController>();

            if (!player)
            {
                Debug.LogError("GameManager: No PlayerController found in scene.");
            }

            playerInput = GetComponent<PlayerInput>();

            if (!playerInput)
            {
                Debug.LogError("GameManager: PlayerInput component is required on the GameManager GameObject.");
            }
        }

        private void OnEnable()
        {
            if (!playerInput || !playerInput.actions) return;

            // Expect actions named "Rotate" (Value/Axes) and "Thrust" (Button)
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
            var value = ctx.ReadValue<float>(); // -1..1 from 1D Axis composite or stick X
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
    }
}
