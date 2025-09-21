using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;

public class OutlinePass : ScriptableRenderPass
{
    public OutlineSettings settings;
    private FilteringSettings m_FilteringSettings;
    private List<ShaderTagId> m_ShaderTagIdList = new List<ShaderTagId>();

    const string k_ProfilerTag = "Outline Pass";

    public OutlinePass(OutlineSettings settings)
    {
        this.settings = settings;
        m_FilteringSettings = new FilteringSettings(RenderQueueRange.all, settings.objectsToOutlineLayer);

        // Standard URP shader tags. You might want to add more depending on the shaders your objects use.
        m_ShaderTagIdList.Add(new ShaderTagId("SRPDefaultUnlit"));
        m_ShaderTagIdList.Add(new ShaderTagId("UniversalForward"));
        m_ShaderTagIdList.Add(new ShaderTagId("UniversalForwardOnly"));
        // If you have custom shaders with specific LightMode tags, add them here.
        // e.g., m_ShaderTagIdList.Add(new ShaderTagId("MyCustomLit"));
    }

    // This method is called by the renderer before executing the pass.
    // It can be used to configure render targets and clearing, identify the camera
    // and pass other settings to the pass.
    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        // ConfigureInput can be used to tell URP that this pass requires access to certain camera textures (e.g., depth, normals)
        // For the shell method, we usually don't need special inputs unless the outline shader itself samples them.
        // If settings.useDepthTesting is true, URP handles depth testing automatically when DrawRenderers is called with appropriate render state.
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (settings.outlineMaterial == null || settings.objectsToOutlineLayer == 0)
            return;

        CommandBuffer cmd = CommandBufferPool.Get(k_ProfilerTag);
        using (new ProfilingScope(cmd, new ProfilingSampler(k_ProfilerTag)))
        {
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            // Update material properties from settings
            settings.outlineMaterial.SetColor("_OutlineColor", settings.outlineColor);
            settings.outlineMaterial.SetFloat("_OutlineThickness", settings.outlineThickness);
            settings.outlineMaterial.SetFloat("_ThicknessScale", settings.thicknessScale);

            // Set up drawing settings
            // The first ShaderTagId is for the first pass of the material (if it has multiple passes).
            // The sorting criteria determine the order in which objects are drawn.
            var drawingSettings = CreateDrawingSettings(m_ShaderTagIdList, ref renderingData, renderingData.cameraData.defaultOpaqueSortFlags);
            drawingSettings.overrideMaterial = settings.outlineMaterial; // Use the outline material to draw
            drawingSettings.overrideMaterialPassIndex = 0; // Assuming the outline shader's main pass is at index 0
            
            // Setup Render State Block for depth testing
            RenderStateBlock renderStateBlock = new RenderStateBlock();
            if (settings.useDepthTesting)
            {
                renderStateBlock.depthState = new DepthState(true, settings.depthCompareFunction);
                renderStateBlock.mask = RenderStateMask.Depth;
            }
            else
            {
                renderStateBlock.depthState = new DepthState(false, CompareFunction.Always); // No depth testing
                renderStateBlock.mask = RenderStateMask.Depth;
            }
            // To ensure the shell is culled correctly (usually front faces for extrusion)
            // the shader itself should handle Cull Front. If not, you could try setting it here, but it's less robust.
            // renderStateBlock.rasterState = new RasterState(CullMode.Front);
            // renderStateBlock.mask |= RenderStateMask.Raster;

            // Draw Renderers
            // This will draw all renderers that match the LayerMask in m_FilteringSettings and are visible to the camera.
            context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref m_FilteringSettings, ref renderStateBlock);
        }
        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    // Called when the camera has finished rendering.
    // Here you can release any temporary render targets created by this pass.
    public override void OnCameraCleanup(CommandBuffer cmd)
    {
        if (cmd == null) throw new System.ArgumentNullException("cmd");
        // No temporary RTs created in this version of the shell method pass.
    }
} 