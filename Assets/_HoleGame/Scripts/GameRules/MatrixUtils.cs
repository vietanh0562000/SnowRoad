namespace HoleBox
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public static class MatrixUtils
    {
        public static List<BoxData> FindConnectedGroup(this TemporaryBoardVisualize boardVisualizer, BoxData startingBox, bool isStickMan = true)
        {
            List<BoxData>    group   = new List<BoxData>(); // Kết quả các box trong nhóm
            Queue<BoxData>   queue   = new Queue<BoxData>(); // Hàng đợi xử lý các box
            HashSet<BoxData> visited = new HashSet<BoxData>(); // Đánh dấu các box đã kiểm tra

            // Lưu ID của box bắt đầu (chỉ tìm các box có cùng ID)
            int targetID = startingBox.id;

            Type targetType = startingBox.GetType();

            // Bắt đầu flood fill từ box đầu tiên
            queue.Enqueue(startingBox);
            visited.Add(startingBox);

            while (queue.Count > 0)
            {
                BoxData currentBox = queue.Dequeue();
                group.Add(currentBox);

                // Duyệt qua các vị trí xung quanh biên của box
                foreach (var adjacentPosition in GetAdjacentPositions(currentBox))
                {
                    BoxData adjacentBox = boardVisualizer.Boxes.GetBoxData(adjacentPosition);

                    // Kiểm tra ô kề cận: 
                    // - Có tồn tại 
                    // - Có cùng ID
                    // - Chưa được kiểm tra
                    if (adjacentBox != null && adjacentBox.id == targetID
                                            && !visited.Contains(adjacentBox) && adjacentBox.GetType() == targetType)
                    {
                        if (isStickMan && adjacentBox is StickManData { IsAvailable: false })
                        {
                            continue;
                        }
                        
                        queue.Enqueue(adjacentBox);
                        visited.Add(adjacentBox);
                    }
                }
            }

            return group;
        }

        private static List<Vector2Int> GetAdjacentPositions(BoxData box)
        {
            List<Vector2Int> adjacentPositions = new List<Vector2Int>();

            Vector2Int start = box.position; // Góc trên-trái của box
            Vector2Int size  = box.size; // Kích thước của box

            // **Duyệt qua Biên:**

            // Cạnh trên (loại góc trên-trái và trên-phải)
            for (int x = 0; x < size.x; x++)
            {
                adjacentPositions.Add(new Vector2Int(start.x + x, start.y - 1));
            }

            // Cạnh dưới (loại góc dưới-trái và dưới-phải)
            for (int x = 0; x < size.x; x++)
            {
                adjacentPositions.Add(new Vector2Int(start.x + x, start.y + size.y));
            }

            // Cạnh trái (loại góc trên-trái và dưới-trái)
            for (int y = 0; y < size.y; y++)
            {
                adjacentPositions.Add(new Vector2Int(start.x - 1, start.y + y));
            }

            // Cạnh phải (loại góc trên-phải và dưới-phải)
            for (int y = 0; y < size.y; y++)
            {
                adjacentPositions.Add(new Vector2Int(start.x + size.x, start.y + y));
            }

            return adjacentPositions;
        }

        public static Vector2Int WorldToMatrix(this Vector3 worldPosition)
        {
            // Giả sử ma trận game bắt đầu tại (0, 0) trong thế giới Unity
            // Chuyển đổi tọa độ thế giới thành tọa độ ma trận dựa trên kích thước ô
            float cellSize = 1f; // Kích thước mỗi ô trong đơn vị thế giới

            int x = Mathf.FloorToInt(worldPosition.x / cellSize);
            int y = Mathf.FloorToInt(worldPosition.z / cellSize); // Z trong World Space là Y trong Matrix

            return new Vector2Int(x, y);
        }

        public static Vector3 MatrixToWorld(this Vector2Int worldPosition) { return new Vector3(worldPosition.x, 0, worldPosition.y); }

        public static BoxData GetBoxDataAtPosition(this List<BoxData> boxes, Vector2Int position)
        {
            foreach (var box in boxes) // Duyệt qua toàn bộ các BoxData trong map
            {
                if (box.InsideBox(position) && box.MatrixValue != 0)
                {
                    return box; // Trả về BoxData khớp vị trí
                }
            }

            return null; // Không tìm thấy BoxData tại vị trí này
        }
        
        public static BoxData GetBoxData(this List<BoxData> boxes, Vector2Int position)
        {
            foreach (var box in boxes) // Duyệt qua toàn bộ các BoxData trong map
            {
                if (box.InsideBox(position))
                {
                    return box; // Trả về BoxData khớp vị trí
                }
            }

            return null; // Không tìm thấy BoxData tại vị trí này
        }
    }
}