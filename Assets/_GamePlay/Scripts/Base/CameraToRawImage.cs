using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(RawImage), typeof(AspectRatioFitter))]
public class CameraToRawImage : MonoBehaviour
{
    public Camera renderCamera;
    [Header("Settings")]
    public bool autoUpdateOnResize = true;
    public int minTextureSize = 64;
    public int maxTextureSize = 2048;
    
    private RawImage rawImage;
    private AspectRatioFitter aspectRatioFitter;
    private RenderTexture renderTexture;
    private Vector2 lastSize;
    private bool isInitialized = false;

    void Start()
    {
        rawImage = GetComponent<RawImage>();
        aspectRatioFitter = GetComponent<AspectRatioFitter>();
        
        // Chờ layout system hoàn tất
        StartCoroutine(InitializeAfterLayout());
    }

    void Update()
    {
        if (autoUpdateOnResize && isInitialized)
        {
            CheckForSizeChange();
        }
    }

    private IEnumerator InitializeAfterLayout()
    {
        // Đợi vài frame để layout system hoàn tất
        yield return null;
        yield return null;
        yield return new WaitForEndOfFrame();
        
        CreateAndApplyRenderTexture();
        isInitialized = true;
    }

    private void CheckForSizeChange()
    {
        Vector2 currentSize = GetActualSize();
        
        if (Vector2.Distance(currentSize, lastSize) > 1f)
        {
            CreateAndApplyRenderTexture();
        }
    }

    private Vector2 GetActualSize()
    {
        RectTransform rt = rawImage.rectTransform;
        
        // Lấy kích thước thực tế trên màn hình (đã scale)
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);
        
        float width = Vector3.Distance(corners[0], corners[3]);
        float height = Vector3.Distance(corners[0], corners[1]);
        
        // Convert từ world units sang pixels
        Canvas canvas = rawImage.canvas;
        if (canvas != null)
        {
            float scaleFactor = canvas.scaleFactor;
            width *= scaleFactor;
            height *= scaleFactor;
        }
        
        return new Vector2(width, height);
    }

    void CreateAndApplyRenderTexture()
    {
        Vector2 actualSize = GetActualSize();
        int width = Mathf.Clamp((int)actualSize.x, minTextureSize, maxTextureSize);
        int height = Mathf.Clamp((int)actualSize.y, minTextureSize, maxTextureSize);
        
        // Nếu kích thước không hợp lệ thì không làm gì
        if (width <= 0 || height <= 0)
        {
            Debug.LogWarning("Invalid texture size: " + width + "x" + height);
            return;
        }
        
        // Lưu lại size để check thay đổi
        lastSize = new Vector2(width, height);
        
        // Giải phóng texture cũ
        CleanupRenderTexture();
        
        // Tạo RenderTexture mới
        renderTexture = new RenderTexture(width, height, 24);
        renderTexture.dimension = UnityEngine.Rendering.TextureDimension.Tex2D;
        renderTexture.volumeDepth = 1;
        renderTexture.antiAliasing = 2;
        renderTexture.Create();
        
        // Cập nhật Camera và RawImage
        if (renderCamera != null)
        {
            renderCamera.targetTexture = renderTexture;
            renderCamera.aspect = (float)width / height;
        }
        
        rawImage.texture = renderTexture;
        
        // Cập nhật AspectRatioFitter
        if (aspectRatioFitter != null)
        {
            aspectRatioFitter.aspectRatio = (float)width / height;
        }
        
        Debug.Log($"Created RenderTexture: {width}x{height}, aspect: {(float)width/height:F2}");
    }

    private void CleanupRenderTexture()
    {
        if (renderTexture != null)
        {
            if (renderCamera != null)
            {
                renderCamera.targetTexture = null;
            }
            
            if (renderTexture.IsCreated())
            {
                renderTexture.Release();
            }
            
            DestroyImmediate(renderTexture);
            renderTexture = null;
        }
    }

    private void OnDestroy()
    {
        CleanupRenderTexture();
    }

    private void OnDisable()
    {
        CleanupRenderTexture();
    }

    // Method để force update texture khi cần
    [ContextMenu("Force Update Texture")]
    public void ForceUpdateTexture()
    {
        CreateAndApplyRenderTexture();
    }
}