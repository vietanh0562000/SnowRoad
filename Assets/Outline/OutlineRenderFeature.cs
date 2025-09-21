using UnityEngine;
using UnityEngine.Rendering.Universal;

public class OutlineRenderFeature : ScriptableRendererFeature
{
    public OutlineSettings settings = new OutlineSettings();
    private OutlinePass m_OutlinePass;

    public override void Create()
    {
        if (settings == null)
        {
            settings = new OutlineSettings();
        }

        m_OutlinePass = new OutlinePass(settings);
        m_OutlinePass.renderPassEvent = settings.renderPassEvent;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.outlineMaterial == null || settings.objectsToOutlineLayer == 0)
        {
            Debug.LogWarningFormat("OutlineRenderFeature: Missing Outline Material or ObjectsToOutlineLayer is not set. Outline pass will not be added.");
            return;
        }

        // Update pass settings if they changed in the editor
        // This is a simple way; for more complex scenarios, you might need a more robust update mechanism.
        if (m_OutlinePass.settings != settings)
        {
            m_OutlinePass.settings = settings;
            m_OutlinePass.renderPassEvent = settings.renderPassEvent; // Ensure renderPassEvent is also updated
        }
        
        // Pass the current camera's color target to the pass
        // For URP 12+, RTHandles are preferred for camera targets
        // However, for simplicity and broader compatibility within URP versions, 
        // direct access via renderer.cameraColorTarget might be used if appropriate, 
        // or the pass can access it from renderingData.
        // For this example, the pass will get it from renderingData.

        renderer.EnqueuePass(m_OutlinePass);
    }

    protected override void Dispose(bool disposing)
    {
        // CoreUtils.Destroy(m_OutlinePass.OutlineMaterial); // If the material was created by the pass
        // m_OutlinePass = null; // Or handle disposal if m_OutlinePass implements IDisposable
        base.Dispose(disposing);
    }

    // It's good practice to re-create the pass if settings are changed in the editor while the application is not running.
    // However, a full OnValidate might be too aggressive if settings are frequently tweaked.
    // For robust editor updates, consider more specific checks or a custom editor.
    void OnValidate()
    {
        if (m_OutlinePass != null)
        {
            m_OutlinePass.settings = settings; 
            m_OutlinePass.renderPassEvent = settings.renderPassEvent;
        }
    }
} 