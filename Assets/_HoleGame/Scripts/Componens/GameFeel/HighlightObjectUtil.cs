namespace HoleBox
{
    using UnityEngine;

    public static class HighlightObjectUtil
    {
        public static void Highlight(this Transform trans, float height = 6)
        {
            // Lấy vị trí camera chính
            Camera mainCamera = Camera.main;

            if (mainCamera == null)
            {
                Debug.LogError("Main Camera not found!");
                return;
            }

            // Lấy hướng vuông góc từ camera (hướng của trục z)
            Vector3 cameraForward = mainCamera.transform.forward;

            var ratio = height / cameraForward.normalized.y; 
            
            // Di chuyển object theo hướng Camera (đảm bảo giữ nguyên góc nhìn)
            trans.position += cameraForward.normalized * ratio;
        }
        
        public static void UnHighlight(this Transform trans)
        {
            // Lấy vị trí camera chính
            Camera mainCamera = Camera.main;

            if (mainCamera == null)
            {
                Debug.LogError("Main Camera not found!");
                return;
            }

            // Lấy hướng vuông góc từ camera (hướng của trục z)
            Vector3 cameraForward = mainCamera.transform.forward;
            
            var ratio = trans.position.y / cameraForward.normalized.y; 
            
            // Di chuyển object theo hướng Camera (đảm bảo giữ nguyên góc nhìn)
            trans.position -= cameraForward.normalized * ratio;
        }
    }
}