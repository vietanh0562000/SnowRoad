using System;
using UnityEngine;
using System.Collections.Generic;
using BasePuzzle;
using BasePuzzle.PuzzlePackages.Core;
using HoleBox;
using UnityEngine.EventSystems;

public class SelectTool : MonoBehaviour, IToolMode
{
    public LEGrid gridEditor;
    public LESpawner leSpawner;
    
    [NonSerialized] public bool isActive      = false;
    [NonSerialized] public bool isShowContext = false;
    
    private LevelEditorManager editorManager;
    private Material highlightMaterial;
    
    private readonly Color highlightColor = new Color(0, 0, 1, 0.3f);
    private readonly Color validColor     = new Color(0, 1, 0, 0.6f);
    private readonly Color invalidColor   = new Color(1, 0, 0, 0.6f);
    
    private readonly Dictionary<Renderer, Material[]> originalMaterials = new();
    
    private bool isDragging = false;
    
    private ALESpawnItem selectedBlock;
    private Vector3      previousPosition;

    public string ToolName      => "Select";
    public string ExternalUsage => "---";
    
    public void OnInit()
    {
        if (leSpawner == null)
        {
            leSpawner = FindFirstObjectByType<LESpawner>();
        }
        
        if (gridEditor == null)
        {
            gridEditor = FindFirstObjectByType<LEGrid>();
        }
    }

    private void Start()
    {
        editorManager = GetComponent<LevelEditorManager>();
    }

    public void OnToolEnable()
    {
        isActive = true;
        if (editorManager != null) editorManager.SelectedObject = null;
    }

    public void OnToolDisable()
    {
        if (isDragging && selectedBlock != null) ReturnPrevious();
        
        isActive = false;
        selectedBlock?.Highlight(false);
        
        selectedBlock = null;
        gridEditor.CleanMatrix();
        
        RemoveHighlight();
        
        isDragging = false;
        
        CloseContextMenu();
    }

    public void OnEscapeKey()
    {
        CloseContextMenu();
    }

    private void RemoveHighlight()
    {
        if (editorManager.SelectedObject != null)
        {
            RestoreOriginalMaterials();
        }

        if (selectedBlock != null)
        {
            selectedBlock.Highlight(false);
        }
    }

    private void StoreOriginalMaterials()
    {
        if (editorManager.SelectedObject != null)
        {
            var renderers = editorManager.SelectedObject.GetComponentsInChildren<Renderer>(true);
            foreach (var renderer in renderers)
            {
                originalMaterials[renderer] = renderer.sharedMaterials;
            }
        }
    }

    public void ForceClearMaterialDict()
    {
        originalMaterials.Clear();
    }

    private void RestoreOriginalMaterials()
    {
        foreach (var kvp in originalMaterials)
        {
            if (kvp.Key.sortingOrder <= 1000)
                kvp.Key.sortingOrder = 1;
            
            if (kvp.Key != null)
            {
                kvp.Key.sharedMaterials = kvp.Value;
            }
        }
        originalMaterials.Clear();
    }

    private void ApplyHighlight()
    {
        if (editorManager.SelectedObject != null)
        {
            var renderers = editorManager.SelectedObject.GetComponentsInChildren<Renderer>(true);
            
            if (renderers.Length > 0)
            {
                var count = renderers.Length;
                for (int t = 0; t < count; t++)
                {
                    if (renderers[t].sortingOrder < 100)
                        renderers[t].sortingOrder = 1000 - t;
                }
            }
        }
    }

    private void CloseContextMenu()
    {
        isShowContext = false;
    }

    private void ShowContextMenu(GameObject target, Vector2 position)
    {
        CloseContextMenu();
    }

