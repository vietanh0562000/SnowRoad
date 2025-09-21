using BasePuzzle.PuzzlePackages.Core;
using UnityEngine;
using HoleBox;

public class PaintTool : MonoBehaviour, IToolMode
{
    public LESpawner spawner;
    public LEGrid    gridEditor;
    
    
    private bool               isActive = false;
    private LevelEditorManager editorManager;
    private GameObject         previewObject;

    private readonly Color validColor   = new Color(0, 1, 0, 0.6f);
    private readonly Color invalidColor   = new Color(1, 0, 0, 0.6f);
    
    private ALESpawnItem previewBlock;

    public string ToolName      => "Paint";
    public string ExternalUsage => "---";

    public void OnInit()
    {
        if (spawner == null)
        {
            spawner = FindFirstObjectByType<LESpawner>();
        }
        
        if (gridEditor == null)
        {
            gridEditor = FindFirstObjectByType<LEGrid>();
        }
    }

    private void Start()
    {
        editorManager = GetComponent<LevelEditorManager>();
        if (editorManager == null)
        {
            Debug.LogError("PaintTool: LevelEditorManager not found!");
            return;
        }
    }

    public void OnToolEnable()
    {
        isActive = true;
        CreatePreviewObject();
    }

    public void OnToolDisable()
    {
        isActive = false;
        gridEditor.CleanMatrix();
        if (previewBlock != null)
        {
            GameEvent<ALESpawnItem>.Emit(LEEvents.ON_DESTROY_PREVIEW, previewBlock );
        }
        
        DestroyPreviewObject();
    }
    
    public void OnEscapeKey() { }

    public void ForceUpdatePreview()
    {
        DestroyPreviewObject();

        previewObject = SpawnPreviewObject();
        
        // Apply preview material to all renderers
        var renderers = previewObject.GetComponentsInChildren<Renderer>(true);

        if (renderers.Length > 0)
        {
            var count = renderers.Length;
            for (int t = 0; t < count; t++)
            {
                if (renderers[t].sortingOrder < 1000)
                    renderers[t].sortingOrder = 1000;
            }
        }

        // Disable all colliders
        var colliders = previewObject.GetComponentsInChildren<Collider>();
        foreach (var collider in colliders)
        {
            collider.enabled     =  false;
        }
    }

    private GameObject SpawnPreviewObject()
    {
        var go = Instantiate(editorManager.SelectedPrefab.gameObject);
        go.name = "PreviewObject";
        
        previewBlock = go.GetComponent<ALESpawnItem>();
        previewBlock.SetUpData();
        
        GameEvent<ALESpawnItem>.Emit(LEEvents.ON_UPDATE_PREVIEW, previewBlock );

        return go;
    }

    private void CreatePreviewObject()
    {
        if (previewObject != null) return;
        if (editorManager == null || editorManager.SelectedPrefab == null)
        {
            DestroyPreviewObject();
            return;
        }

        ForceUpdatePreview();
    }

    private void DestroyPreviewObject()
    {
        if (previewObject != null)
        {
            Destroy(previewObject);
            previewObject = null;
        }
    }

    public void OnUpdate()
    {
        if (!isActive || editorManager == null) return;

        if (editorManager.SelectedPrefab == null)
        {
            DestroyPreviewObject();
            return;
        }

        if (previewObject == null)
        {
            CreatePreviewObject();
        }
        
        gridEditor.CleanMatrix();

        if (Input.GetKeyDown(KeyCode.Tab) && previewBlock.IsAbleToChangeSwap)
        {
            previewBlock.Swap();
        }
        
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Camera.main.WorldToScreenPoint(previewObject.transform.position).z;

        var hit = Camera.main.ScreenToWorldPoint(mousePos);

        Vector3 position = hit;
        var     gridPos  = LEGrid.WorldToGrid(position);

        previewObject.transform.position = position;
        previewObject.transform.rotation = Quaternion.identity;
    
        // Check if position is valid (not overlapping with other objects)
        bool isValid   = (previewBlock.NeedGridCheck && !IsOverlapping(gridPos, previewBlock.Data.size))
                         || previewBlock.InnerOverlapCheck(gridEditor, spawner, gridPos);
        bool alignment =  IsAlignment(gridPos);

        if (isValid && previewBlock.IsPlaceable)
        {
            if (gridEditor.IsInsideMatrix(gridPos, previewBlock.Data.size))
            {
                gridEditor.SetTileColor(gridPos, previewBlock.Data.size, alignment ? validColor : invalidColor);
            }
        }
        
        if (Input.GetMouseButton(0) && isValid && alignment)
        {
            if (previewBlock.IsActionItem)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    previewBlock.OnPlacedAction(gridEditor, spawner, gridPos);
                }
            }
            else
            {
                var spawnedBlock = spawner.Spawn(editorManager.SelectedPrefab);

                spawnedBlock.SetPosition(new Vector2Int(gridPos.x, gridPos.y));
                spawnedBlock.transform.position = new Vector3(gridPos.x, 0, gridPos.y);

                editorManager.SelectedObject = spawnedBlock.gameObject;

                spawnedBlock.CopyData(previewBlock.Data);
                spawnedBlock.UpdateFollowData();
                spawner.AddObject(spawnedBlock.Data);

                editorManager.SelectedObject = spawnedBlock.gameObject;
            }
        }
    }

    private bool IsAlignment(Vector2Int gridPos)
    {
        return true;
    }
    

    private bool IsOverlapping(Vector2Int position, Vector2Int size)
    {
        return !gridEditor.IsInsideMatrix(position, size) || spawner.IsOverlapping(position, size);
    }
}