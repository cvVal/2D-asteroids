using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    public class UIGameWinState : UIBaseState
    {
        public UIGameWinState(UIController uiController) : base(uiController)
        {
        }

        public override void EnterState()
        {
            Debug.Log("UIGameOverState: Entered game over state");
        }

        public override void SelectButton()
        {
            var selected = EventSystem.current?.currentSelectedGameObject;

            if (selected == null)
            {
                Debug.LogWarning("UIGameOverState: No selected UI element. Defaulting to Restart.");
                UIControllerController.RestartGame();
                return;
            }

            var selectedName = selected.name.ToLowerInvariant();

            if (selectedName.Contains("Restart"))
            {
                Debug.Log("UIGameOverState: Restart/Retry button selected via input confirm.");
                UIControllerController.RestartGame();
                return;
            }

            if (selectedName.Contains("Exit"))
            {
                Debug.Log("UIGameOverState: Quit/Exit button selected via input confirm.");
                UIControllerController.QuitGame();
                return;
            }
        }
    }
}
