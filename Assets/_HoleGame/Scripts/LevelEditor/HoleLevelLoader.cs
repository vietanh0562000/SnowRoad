namespace HoleBox
{
    using System;
    using System.IO;
    using PuzzleGames;
    using Newtonsoft.Json;
    using UnityEngine;

    public class HoleLevelLoader : MonoBehaviour
    {
        [SerializeField] private TemporaryBoardVisualize   boardVisualize;
        [SerializeField] private CameraCenteringController cameraCentering;
        [SerializeField] private BarrierCheck              barrierCheck;

        private void OnValidate() { boardVisualize = GetComponent<TemporaryBoardVisualize>(); }

        public void LoadLevel(string levelJson)
        {
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };

            LevelData levelData = JsonConvert.DeserializeObject<LevelData>(levelJson, settings);

            Debug.Log($"Level {levelJson} loaded successfully.");

            // Initialize the boardVisualize with extracted data
            boardVisualize.SetLevel(levelData);
            cameraCentering.CenterCamera(boardVisualize.Matrix);
            barrierCheck.GetBarrierGroup();
        }
    }
}