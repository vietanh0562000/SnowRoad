using UnityEngine;

namespace HoleBox
{
    using System.Data;
    using com.ootii.Messages;
    using BasePuzzle.PuzzlePackages;
    using PuzzleGames;
    using Sirenix.OdinInspector;

    public class CameraCenteringController : MonoBehaviour
    {
        [SerializeField] private Camera mainCamera;
        [SerializeField] private float  ratio            = 1.15f;
        [SerializeField] private float  minSize          = 15;
        [SerializeField] private float  minSizeWith5Slot = 20;
        [SerializeField] private float  ExtendY          = 2;

        private float ExtendBanner     = 0f;
        private float orthographicSize = 20f;

        private Vector2Int _matrix;

        private static readonly float BaseLineAspect = 22 / 9f;

        private float MinSize => TemporaryBoardVisualize.Instance.IsMaxSlot() ? minSizeWith5Slot : minSize; 
        
        public void CenterCamera(Vector2Int matrix)
        {
            _matrix = matrix;
            ValidateCamera();
        }

        private void Awake()
        {
            MessageDispatcher.AddListener(EventID.SHOW_BANNER_ADS, OnShowBanner, true);
            MessageDispatcher.AddListener(EventID.REFRESH_CAMERA, OnShowBanner, true);
            MessageDispatcher.AddListener(EventID.HIDE_BANNER_ADS, OnHideBanner, true);
        }

        private void OnDestroy()
        {
            MessageDispatcher.RemoveListener(EventID.SHOW_BANNER_ADS, OnShowBanner, true);
            MessageDispatcher.RemoveListener(EventID.REFRESH_CAMERA, OnShowBanner, true);
            MessageDispatcher.RemoveListener(EventID.HIDE_BANNER_ADS, OnHideBanner, true);
        }

        private void OnShowBanner(IMessage rmessage) { ValidateCamera(); }
        private void OnHideBanner(IMessage rmessage) { ValidateCamera(); }

        private void ValidatePos()
        {
            // Calculate the center of the map
            int rows = _matrix.y; // Number of rows
            int cols = _matrix.x; // Number of columns
            // Assuming grid cells are 1 unit in size, calculate center position
            Vector3 centerPosition = new Vector3(cols / 2f, (rows + ExtendY + ExtendBanner) / 2f, 0); // Assuming the map lies on the X-Y plane

            // Set the camera's position
            mainCamera.transform.position = new Vector3(centerPosition.x - 0.5f, 10, centerPosition.y - 0.5f); // Set Z to -10 for 2D view
        }

        [Button]
        public void ValidateCamera()
        {
            if (mainCamera == null)
            {
                Debug.LogError("Missing references to Camera.");
                return;
            }
            
            float screenAspectRatio = 1 / mainCamera.aspect;
            if (screenAspectRatio >= 18 / 9f)
            {
                //ExtendBanner = AdsManager.IsShowBanner ? -4f : 0f;
            }
            else
            {
                //ExtendBanner = AdsManager.IsShowBanner ? -6f : 0f;
            }
            
            var rate =  1f;

            // Calculate the center of the map
            int rows = _matrix.y; // Number of rows
            int cols = _matrix.x; // Number of columns

            if (rows >= 22)
            {
                ExtendY = -1.5f;
            }

            ValidatePos();

            // Adjust the camera's orthographic size for better fit (for orthographic cameras only)
            if (mainCamera.orthographic)
            {
                // Fit rows (height) within the screen's vertical dimension
                float ratioRateMult = screenAspectRatio;
                if (screenAspectRatio > 2.15f)
                {
                    ratioRateMult = 2.15f;
                }
                else if (screenAspectRatio < 16 / 9f)
                {
                    ratioRateMult = 1.778f;
                }
                
                if (rows >= 24 && cols < 20)
                {
                    if (screenAspectRatio >= 18/9f)
                    {
                        orthographicSize = (rows - 4) / BaseLineAspect * ratioRateMult * 1.085f;
                    }
                    else
                    {
                        orthographicSize = rows - 5.5f;
                    }

                    mainCamera.orthographicSize = orthographicSize * rate;
                    return;
                }

                if (screenAspectRatio > BaseLineAspect)
                {
                    
                    orthographicSize = Mathf.Max(MinSize, cols / BaseLineAspect * ratioRateMult * ratio);
                }
                else
                {
                    // Fit rows (height) within the screen's vertical dimension
                    int   min = Mathf.Min(rows, cols);
                    float v   = 0.85f;
                    if (screenAspectRatio < 18 / 9f)
                    {
                        v = 1.05f;
                    }
                    orthographicSize = Mathf.Max(MinSize, min / BaseLineAspect * ratioRateMult * v * ratio);
                }

                mainCamera.orthographicSize = orthographicSize;
            }
        }
    }
}