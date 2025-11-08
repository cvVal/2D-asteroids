using UnityEngine;
using UnityEngine.SceneManagement;

namespace Managers
{
    public class CursorManager : MonoBehaviour {
        
        private static CursorManager _instance;
        
        [Header("Settings:")]
        [Tooltip("The cursor to change to")]
        public Texture2D newCursorSprite;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            ChangeCursor();
            HideCursor();
        }

        private void OnEnable()
        {
            EventManager.OnGameEnd += OnGameEnd;
            EventManager.OnGameWin += OnGameEnd;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            EventManager.OnGameEnd -= OnGameEnd;
            EventManager.OnGameWin -= OnGameEnd;
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            HideCursor();
        }

        private static void OnGameEnd()
        {
            ShowCursor();
        }

        private static void HideCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private static void ShowCursor()
        {
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        }
        
        /// <summary>
        /// Description:
        /// Changes the cursor to the one set in the inspector
        /// Inputs:
        /// None
        /// Returns:
        /// void (no return)
        /// </summary>
        private void ChangeCursor()
        {
            // The location that clicking actually hits, also positions the clicker
            var hotSpot = new Vector2
            {
                // Dividing the width and height by 2 will center it
                x = newCursorSprite.width * 0.5f,
                y = newCursorSprite.height * 0.5f
            };

            Cursor.SetCursor(newCursorSprite, hotSpot, CursorMode.Auto);
        }
    }
}
