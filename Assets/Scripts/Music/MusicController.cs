using UnityEngine;

namespace Music
{
    public class MusicController : MonoBehaviour
    {
        private void Awake()
        {
            if (FindObjectsByType<MusicController>(FindObjectsSortMode.None).Length > 1)
            {
                Destroy(gameObject);
            }
            else
            {
                DontDestroyOnLoad(gameObject);
            }
        }
    }
}
