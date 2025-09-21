using UnityEngine;

namespace HoleBox
{
    using System;
    using System.Collections.Generic;
    using PuzzleGames;
    using Sirenix.OdinInspector;

    public class BarrierCheck : MonoBehaviour
    {
        private TemporaryBoardVisualize boardVisualizer; // Tham chiếu đến bảng game

        private List<BarrierGroup> groupBarriers = new List<BarrierGroup>();

        void Start()
        {
            GameManager.Instance.OnClickHole -= CheckBarrier;
            GameManager.Instance.OnClickHole += CheckBarrier;
        }

        private void OnDestroy() { GameManager.Instance.OnClickHole -= CheckBarrier; }

        public void GetBarrierGroup()
        {
            boardVisualizer = TemporaryBoardVisualize.Instance;

            groupBarriers = new List<BarrierGroup>();

            var boxes = boardVisualizer.Boxes;

            for (int i = 0; i < boxes.Count; i++)
            {
                var box = boxes[i];

                if (box is not ObstacleData { IsBarrier: true } obstacleData) continue;

                if (groupBarriers.Exists(list => list.barriers.Contains(box) && list.isFirstOpen == obstacleData.IsOpenBarrier)) continue;

                var barriers = boardVisualizer.FindConnectedGroup(obstacleData, false);

                var stickmans = GetEnclosedBoxes(barriers);

                if (stickmans.Count == 0)
                {
                    continue;
                }

                var newGroup = new BarrierGroup()
                {
                    barriers    = barriers,
                    stickmans   = stickmans,
                    isFirstOpen = obstacleData.IsOpenBarrier
                };

                groupBarriers.Add(newGroup);
            }

            //LogBarrierGroups();
        }

        private void CheckBarrier()
        {
            for (int i = 0; i < groupBarriers.Count; i++)
            {
                groupBarriers[i].CheckToOpen();
            }
        }


        /// <summary>
        /// Trả về danh sách các BoxData bị bao quanh bởi barriers và biên ma trận, xét cả size của BoxData.
        /// </summary>
        public List<BoxData> GetEnclosedBoxes(List<BoxData> barriers)
        {
            var boxes  = boardVisualizer.Boxes;
            int width  = boardVisualizer.Matrix.x;
            int height = boardVisualizer.Matrix.y;

            int[,] state = new int[width, height];

            // Đánh dấu toàn bộ vùng bị chiếm bởi barrier
            foreach (var barrier in barriers)
            {
                for (int dx = 0; dx < barrier.size.x; dx++)
                {
                    for (int dy = 0; dy < barrier.size.y; dy++)
                    {
                        int px = barrier.position.x + dx;
                        int py = barrier.position.y + dy;
                        if (px >= 0 && py >= 0 && px < width && py < height)
                        {
                            state[px, py] = 1;
                        }
                    }
                }
            }

            // Tìm tất cả vị trí của HoleBoxData
            List<Vector2Int> holePositions = new List<Vector2Int>();
            foreach (var box in boardVisualizer.Holes)
            {
                if (box is HoleBoxData)
                {
                    for (int dx = 0; dx < box.size.x; dx++)
                    {
                        for (int dy = 0; dy < box.size.y; dy++)
                        {
                            int px = box.position.x + dx;
                            int py = box.position.y + dy;
                            if (px >= 0 && py >= 0 && px < width && py < height)
                            {
                                holePositions.Add(new Vector2Int(px, py));
                            }
                        }
                    }
                }
            }

            Queue<Vector2Int> queue = new Queue<Vector2Int>();
            // Thêm các vị trí HoleBoxData vào queue
            foreach (var pos in holePositions)
            {
                if (state[pos.x, pos.y] == 0)
                {
                    queue.Enqueue(pos);
                    state[pos.x, pos.y] = 2;
                }
            }

            int[] dxs = { 0, 1, 0, -1 };
            int[] dys = { 1, 0, -1, 0 };
            while (queue.Count > 0)
            {
                var pos = queue.Dequeue();
                for (int dir = 0; dir < 4; dir++)
                {
                    int nx = pos.x + dxs[dir];
                    int ny = pos.y + dys[dir];
                    if (nx >= 0 && ny >= 0 && nx < width && ny < height && state[nx, ny] == 0)
                    {
                        queue.Enqueue(new Vector2Int(nx, ny));
                        state[nx, ny] = 2;
                    }
                }
            }

            // Các BoxData bị bao quanh là các BoxData mà toàn bộ vùng chiếm đóng đều có state == 0
            List<BoxData> enclosed = new List<BoxData>();
            foreach (var box in boxes)
            {
                if (box is not StickManData)
                {
                    continue;
                }

                bool isEnclosed = true;
                for (int dx = 0; dx < box.size.x && isEnclosed; dx++)
                {
                    for (int dy = 0; dy < box.size.y && isEnclosed; dy++)
                    {
                        int px = box.position.x + dx;
                        int py = box.position.y + dy;
                        if (px < 0 || py < 0 || px >= width || py >= height || state[px, py] != 0)
                        {
                            isEnclosed = false;
                        }
                    }
                }

                if (isEnclosed)
                {
                    enclosed.Add(box);
                }
            }

            return enclosed;
        }

        [Button]
        public void LogBarrierGroups()
        {
            Debug.Log($"Tổng số group: {groupBarriers.Count}");
            for (int i = 0; i < groupBarriers.Count; i++)
            {
                var group = groupBarriers[i];
                Debug.Log($"--- Group {i + 1} ---");
                Debug.Log($"Số lượng barrier: {group.barriers.Count}");
                foreach (var barrier in group.barriers)
                {
                    Debug.Log($"Barrier tại vị trí: {barrier.position.x}, {barrier.position.y}");
                }

                int claimed = 0, unclaimed = 0;
                foreach (var stickman in group.stickmans)
                {
                    // Giả sử BoxData có thuộc tính isClaimed, nếu không hãy thay bằng thuộc tính phù hợp
                    if (stickman.IsClaimed)
                        claimed++;
                    else
                        unclaimed++;
                }

                Debug.Log($"Stickman đã claim: {claimed}");
                Debug.Log($"Stickman chưa claim: {unclaimed}");
            }
        }
    }

    [Serializable]
    public class BarrierGroup
    {
        public List<BoxData> barriers;
        public List<BoxData> stickmans;
        public bool          isFirstOpen;
        public void CheckToOpen()
        {
            bool isOpen = true;

            for (int i = 0; i < stickmans.Count; i++)
            {
                if (!stickmans[i].IsClaimed)
                {
                    isOpen = false;
                    break;
                }
            }

            if (isOpen)
            {
                for (int i = 0; i < barriers.Count; i++)
                {
                    if (barriers[i] is ObstacleData barrier)
                    {
                        barrier.OpenBarrier();
                    }
                }
            }
        }
    }
}