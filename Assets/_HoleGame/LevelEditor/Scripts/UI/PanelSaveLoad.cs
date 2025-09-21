using System;
using System.IO;
using BasePuzzle.PuzzlePackages.Core;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HoleBox
{
    using PuzzleGames;

    public class PanelSaveLoad : MonoBehaviour
    {
        private const string _SAVE_PATH = "_HoleGame/Resources";

        private enum ButtonType
        {
            Load,
            Save,
            Play,
            Verify
        }

        [Header("Data")] [SerializeField] private LESpawner _spawner;


        [Header("UI References")] [SerializeField]
        private Button _btnLoad;

        [SerializeField] private Button           _btnSave;
        [SerializeField] private Button           _btnPlayEdit;
        [SerializeField] private Button           _btnVerify;
        [SerializeField] private VerifyPopup      _verifyPopup;
        [SerializeField] private TMP_Dropdown     _ddDifficulty;
        [SerializeField] private PanelMapSettings _panelMapSettings;
        [SerializeField] private PanelMapConfig   _panelMapConfig;
        [SerializeField] private TMP_Text         _txtLevel;

        private JsonSerializerSettings _serializerSettings;
        private string                 _levelName = string.Empty;

        private void Awake()
        {
#if UNITY_EDITOR
            GameViewUtils.SetFullScreen();
#endif
            GameEvent<PanelMapSettings>.Register(LEEvents.ON_SETUP_MAP_LEVEL_EDITOR, OnSetupMapLevelEditor, this);
            GameEvent<PanelMapSettings>.Register(LEEvents.CLICK_BTN_APPLY_MAP_SETTINGS, ClickBtnApplyMapSettings, this);

            Initialize();
        }

        private void Initialize()
        {
            _serializerSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                TypeNameHandling  = TypeNameHandling.All // Enables polymorphic deserialization
            };

            SetActiveButtons(true, ButtonType.Load);
            SetActiveButtons(false, ButtonType.Save, ButtonType.Play, ButtonType.Verify);

            var difficulties = Enum.GetValues(typeof(LevelDifficulty));

            for (int i = 0; i < difficulties.Length; i++)
            {
                var option = new TMP_Dropdown.OptionData(difficulties.GetValue(i).ToString());
                _ddDifficulty.options.Add(option);
            }
        }

        private void Start()
        {
            var levelJson = TempDataHandler.Get<string>(TempDataKeys.CURRENT_LEVEL_JSON_DATA);
            if (!string.IsNullOrEmpty(levelJson))
                LoadLevel(levelJson);
        }

        private void OnDestroy()
        {
            GameEvent<PanelMapSettings>.Unregister(LEEvents.ON_SETUP_MAP_LEVEL_EDITOR, OnSetupMapLevelEditor, this);
            GameEvent<PanelMapSettings>.Unregister(LEEvents.CLICK_BTN_APPLY_MAP_SETTINGS, ClickBtnApplyMapSettings, this);
        }

        private void ClickBtnApplyMapSettings(PanelMapSettings obj)
        {
            _txtLevel.gameObject.SetActive(false);
            _levelName = string.Empty;
            TempDataHandler.Set(TempDataKeys.CURRENT_LEVEL_FROM_LEVEL_EDITOR, _levelName);
        }

        private void OnSetupMapLevelEditor(PanelMapSettings obj) { SetActiveButtons(true, ButtonType.Save, ButtonType.Play, ButtonType.Verify); }

        private void SetActiveButtons(bool active, params ButtonType[] types)
        {
            foreach (var type in types)
            {
                SetActiveButton(active, type);
            }
        }

        private void SetActiveButton(bool active, ButtonType type)
        {
            switch (type)
            {
                case ButtonType.Load:
                    _btnLoad.gameObject.SetActive(active);
                    return;
                case ButtonType.Save:
                    _btnSave.gameObject.SetActive(active);
                    return;
                case ButtonType.Play:
                    _btnPlayEdit.gameObject.SetActive(active);
                    return;
                case ButtonType.Verify:
                    _btnVerify.gameObject.SetActive(active);
                    return;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public void ClickBtnSave() { SaveFile(GetLevelJson()); }

        public void ClickBtnLoad()
        {
            string folderPath = Path.Combine(Application.dataPath, _SAVE_PATH);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var openPath = FileBrowser.OpenFile("Open", folderPath, "txt");
            if (File.Exists(openPath))
            {
                string jsonString = File.ReadAllText(openPath);
                _levelName = Path.GetFileNameWithoutExtension(openPath);

                TempDataHandler.Set(TempDataKeys.CURRENT_LEVEL_FROM_LEVEL_EDITOR, _levelName);
                LoadLevel(jsonString);
            }
            else
            {
                Debug.LogError("File not found: " + openPath);
            }
        }

        private void LoadLevel(string levelJson)
        {
            _levelName = TempDataHandler.Get<string>(TempDataKeys.CURRENT_LEVEL_FROM_LEVEL_EDITOR);
            bool fromEditor = string.IsNullOrEmpty(_levelName);
            if (!fromEditor)
            {
                _txtLevel.gameObject.SetActive(true);
                _txtLevel.text = "Level: " + _levelName;
            }
            else
            {
                _txtLevel.gameObject.SetActive(false);
                _txtLevel.text = string.Empty;
            }

            TxtLevelData data = JsonConvert.DeserializeObject<TxtLevelData>(levelJson);
            _ddDifficulty.SetValueWithoutNotify((int)data.difficulty);
            Debug.LogError($"difficult: {data.difficulty}, int: {(int)data.difficulty}");

            var decompressMapData = JsonCompressing.Decompressing(data.mapData);
            var mapJson           = JsonConvert.DeserializeObject<LevelData>(decompressMapData, _serializerSettings);

            // load old map
            _panelMapSettings.OnLoadOldLevel(mapJson);
            _spawner.OnLoadOldLevel(mapJson);
            _panelMapConfig.OnLoadOld();
        }

        public void ClickBtnPlay()
        {
            Destroy(PoolHolder.PoolTransform.gameObject);

            TempDataHandler.Set(TempDataKeys.CURRENT_LEVEL_JSON_DATA, GetLevelJson());
            TempDataHandler.Set(TempDataKeys.OPEN_LEVEL_FROM_EDITOR, true);
            TempDataHandler.Set(TempDataKeys.CURRENT_LEVEL_FROM_LEVEL_EDITOR, _levelName);

#if UNITY_EDITOR
            GameViewUtils.SetGameViewSize(1080, 1920, UnityEditor.GameViewSizeGroupType.Android);
            LoadSceneManager.Instance.LoadScene("GamePlay");
#endif
        }

        public void ClickBtnVerify()
        {
            var mapData = _spawner.GetLevelData();

            var validate = CreateLevelValidator.ValidateLevel(mapData.Boxes, mapData.ContainerQueues, out var validationResults);

            _verifyPopup.ShowPopup(validate, validationResults);
        }

        public void ClickBtnVerifyAll()
        {
            var  validate    = "";
            bool isValidated = true;

            for (int i = 1; i <= LevelDataController.instance.GetMaxLevel(); i++)
            {
                var mapData = GetLevelData(i);

                if (!CreateLevelValidator.ValidateLevel(mapData.Boxes, mapData.ContainerQueues, out var validationResults))
                {
                    validate    += $"Level {i} không hợp lệ!! \n";
                    isValidated =  false;
                }
            }

            if (isValidated)
            {
                _verifyPopup.ShowPopup("Tất cả level đều hợp lệ!!");
            }
            else
            {
                _verifyPopup.ShowPopup(validate);
            }
        }

        private LevelData GetLevelData(int level)
        {
            string folderPath = Path.Combine(Application.dataPath, _SAVE_PATH);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string       jsonString = File.ReadAllText($"{folderPath}/{level}.txt");
            TxtLevelData data       = JsonConvert.DeserializeObject<TxtLevelData>(jsonString);
            _ddDifficulty.SetValueWithoutNotify((int)data.difficulty);
            Debug.LogError($"difficult: {data.difficulty}, int: {(int)data.difficulty}");

            var decompressMapData = JsonCompressing.Decompressing(data.mapData);
            var mapJson           = JsonConvert.DeserializeObject<LevelData>(decompressMapData, _serializerSettings);

            return mapJson;
        }

        private string GetLevelJson()
        {
            var mapData = _spawner.GetLevelData();
            var mapJson = JsonConvert.SerializeObject(mapData, _serializerSettings);

            var difficulty = (LevelDifficulty)_ddDifficulty.value;
            var levelData  = new TxtLevelData(difficulty, 0, JsonCompressing.Compressing(mapJson));
            var levelJson  = JsonConvert.SerializeObject(levelData);
            return levelJson;
        }

        private void SaveFile(string textToSave)
        {
            string folderPath = Path.Combine(Application.dataPath, _SAVE_PATH);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var savePath = FileBrowser.SaveFile("Save", folderPath, _levelName == string.Empty ? "level" : _levelName, "txt");

            try
            {
                File.WriteAllText(savePath, textToSave);
                Debug.LogError("Saving Ok: " + savePath);

#if UNITY_EDITOR
                UnityEditor.AssetDatabase.Refresh();
#endif
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error saving file: " + e.Message);
            }
        }
    }
}