    public void OnUpdate()
    {
        if (!isActive) return;

        if (Input.GetKeyDown(KeyCode.Delete))
        {
            if (editorManager.SelectedObject != null)
            {
                RemoveHighlight();
                CloseContextMenu();
                
                GetComponent<DeleteTool>().DeleteObject(editorManager.SelectedObject);
                
                return;
            }
        }
        
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // Handle right-click for context menu
        if (Input.GetMouseButtonDown(1) && !UIServices.IsPointerOverUIObject() && !EventSystem.current.IsPointerOverGameObject())
        {
            if (Physics.Raycast(ray, out hit))
            {
                GameObject hitObject = TryGetParent(hit.collider);
                if (hitObject != editorManager.SelectedObject)
                {
                    RemoveHighlight();
                    editorManager.SelectedObject = hitObject;
                    StoreOriginalMaterials();
                    ApplyHighlight();
                }
                
                selectedBlock = hitObject.GetComponent<ALESpawnItem>();
                if (selectedBlock != null)
                {
                    selectedBlock.Highlight(true);
                    GameEvent<ALESpawnItem>.Emit(LEEvents.ON_UPDATE_PREVIEW, selectedBlock );
                }

                ShowContextMenu(hitObject, Input.mousePosition);
            }

            isDragging = false;
        }

        // Handle left-click for selection and movement
        if (Input.GetMouseButtonDown(0) && !UIServices.IsPointerOverUIObject())
        {
            if (Physics.Raycast(ray, out hit))
            {
                GameObject hitObject = TryGetParent(hit.collider);
                if (hitObject != editorManager.SelectedObject)
                {
                    RemoveHighlight();
                    editorManager.SelectedObject = hitObject;
                    StoreOriginalMaterials();
                    ApplyHighlight();
                }
                
                selectedBlock = hitObject.GetComponent<ALESpawnItem>();
                if (selectedBlock != null)
                {
                    selectedBlock.Highlight(true);
                    GameEvent<ALESpawnItem>.Emit(LEEvents.ON_UPDATE_PREVIEW, selectedBlock );
                }
                
                previousPosition = hitObject.transform.position;
                isDragging = true;
            }
            
            CloseContextMenu();
        }
        else if (Input.GetMouseButton(0) && isDragging && editorManager.SelectedObject != null)
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = Camera.main.WorldToScreenPoint(editorManager.SelectedObject.transform.position).z;

            var  position = Camera.main.ScreenToWorldPoint(mousePos);
           
            
            if (selectedBlock != null)
            { 
                gridEditor.CleanMatrix();
                editorManager.SelectedObject.transform.position = position;
                
                var  gridPos     = LEGrid.WorldToGrid(position);
                bool isValid     = !IsOverlapping(selectedBlock, gridPos, selectedBlock.Data.size);
                bool isAlignment = IsAlignment(gridPos);
                
                if (isValid)
                {
                    gridEditor.SetTileColor(gridPos, selectedBlock.Data.size, isAlignment ? validColor: invalidColor);
                }
            }
        }
        else if (Input.GetMouseButtonUp(0) && isDragging && editorManager.SelectedObject != null)
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = Camera.main.WorldToScreenPoint(editorManager.SelectedObject.transform.position).z;

            var position = Camera.main.ScreenToWorldPoint(mousePos);
            var gridPos  = LEGrid.WorldToGrid(position);

            gridEditor.CleanMatrix();
            if (!IsAlignment(gridPos))
            {
                ReturnPrevious();
                
                isDragging = false;
                return;
            }
            
            selectedBlock = editorManager.SelectedObject.GetComponent<ALESpawnItem>();
            if (selectedBlock != null)
            {
                if (IsOverlapping(selectedBlock, gridPos, selectedBlock.Data.size))
                {
                    ReturnPrevious();
                }
                else
                {
                    selectedBlock.SetPosition(new Vector2Int(gridPos.x, gridPos.y));
                    var pos = new Vector3(gridPos.x, 0, gridPos.y);
                    editorManager.SelectedObject.transform.position = pos;
                    previousPosition                                = pos;
                }
            }
            
            isDragging = false;
        }
    }
    
    private bool IsAlignment(Vector2Int gridPos)
    {
        return true;
    }
    
    private bool IsOverlapping(ALESpawnItem item, Vector2Int position, Vector2Int size)
    {
        return !gridEditor.IsInsideMatrix(position, size) || leSpawner.IsOverlappingWithoutBox(item.Data, position, size);
    }

    public void ShowContextIfPossible()
    {
        if (!UIServices.IsPointerOverUIObject() && !EventSystem.current.IsPointerOverGameObject())
        {
            Ray        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit))
            {
                GameObject hitObject = TryGetParent(hit.collider);
                if (hitObject != editorManager.SelectedObject)
                {
                    RemoveHighlight();
                    editorManager.SelectedObject = hitObject;
                    StoreOriginalMaterials();
                    ApplyHighlight();
                }
                
                selectedBlock = hitObject.GetComponent<ALESpawnItem>();
                
                ShowContextMenu(hitObject, Input.mousePosition);
            }
        }
    }
    
    
    private void ReturnPrevious()
    {
        editorManager.SelectedObject.transform.position = previousPosition;
    }
    
    private GameObject TryGetParent(Collider col)
    {
        var iBlock = col.gameObject.GetComponentInParent<ALESpawnItem>();
        if (iBlock != null)
        {
            return iBlock.gameObject;
        }
        
        return col.gameObject;
    }
} 