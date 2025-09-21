using UnityEngine;
using System.Collections.Generic;
using BasePuzzle.PuzzlePackages.Core;
using HoleBox;
using UnityEngine.UI;

public class DeleteTool : MonoBehaviour, IToolMode
{
    public LEGrid    gridEditor;
    public LESpawner spawner;

    private bool               isActive = false;
    private LevelEditorManager editorManager;
    private GameObject         lastHoveredObject;
    private Material           highlightMaterial;
    
    private readonly Color highlightColor = new Color(1, 0, 0, 0.3f);
    
    private readonly Dictionary<GameObject, SpriteRenderer> multiSelectedObjects = new();
    
    private bool isDragging = false;
    private Vector3 dragStartPos, dragEndPos;
    
    public string ToolName      => "Delete";
    public string ExternalUsage => @"Paint - Select. [Delete] - confirm. \n[Esc] - reset.";
    
    public void OnInit()
    {
        if (gridEditor == null)
        {
            gridEditor = FindFirstObjectByType<LEGrid>();
        }
        
        if (spawner == null)
        {
            spawner = FindFirstObjectByType<LESpawner>();
        }
    }

    private void Start()
    {
        editorManager = GetComponent<LevelEditorManager>();
    }

    public void OnToolEnable()
    {
        isActive = true;
    }

    public void OnToolDisable()
    {
        isActive = false;
        gridEditor.CleanMatrix();
        RemoveMultiHighlight();
        multiSelectedObjects.Clear();
    }
    
    public void OnEscapeKey()
    {
        RemoveMultiHighlight();
        multiSelectedObjects.Clear();
    }

    public void OnUpdate()
    {
        if (!isActive) return;

        // Raycast để phát hiện object dưới con trỏ chuột
        if (Input.GetMouseButton(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                var hoveredObj = TryGetParent(hit.collider);
                if (hoveredObj != null && hoveredObj.activeInHierarchy)
                {
                    if (!multiSelectedObjects.ContainsKey(hoveredObj))
                    {
                        var selected = hoveredObj.GetComponent<ALESpawnItem>();
                        selected.Highlight(true);
                        var r        = selected.HLGo.GetComponent<SpriteRenderer>();
                        r.color = highlightColor;
                        multiSelectedObjects[hoveredObj] = r;
                    }
                }
            }
        }

        // Xóa khi nhấn phím Delete
        if (Input.GetKeyDown(KeyCode.Delete) && multiSelectedObjects.Count > 0)
        {
            var keys = multiSelectedObjects.Keys;
            foreach (var obj in keys)
            {
                DeleteObject(obj.gameObject);
            }
            RemoveMultiHighlight();
            multiSelectedObjects.Clear();
        }
    }

    private void RemoveMultiHighlight()
    {
        foreach (var kvp in multiSelectedObjects)
        {
            var obj = kvp.Key;
            if (obj == null) return;
            
            var renderer = kvp.Value;
            renderer.color = Color.blue;
            
            var selected = obj.GetComponent<ALESpawnItem>();
            selected.Highlight(false);
        }
    }

    public void DeleteObject(GameObject go)
    {
        var spawnItem = go.GetComponent<ALESpawnItem>();
        if (spawnItem != null)
        {
            spawnItem.Highlight(false);
            spawner.DeleteObject(spawnItem.Data);
            GameEvent<ALESpawnItem>.Emit(LEEvents.ON_DESTROY_PREVIEW, spawnItem );
            spawnItem.SendToPool();
        }
    }

    private GameObject TryGetParent(Collider col)
    {
        var iBlock = col.gameObject.GetComponentInParent<ALESpawnItem>();
        if (iBlock != null)
        {
            return iBlock.gameObject;
        }
        
        return null;
    }
}