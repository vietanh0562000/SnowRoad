using System.Collections.Generic;
using UnityEngine;

namespace BoosterBtn
{
    using com.ootii.Messages;
    using DG.Tweening;
    using PuzzleGames;
    using HoleBox;
    using UnityEngine.EventSystems;

    public class UFOLogic : MonoBehaviour
    {
        private TemporaryBoardVisualize boardVisualizer => TemporaryBoardVisualize.Instance;

        // Danh sách các box thuộc nhóm
        private List<BoxData> groupBoxes = new List<BoxData>();

        void Update()
        {
            if (boardVisualizer != null && boardVisualizer.UseUfo)
            {
                if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject()) // Nhấp chuột trái
                {
                    // Lấy tọa độ chuột
                    Vector2 screenPosition = Input.mousePosition;

                    // Chọn box dựa trên tọa độ màn hình
                    SelectPointFromScreen(screenPosition);
                }
            }
        }

        public void SelectPointFromScreen(Vector2 screenPosition)
        {
            // Sử dụng raycast để tìm vật thể tại vị trí màn hình
            Ray        ray = Camera.main.ScreenPointToRay(screenPosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                var check = hit.transform.GetComponentInParent<StickManBox>();

                if (check != null)
                {
                    SelectPoint(check.Data);
                }
            }
        }

        // Xử lý khi chọn một điểm (hoặc hover) trên ma trận
        public void SelectPoint(BoxData startingBox)
        {
            // Lấy BoxData tại vị trí được chỉ định
            if (startingBox is StickManData { IsAvailable: true, IsClaimed: false })
            {
                /*// Clear group cũ
                groupBoxes.Clear();

                // Tìm các box thuộc nhóm liên kết
                groupBoxes = boardVisualizer.FindConnectedGroup(startingBox);

                Debug.Log($"Found a group with {groupBoxes.Count} boxes at position {position} with ID {startingBox.id}.");

                if (groupBoxes != null && groupBoxes.Count > 0)
                {*/
                boardVisualizer.ClaimBoxes(new List<BoxData> { startingBox }, startingBox.id, startingBox.GetMiddlePosition());

                HapticController.instance.Play();
                AudioController.PlaySound(SoundKind.UseBoosterUFO);
                var manager = ResourceType.Powerup_Helidrop.Manager();
                InGameTracker.UseBooster();
                manager?.Subtract(1);
                manager?.UI.UpdateUI();
                GameManager.Instance.SetPlayed();
                MessageDispatcher.SendMessage(EventID.USE_BOOSTER, 0);
                /*}*/
            }
        }
    }
}