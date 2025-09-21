using System;
using UnityEngine;
using System.Collections.Generic;
using HoleBox;
using UnityEngine.SceneManagement;

public class LevelEditorManager : PersistentSingleton<LevelEditorManager>
{
    public enum CursorType
    {
        Default,
        Paint,
        Delete,
        Select,
    }

    [System.Serializable]
    public class CursorIcon
    {
        public CursorType type;
        public Texture2D texture;
        public Vector2 hotspot = Vector2.zero;
    }

    [SerializeField] private List<CursorIcon> cursorIcons = new List<CursorIcon>();
    private Dictionary<CursorType, CursorIcon> cursorIconMap = new Dictionary<CursorType, CursorIcon>();

    private Dictionary<string, IToolMode> tools = new();
    private IToolMode                     currentTool;
    private bool                          isEditorActive = false;
    
    // Selected object management
    private GameObject selectedObject;
    public GameObject SelectedObject 
    { 
        get => selectedObject;
        set => selectedObject = value;
    }

    // Selected prefab management
    private SelectTool selectTool;
    
    private ALESpawnItem selectedPrefab;
    public ALESpawnItem SelectedPrefab
    {
        get => selectedPrefab;
        set
        {
            selectedPrefab = value;
            if (value != null)
            {
                SwitchTool("Paint");
                GetComponent<PaintTool>().ForceUpdatePreview();
            }
        }
    }

    private void OnEnable()
    {
        
    }

    private void Start()
    {
        // Initialize tools
        tools["Paint"]  = GetComponent<PaintTool>();
        tools["Delete"] = GetComponent<DeleteTool>();
        selectTool      = GetComponent<SelectTool>();
        tools["Select"] = selectTool;
        tools["Picker"] = GetComponent<PickerTool>();
        
        // Initialize cursor icons
        foreach (var icon in cursorIcons)
        {
            cursorIconMap[icon.type] = icon;
        }
        
        foreach (KeyValuePair<string, IToolMode> tool in tools)
        {
            tool.Value.OnInit();
        }
        
        isEditorActive = true;
        
        // Subscribe to scene change events
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnDisable()
    {
        // Unsubscribe from scene change events
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Scene {scene.name} loaded");
        
        if (scene.name == "LevelEditor")
        {
            isEditorActive = true;
            foreach (KeyValuePair<string, IToolMode> tool in tools)
            {
                tool.Value.OnInit();
            }
            
            SetDefaultTool();
            
            Debug.Log("LevelEditor loaded");
        }
        else
        {
            isEditorActive = false;
            if (currentTool != null)
            {
                currentTool.OnToolDisable();
                currentTool = null;
            }
            
            SelectedObject = null; // Clear selection when editor is disabled
            SelectedPrefab = null;
            
            SetCursor(CursorType.Default);
        }
    }

    private void OnSceneUnloaded(Scene scene)
    {
        // No special handling needed when unloading scenes
    }

    private void Update()
    {
        if (!isEditorActive) return;
        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            currentTool?.OnEscapeKey();
            return;
        }
        
        // Không xử lý phím tắt nếu chuột đang hover qua UI
        if (UnityEngine.EventSystems.EventSystem.current != null && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            // Vẫn cho phép update tool hiện tại (ví dụ: selectTool context menu)
            if (currentTool != null)
            {
                currentTool.OnUpdate();
            }
            return;
        }
        if (Input.GetMouseButtonDown(1))
        {
            if (currentTool != null && !selectTool.isActive)
            {
                SwitchTool("Select");
                selectTool.ShowContextIfPossible();
                return;
            }
        }

        // Handle tool switching
        if (Input.GetKeyDown(KeyCode.W))
        {
            if (selectedPrefab != null)
            {
                SwitchTool("Paint");
            }
            else
            {
                SwitchTool("Picker");
            }
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            SwitchTool("Delete");
        }
        else if (Input.GetKeyDown(KeyCode.Q))
        {
            SwitchTool("Select");
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            SwitchTool("Picker");
        }

        // Update current tool
        if (currentTool != null)
        {
            currentTool.OnUpdate();
        }
    }

    public void SetDefaultTool()
    {
        SwitchTool("Select");
    }

    public void ChangeTool(int toolIndex)
    {
        if (toolIndex == 1)
        {
            if (selectedPrefab != null)
            {
                SwitchTool("Paint");
            }
            else
            {
                SwitchTool("Picker");
            }
        }
        else if (toolIndex == 2)
        {
            SwitchTool("Delete");
        }
        else if (toolIndex == 3)
        {
            SwitchTool("Select");
        }
        else if (toolIndex == 4)
        {
            SwitchTool("Picker");
        }
    }

    private void SwitchTool(string toolName)
    {
        if (currentTool != null)
        {
            currentTool.OnToolDisable();
        }

        if (tools.TryGetValue(toolName, out IToolMode newTool))
        {
            currentTool = newTool;
            currentTool.OnToolEnable();

            //Update cursor based on tool
            switch (toolName)
            {
                case "Paint":
                    SetCursor(CursorType.Paint);
                    break;
                case "Delete":
                    SetCursor(CursorType.Delete);
                    break;
                default:
                    SetCursor(CursorType.Select);
                    break;
            }
            
            FindFirstObjectByType<PanelMapSettings>()?.SetTool(currentTool.ToolName, currentTool.ExternalUsage);
        }
    }

    private void SetCursor(CursorType type)
    {
        if (cursorIconMap.TryGetValue(type, out CursorIcon icon))
        {
            Cursor.SetCursor(icon.texture, icon.hotspot, CursorMode.Auto);
        }
        else
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
    }
}