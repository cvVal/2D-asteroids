using UnityEngine;

namespace Core
{
    public class ScreenWrap : MonoBehaviour
    {
        private const float ScreenWidth = 12.5f;
        private const float ScreenHeight = 7f;

        private void Update()
        {
            transform.position =
                transform.position.x switch
                {
                    // If off left side, move to right side
                    < -ScreenWidth => new Vector2(ScreenWidth, transform.position.y),
                    // If off right side, move to left side
                    > ScreenWidth => new Vector2(-ScreenWidth, transform.position.y),
                    _ => transform.position
                };

            transform.position =
                transform.position.y switch
                {
                    // If off bottom side, move to top side
                    < -ScreenHeight => new Vector2(transform.position.x, ScreenHeight),
                    // If off top side, move to bottom side
                    > ScreenHeight => new Vector2(transform.position.x, -ScreenHeight),
                    _ => transform.position
                };
        }
    }
}