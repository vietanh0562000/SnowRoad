using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[System.Serializable]
public class OutlineSettings
{
    public Color outlineColor = Color.black;
    [Range(0.001f, 0.1f)]
    public float outlineThickness = 0.01f;
    public LayerMask objectsToOutlineLayer = -1; // Default to everything
    public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
    public Material outlineMaterial;

    [Header("Depth Testing")]
    public bool useDepthTesting = true;
    public CompareFunction depthCompareFunction = CompareFunction.LessEqual;

    // Optional for Shell/Geometry method
    [Header("Shell Method Specific")]
    [Range(0.1f, 5f)]
    public float thicknessScale = 1f;
} 