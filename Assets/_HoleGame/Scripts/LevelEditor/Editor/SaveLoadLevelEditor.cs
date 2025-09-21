namespace HoleBox
{
    using System;
    using UnityEditor;
    using UnityEngine;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using PuzzleGames;
    using Unity.Plastic.Newtonsoft.Json;

    public partial class CreateLevelEditor : Editor
    {
        public string LevelsFolder = ForTesting.LevelsFolder;

        private List<int> levelNames         = new List<int>();
        private int       selectedLevelIndex = -1;
        private int       newLevelName       = -1;

        public void OnGUISaveLoad()
        {
            EditorGUILayout.Space(10);
            GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize  = 18,
                normal    = new GUIStyleState { textColor = Color.yellow },
                alignment = TextAnchor.MiddleCenter // Căn giữa
            };
            EditorGUILayout.LabelField($"Current Level : {newLevelName}", headerStyle);
            GUILayout.Space(20); // Add spacing in the inspector for better organization

            // Dropdown for selecting levels
            EditorGUILayout.LabelField("Level Management", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            selectedLevelIndex = EditorGUILayout.Popup(String.Empty, selectedLevelIndex, levelNames.ConvertAll(level => $"Level {level}").ToArray(), GUILayout.Height(40));
            // Button to create a new level
            if (GUILayout.Button("Load Level", GUILayout.Height(40), GUILayout.Width(100)))
            {
                LoadLevel();
            }

            if (GUILayout.Button("New", GUILayout.Height(40), GUILayout.Width(100)))
            {
                CreateNewLevel();
            }

            EditorGUILayout.EndHorizontal();
            GUILayout.Space(10);
            if (GUILayout.Button("Save Level", GUILayout.Height(40)))
            {
                ValidateLevel();
                SaveLevel();
            }

            GUILayout.Space(10);
            OnGUIValidateLevel();
        }

        private void LoadLevelNames()
        {
            if (!Directory.Exists(LevelsFolder))
            {
                Directory.CreateDirectory(LevelsFolder);
            }

            var files = Directory.GetFiles(LevelsFolder, "*.json");
            levelNames.Clear();

            foreach (var file in files)
            {
                if (int.TryParse(Path.GetFileNameWithoutExtension(file), out int levelNumber))
                {
                    levelNames.Add(levelNumber);
                }
            }

            levelNames.Sort(); // Ensure levels are always in ascending order

            if (levelNames.Count > 0)
            {
                selectedLevelIndex = 0; // Default to the first level
            }

            RePaintAll();
        }

        private void SaveLevel()
        {
            if (boardEditor == null)
            {
                Debug.LogWarning("No level data to save.");
                return;
            }

            if (newLevelName <= 0)
            {
                Debug.LogWarning("Please provide a valid level number greater than 0.");
                return;
            }

            // Serialize LevelData
            var levelData = boardEditor.GetLevelData();

            Debug.Log($"Level {newLevelName} saved successfully.");
            LoadLevelNames(); // Refresh level list

            // Serialize using Newtonsoft.Json with type information
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All // Enables polymorphic serialization
            };

            string json = JsonConvert.SerializeObject(levelData, Formatting.None, settings);

            // Save JSON to file
            string path = Path.Combine(LevelsFolder, $"{newLevelName}.json");
            File.WriteAllText(path, json);

            Debug.Log($"Level {newLevelName} saved successfully at {path}");
        }

        private void LoadLevel()
        {
            if (selectedLevelIndex < 0 || selectedLevelIndex >= levelNames.Count)
            {
                Debug.LogWarning("Please select a valid level to load.");
                return;
            }

            newLevelName = selectedLevelIndex + 1;

            int    levelName = levelNames[selectedLevelIndex];
            string path      = Path.Combine(LevelsFolder, $"{levelName}.json");

            if (!File.Exists(path))
            {
                Debug.LogError($"Level file {levelName} does not exist.");
                return;
            }

            // Read JSON from file
            string json = File.ReadAllText(path);

            // Deserialize JSON back into LevelData object
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All // Enables polymorphic deserialization
            };

            LevelData levelData = JsonConvert.DeserializeObject<LevelData>(json, settings);

            boardEditor.SetLevelData(levelData);

            Debug.Log($"Level {levelName} loaded successfully.");

            RePaintAll(); // Refresh scene view
        }

        private void CreateNewLevel()
        {
            // Determine the next available level number
            newLevelName = levelNames.Count > 0 ? levelNames.Max() + 1 : 1;

            // Reset boardEditor to default state for a new level
            boardEditor        = (CreateLevel)target;
            boardEditor.Matrix = new Vector2Int(16, 16); // Default matrix size
            boardEditor.Boxes.Clear();

            Debug.Log($"Created new level {newLevelName}.");

            RePaintAll();
        }
    }
}