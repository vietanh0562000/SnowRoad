namespace HoleBox
{
    using BasePuzzle.PuzzlePackages.Core;
    using Sirenix.OdinInspector;
    using UnityEngine;

    public class DrawMatrix : MonoBehaviour
    {
        public EdgePrefab   EdgeLeftPrefab;
        public EdgePrefab   EdgeRightPrefab;
        public EdgePrefab   EdgeBot;
        public EdgePrefab   EdgeTop;
        public CornerPrefab CornerLeftTop; // Prefab góc bo
        public CornerPrefab CornerLeftBottom; // Prefab góc bo
        public CornerPrefab CornerRightTop; // Prefab góc bo
        public CornerPrefab CornerRightBottom; // Prefab góc bo
        public Transform    gridMap; // Transform cha chứa đối tượng viền
        public Vector3      gridPos     = new Vector3(-0.5f, 0, -0.5f); // Transform cha chứa đối tượng viền
        public float        range       = 0.5f; // Transform cha chứa đối tượng viền
        public float        cornerRange = 0.2f; // Transform cha chứa đối tượng viền


        [Button]
        public void SpawnMatrixBorder(Vector2Int matrix)
        {
            if (matrix.x <= 0 || matrix.y <= 0)
            {
                Debug.LogWarning("Matrix size hoặc prefab references không hợp lệ.");
                return;
            }

            gridMap.position = Vector3.zero;

            // Vẽ các góc
            SpawnCorner(CornerLeftBottom, -cornerRange, -cornerRange); // Góc dưới bên trái
            SpawnCorner(CornerRightBottom, matrix.x + cornerRange, -cornerRange); // Góc dưới bên phải
            SpawnCorner(CornerLeftTop, -cornerRange, matrix.y + cornerRange); // Góc trên bên trái
            SpawnCorner(CornerRightTop, matrix.x + cornerRange, matrix.y + cornerRange ); // Góc trên bên phải

            // Vẽ các cạnh
            // Cạnh dưới
            SpawnEdge(EdgeBot, Vector3.right, matrix.x - 1, new Vector3(1, 0, -range), false);
            // Cạnh trên
            SpawnEdge(EdgeTop, Vector3.right, matrix.x - 1, new Vector3(1, 0, matrix.y + range), false);

            // Cạnh trái
            SpawnEdge(EdgeLeftPrefab, Vector3.forward, matrix.y - 1, new Vector3(-range, 0, 1), true);
            // Cạnh phải
            SpawnEdge(EdgeRightPrefab, Vector3.forward, matrix.y - 1, new Vector3(matrix.x + range, 0, 1), true);

            gridMap.position = gridPos;
        }

        private void SpawnCorner(CornerPrefab cornerPrefab, float x, float z)
        {
            // Tính vị trí của góc
            Vector3 position = new Vector3(x, 0, z);

            // Spawn góc
            var corner = Instantiate(cornerPrefab, position, Quaternion.identity, gridMap);
            corner.name = $"Corner_{x}_{z}";
        }

        private void SpawnEdge(EdgePrefab edgePrefab, Vector3 direction, int length, Vector3 startPosition, bool isVertical)
        {
            for (int i = 0; i < length; i++)
            {
                // Tính vị trí của mỗi phần tử cạnh
                Vector3 position = startPosition + direction * i;

                // Xoay đúng hướng cho cạnh
                Quaternion rotation = Quaternion.identity;

                // Spawn cạnh
                var edge = Instantiate(edgePrefab, position, rotation, gridMap);
                edge.name = $"Edge_{position.x}_{position.z}";
            }
        }
    }
}