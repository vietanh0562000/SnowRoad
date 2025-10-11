using UnityEngine;
using Zenject;

namespace PuzzleGames
{
    using System.Collections;
    using BasePuzzle.PuzzlePackages.Core;

    public class LoadSceneGame : MonoBehaviour
    {
        [SerializeField] private string nextSceneName = "Home";
        [Inject] SignalBus _signalBus;
        private IEnumerator Start()
        {
            yield return null;

            var dataController = LevelDataController.instance;
            if (dataController.Level <= 1)
            {
                Debug.LogError("Load level 1");
                dataController.Play();
                LoadLevel(1);
            }
            else
            {
                Debug.LogError("Load Menu");
                LoadSceneManager.Instance.LoadScene(nextSceneName);
            }
        }

        private void LoadLevel(int level)
        {
            _signalBus.Fire(new LoadLevelSignal(level));
        }
    }
}