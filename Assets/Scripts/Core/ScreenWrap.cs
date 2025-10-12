using UnityEngine;
using Utility;

namespace Core
{
    public class ScreenWrap : MonoBehaviour
    {
        private void Update()
        {
            transform.position =
                transform.position.x switch
                {
                    // If off left side, move to right side
                    < -Constants.ScreenWidth => new Vector2(Constants.ScreenWidth, transform.position.y),
                    // If off right side, move to left side
                    > Constants.ScreenWidth => new Vector2(-Constants.ScreenWidth, transform.position.y),
                    _ => transform.position
                };

            transform.position =
                transform.position.y switch
                {
                    // If off bottom side, move to top side
                    < -Constants.ScreenHeight => new Vector2(transform.position.x, Constants.ScreenHeight),
                    // If off top side, move to bottom side
                    > Constants.ScreenHeight => new Vector2(transform.position.x, -Constants.ScreenHeight),
                    _ => transform.position
                };
        }
    }
}
