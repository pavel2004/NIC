using UnityEngine;

namespace GameAssets.Scripts.Utils
{
    public class GameTime : MonoBehaviour
    {

        public void PauseGame()
        {
            Time.timeScale = 0f;
        }

        public void ResumeGame()
        {
            Time.timeScale = 1f;
        }
    }
}
