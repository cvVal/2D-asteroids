using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI
{
    public class UIGameOverState : UIBaseState
    {
        public UIGameOverState(UIController uiController) : base(uiController)
        {
        }

        public override void EnterState()
        {
            Debug.Log("UIGameOverState: Entered game over state");
        }

        public override void SelectButton()
        {
            Debug.Log("UIGameOverState: Restarting game by reloading scene");
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
