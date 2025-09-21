using UnityEngine;

namespace HoleBox
{
    using System.Collections.Generic;
    using System.IO;
    using TMPro;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;

    public class ForTesting : MonoBehaviour
    {
        public static string LevelsFolder = "Assets/_HoleGame/Levels"; // Path to save levels

        public TMP_Dropdown    levelDropdown; // Gán ngoài Inspector
        public Button          loadLevelButton; // Gán ngoài Inspector
        public HoleLevelLoader levelLoader; // Gán ngoài Inspector

        private List<int> levelNumbers = new List<int>();

        private void Start()
        {
            PopulateLevels();
            loadLevelButton.onClick.AddListener(OnLoadButtonClicked);
        }


        public void ResetScene() { SceneManager.LoadScene(SceneManager.GetActiveScene().name); }

        void PopulateLevels()
        {
            levelDropdown.ClearOptions();
            levelNumbers.Clear();

            string levelsFolder = LevelsFolder;
            if (!Directory.Exists(levelsFolder))
                Directory.CreateDirectory(levelsFolder);

            var          files           = Directory.GetFiles(levelsFolder, "*.json");
            List<string> dropdownOptions = new List<string>();
            foreach (var file in files)
            {
                if (int.TryParse(Path.GetFileNameWithoutExtension(file), out int num))
                {
                    levelNumbers.Add(num);
                    dropdownOptions.Add($"Level {num}");
                }
            }

            levelNumbers.Sort();
            dropdownOptions.Sort();

            levelDropdown.AddOptions(dropdownOptions);

            if (levelNumbers.Count == 0)
                levelDropdown.interactable = false;
            else
                levelDropdown.interactable = true;
        }

        void OnLoadButtonClicked()
        {
            int index = levelDropdown.value;
            if (index >= 0 && index < levelNumbers.Count)
            {
               LoadLevel(levelNumbers[index]);
            }
            else
            {
                Debug.LogWarning("Please select a valid level.");
            }
        }

        private void LoadLevel(int levelNumber)
        {
            string path = Path.Combine(LevelsFolder, $"{levelNumber}.json");

            if (!File.Exists(path))
            {
                Debug.LogError($"Level file {path} does not exist.");
                return;
            }

            string json = File.ReadAllText(path);
            
            levelLoader.LoadLevel(json);
        }
    }
}