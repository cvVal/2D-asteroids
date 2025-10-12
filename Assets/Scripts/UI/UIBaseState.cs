namespace UI
{
    /// <summary>
    /// Base class for UI state machine states that control different UI screens and behaviors.
    /// Implements the State pattern for managing UI transitions (e.g., main menu, dialogue, game over).
    /// </summary>
    public abstract class UIBaseState
    {
        /// <summary>
        /// Gets the UI controller that manages this state.
        /// </summary>
        protected readonly UIController UIControllerController;

        /// <summary>
        /// Initializes a new instance of the UIBaseState class.
        /// </summary>
        /// <param name="uiController">The UI controller that will manage this state.</param>
        protected UIBaseState(UIController uiController)
        {
            UIControllerController = uiController;
        }

        /// <summary>
        /// Called when the UI transitions into this state.
        /// Use this method to initialize UI elements, show/hide containers, and set up input handling.
        /// </summary>
        public abstract void EnterState();

        /// <summary>
        /// Called when the user confirms a button selection in this UI state.
        /// Use this method to handle button click actions and trigger state transitions.
        /// </summary>
        public abstract void SelectButton();
    }
}
