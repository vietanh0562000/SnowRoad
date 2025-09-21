namespace PuzzleGames
{
    using UnityEngine;

    public class TimeScaleManager : MonoBehaviour
    {
        private void Start()
        {
            GameManager.OnGamePlaying += ResetTime;
            GameManager.OnGameStop    += OnStop;
            GameManager.OnQuitGame    += ResetTime;
        }
        public void OnStop() { Time.timeScale = 0; }

        private void ResetTime() { Time.timeScale = 1; }

        private void OnDestroy()
        {
            GameManager.OnGamePlaying -= ResetTime;
            GameManager.OnGameStop    -= OnStop;
            GameManager.OnQuitGame    -= ResetTime;
        }
    }
}