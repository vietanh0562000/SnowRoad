namespace HoleBox
{
    using System.Collections.Generic;
    using System.Linq;
    using Sirenix.OdinInspector;
    using UnityEngine;
    using UnityEngine.Serialization;

    // Enum chế độ vẽ
    public enum BoxType
    {
        StickMan,
        Hole,
        Obstacle,
        Enemy,
        Tunnel
    }


    public class CreateLevel : MonoBehaviour
    {
        public Vector2Int Matrix = new Vector2Int(10, 10); // Kích thước ma trận (số hàng và số cột)

        public List<BoxData> Boxes = new();

        public StaticContainerConfig    StaticContainerConfig;
        public List<ContainerQueueData> ContainerQueues;

        public void SetLevelData(LevelData levelData)
        {
            Matrix                = levelData.Matrix;
            Boxes                 = levelData.Boxes;
            StaticContainerConfig = levelData.StaticConfig;
            ContainerQueues       = levelData.ContainerQueues;
        }

        public LevelData GetLevelData()
        {
            return new LevelData(Matrix, Boxes, StaticContainerConfig, ContainerQueues);
        }

        private void OnDrawGizmosSelected()
        {
            // Hiển thị lưới trong Scene
            Gizmos.color = Color.gray;
            for (int x = 0; x <= Matrix.x; x++)
            {
                Gizmos.DrawLine(new Vector3(x, 0, 0), new Vector3(x, 0, Matrix.y));
            }

            for (int y = 0; y <= Matrix.y; y++)
            {
                Gizmos.DrawLine(new Vector3(0, 0, y), new Vector3(Matrix.x, 0, y));
            }

            // Hiển thị các Box
            foreach (var box in Boxes)
            {
                if (box is HoleBoxData hole)
                {
                    Vector3 holeCenter = new Vector3(hole.position.x + hole.size.x / 2f, 0, hole.position.y + hole.size.y / 2f);

                    Gizmos.color = GameLogicUltils.GetColor(hole.id);
                    Gizmos.DrawSphere(holeCenter, hole.size.x / 2);

                    if (hole.closedHole)
                    {
                        Gizmos.DrawIcon(holeCenter, "ClosedHole", false);
                    }

                    if (hole.lockedHole)
                    {
                        Gizmos.DrawIcon(holeCenter, "Lock", false);

                        var keyHole = Boxes.GetBoxDataAtPosition(hole.keyPos);
                        if (keyHole != null)
                        {
                            Vector3 keyCenter = new Vector3(keyHole.position.x + keyHole.size.x / 2f,
                                0, keyHole.position.y + keyHole.size.y / 2f);
                            Gizmos.DrawIcon(keyCenter, "Key", false);
                        }
                    }

                    continue;
                }

                Vector3 boxCenter = new Vector3(box.position.x + box.size.x / 2f, 0, box.position.y + box.size.y / 2f);
                Vector3 boxSize   = new Vector3(box.size.x, 0.1f, box.size.y);
                Gizmos.color = GameLogicUltils.GetColor(box.id);
                Gizmos.DrawCube(boxCenter, boxSize);

                //todo: for testing mechanic
                if (box is StickManData stickManData)
                {
                    if (stickManData.IsFrozen)
                    {
                        Gizmos.DrawIcon(boxCenter, "Ice", false);
                    }

                    if (stickManData.IsHidden)
                    {
                        Gizmos.DrawIcon(boxCenter, "Bom", false);
                    }
                }

                if (box is TunnelData)
                {
                    Gizmos.DrawIcon(boxCenter, "Tunnel", false);
                }

                if (box is ObstacleData obstacleData)
                {
                    if (obstacleData.IsBarrier)
                    {
                        Gizmos.DrawIcon(boxCenter, "Barrier", false);
                    }
                }
            }
        }
    }
}