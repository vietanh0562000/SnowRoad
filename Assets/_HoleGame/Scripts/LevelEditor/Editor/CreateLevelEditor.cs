#if UNITY_EDITOR
namespace HoleBox
{
    using DevTools.Extensions;
    using HoleBox;
    using UnityEditor;
    using UnityEngine;

    public enum EditLevelMode
    {
        Draw,
        Move
    }

    [CustomEditor(typeof(CreateLevel))]
    public partial class CreateLevelEditor
    {
        private CreateLevel boardEditor;

        private bool       isDrawing = false; // Đánh dấu trạng thái kéo chuột và vẽ Box
        private Vector2Int MatrixSize => boardEditor.Matrix;
        private BoxData    currentBox; // Lưu trữ Box đang được vẽ tạm thời
        private Vector2Int previewCell; // Cell hiện tại chuột đang chỉ vào

        #region Move Mode

        private BoxData    selectedBox;
        private Vector2Int originalPosition;
        private bool       isDraggingBox = false;

        #endregion

        private int ObstacleID => GameConstants.ObstacleID;

        private BoxType       currentMode = BoxType.StickMan; // Mặc định là vẽ Box
        private EditLevelMode EditMode    = EditLevelMode.Draw;

        private void OnEnable()
        {
            boardEditor = (CreateLevel)target;
            LoadLevelNames();
        }

        public override void OnInspectorGUI()
        {
            GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize  = 15,
                normal    = new GUIStyleState { textColor = Color.cyan }, // Màu chữ (cyan)
                alignment = TextAnchor.MiddleCenter // Căn giữa
            };
            EditorGUILayout.LabelField($"❖ Draw Mode : {currentMode.ToString()} ❖", headerStyle);
            headerStyle.normal = new GUIStyleState() { textColor = Color.green };
            EditorGUILayout.LabelField($"★ Edit Mode : {EditMode.ToString()} ★", headerStyle);
            OnGUISaveLoad();
            OnGUIDrawContainer();
        }

        private void OnSceneGUI()
        {
            Event e = Event.current;
            if (e == null) return;

            if (SelectingKeyHole && TargetHoleData != null)
            {
                HandleKeyHoleSelection();
            }

            // Raycast để xác định vị trí chuột trên Scene
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hitPosition))
            {
                Vector3 worldPos = hitPosition.point;
                previewCell = WorldToGrid(worldPos);

                if (e.type == EventType.MouseDown && e.button == 1)
                {
                    EditObject(previewCell);
                    e.Use();
                }
                else if (e.type == EventType.KeyDown && e.keyCode == KeyCode.LeftShift)
                {
                    DeleteObject(previewCell);
                    e.Use();
                }
                else if (EditMode == EditLevelMode.Draw)
                {
                    // Vẽ Preview trên lưới - chỉ áp dụng khi chế độ là Hole
                    if (currentMode == BoxType.Hole)
                    {
                        DrawPreviewGrid(previewCell);
                    }

                    if (currentMode is BoxType.StickMan or BoxType.Obstacle
                        && isDrawing) // Vẽ Box khi kéo chuột
                    {
                        UpdateBoxDrawing(previewCell);
                    }

                    // Nhấn chuột trái để vẽ
                    if (e.type == EventType.MouseDown && e.button == 0)
                    {
                        StartDrawing(previewCell);
                        e.Use();
                    }
                    // Thả chuột trái để hoàn tất vẽ
                    else if (e.type == EventType.MouseUp && e.button == 0)
                    {
                        FinishDrawing();
                        e.Use();
                    }
                    // Nhấn chuột phải để xóa đối tượng
                }
                else if (EditMode == EditLevelMode.Move)
                {
                    if (e.type == EventType.MouseDown)
                    {
                        selectedBox = FindBoxAtCell(previewCell);
                        if (selectedBox != null)
                        {
                            originalPosition = selectedBox.position;
                            isDraggingBox    = true;
                            e.Use();
                        }
                    }
                    else if (e.type == EventType.MouseDrag && isDraggingBox)
                    {
                        Vector2Int clampedCell = ClampToMatrix(previewCell);
                        selectedBox.position = clampedCell;

                        DrawBoxGhost(selectedBox, IsValidPosition(selectedBox, clampedCell));
                        SceneView.RepaintAll();
                        e.Use();
                    }
                    else if (e.type == EventType.MouseUp && isDraggingBox)
                    {
                        bool valid = IsValidPosition(selectedBox, selectedBox.position);
                        if (!valid)
                        {
                            selectedBox.position = originalPosition;
                            Debug.LogWarning("Box moved to invalid position. Reverting.");
                        }
                        else
                        {
                            Debug.Log($"Box moved to: {selectedBox.position}");
                        }

                        selectedBox   = null;
                        isDraggingBox = false;
                        e.Use();
                    }
                }
            }

            // Thay đổi chế độ vẽ bằng phím tắt
            HandleModeChange(e);
        }

        private void HandleModeChange(Event e)
        {
            if (e.type == EventType.KeyDown)
            {
                if (e.keyCode == KeyCode.Z)
                {
                    EditMode    = EditLevelMode.Draw;
                    currentMode = BoxType.StickMan;
                }
                else if (e.keyCode == KeyCode.X)
                {
                    EditMode    = EditLevelMode.Draw;
                    currentMode = BoxType.Hole;
                }
                else if (e.keyCode == KeyCode.C)
                {
                    EditMode    = EditLevelMode.Draw;
                    currentMode = BoxType.Obstacle;
                }
                else if (e.keyCode == KeyCode.V)
                {
                    EditMode    = EditLevelMode.Draw;
                    currentMode = BoxType.Enemy;
                }
                else if (e.keyCode == KeyCode.B)
                {
                    EditMode    = EditLevelMode.Draw;
                    currentMode = BoxType.Tunnel;
                }

                if (e.keyCode == KeyCode.M)
                {
                    if (EditMode == EditLevelMode.Draw)
                    {
                        EditMode = EditLevelMode.Move;
                    }
                    else
                    {
                        EditMode = EditLevelMode.Draw;
                    }
                }


                Repaint();
            }
        }

        private void StartDrawing(Vector2Int cell)
        {
            if (!IsInsideMatrix(cell))
            {
                Debug.LogWarning("Start position is out of bounds!");
                return;
            }

            switch (currentMode)
            {
                case BoxType.StickMan:
                    StartBoxDrawing(cell);
                    break;
                case BoxType.Obstacle:
                    StartBoxDrawing(cell, ObstacleID);
                    break;
                case BoxType.Tunnel:
                    StartBoxDrawing(cell);
                    break;
                case BoxType.Hole:
                    AddHole(cell);
                    break;
            }
        }

        private void StartBoxDrawing(Vector2Int cell, int id = 1)
        {
            // Kiểm tra nếu tọa độ bắt đầu nằm ngoài giới hạn của ma trận
            if (cell.x < 0 || cell.y < 0 || cell.x >= MatrixSize.x || cell.y >= MatrixSize.y)
            {
                Debug.LogWarning("Start position is out of bounds!");
                return;
            }

            if ((cell.x >= MatrixSize.x - 1 || cell.y >= MatrixSize.y - 1))
            {
                Debug.LogWarning("Start position is out of bounds!");
                return;
            }

            isDrawing = true;

            // if (currentMode is BoxType.StickMan)
            // {
            //     // Tạo Box ban đầu với kích thước 1x1
            //     currentBox = new StickManData()
            //     {
            //         id       = id,
            //         position = cell,
            //         size     = Vector2Int.one * 2
            //     };
            // }
            // else if (currentMode is BoxType.Obstacle)
            // {
            //     // Tạo Box ban đầu với kích thước 1x1
            //     currentBox = new ObstacleData()
            //     {
            //         id       = id,
            //         position = cell,
            //         size     = Vector2Int.one * 2
            //     };
            // }
            // else if (currentMode is BoxType.Tunnel)
            // {
            //     // Tạo Box ban đầu với kích thước 1x1
            //     currentBox = new TunnelData()
            //     {
            //         id        = id,
            //         position  = cell,
            //         size      = Vector2Int.one * 2,
            //         direction = Vector2Int.right
            //     };
            // }

            if (IsOverlapping(currentBox.position, currentBox.size))
            {
                Debug.LogWarning("Cannot create box: Overlapping with another box!");
                isDrawing = false;
                return;
            }

            boardEditor.Boxes.Add(currentBox);
        }

        private void UpdateBoxDrawing(Vector2Int cell)
        {
            if (!isDrawing || currentBox == null) return;

            // Giới hạn cell trong ma trận
            cell = ClampToMatrix(cell);

            // Cập nhật kích thước và vị trí của Box đang kéo
            Vector2Int newSize = new Vector2Int(
                Mathf.Abs(cell.x - currentBox.position.x) + 1,
                Mathf.Abs(cell.y - currentBox.position.y) + 1
            );

            const int minimumHalfSize = 1; // Minimum valid value for half the size (x-dimension)

            int halfSizeX = newSize.x / 2;
            halfSizeX = Mathf.Max(halfSizeX, minimumHalfSize);
            newSize.x = halfSizeX * 2;

            int halfSizeY = newSize.y / 2;
            halfSizeY = Mathf.Max(halfSizeY, minimumHalfSize);
            newSize.y = halfSizeY * 2;

            Vector2Int newPosition = new Vector2Int(
                Mathf.Min(cell.x, currentBox.position.x),
                Mathf.Min(cell.y, currentBox.position.y)
            );

            if (IsOverlappingWithoutCurrent(newPosition, newSize))
            {
                Debug.LogWarning("Cannot resize box: Overlapping with another box!");
                return;
            }

            currentBox.size     = newSize;
            currentBox.position = newPosition;

            // Cập nhật Box
            SceneView.RepaintAll();
        }

        private void FinishDrawing()
        {
            if (currentMode == BoxType.StickMan && currentBox != null)
            {
                if (!IsInsideMatrix(currentBox.position))
                {
                    Debug.LogWarning("Start position is out of bounds!");
                    currentBox = null;
                    return;
                }

                if (IsOverlappingWithoutCurrent(currentBox.position, currentBox.size))
                {
                    Debug.LogWarning("Cannot draw box: Overlapping with another box!");
                    currentBox = null;
                    return;
                }

                // Hiển thị popup chỉnh sửa nếu là Box
                // BoxDataEditorWindow.ShowBoxEditor(currentBox, box =>
                // {
                //     Debug.Log($"Updated Box: ID={box.id}, Position={box.position}, Size={box.size}");
                //     currentBox = null;
                // });
            }

            isDrawing = false;
        }

        private void AddHole(Vector2Int cell)
        {
            Vector2Int holeSize = new Vector2Int(4, 4);

            // Kiểm tra hợp lệ
            if (IsOverlapping(cell, holeSize))
            {
                Debug.LogWarning("Cannot create hole: Overlapping with another object!");
                return;
            }

            if (!IsInsideMatrix(cell, holeSize))
            {
                Debug.LogWarning("Hole is out of matrix bounds!");
                return;
            }

            // Tạo Hole mới
            // HoleBoxData newHole = new HoleBoxData
            // {
            //     id       = 1,
            //     position = cell,
            //     size     = holeSize
            // };
            //
            // boardEditor.Boxes.Add(newHole);
            //
            // // Hiển thị popup chỉnh sửa dữ liệu Hole
            // BoxDataEditorWindow.ShowBoxEditor(newHole, box => { Debug.Log($"Updated Hole: ID={box.id}, Position={box.position}, Size={box.size}"); });
        }

        private void DrawPreviewGrid(Vector2Int cell)
        {
            Vector2Int holeSize = new Vector2Int(4, 4);
            bool       isValid  = !IsOverlapping(cell, holeSize) && IsInsideMatrix(cell, holeSize);

            // Chọn màu: Xanh lá cây nếu hợp lệ, Đỏ nếu không hợp lệ
            Color gridColor = isValid ? Color.green : Color.red;

            Handles.color = gridColor;
            Vector3 bottomLeft = new Vector3(cell.x, 0, cell.y);
            Vector3 topRight   = new Vector3(cell.x + holeSize.x, 0, cell.y + holeSize.y);
            Handles.DrawSolidRectangleWithOutline(
                new[]
                {
                    bottomLeft,
                    new Vector3(topRight.x, 0, bottomLeft.z),
                    topRight,
                    new Vector3(bottomLeft.x, 0, topRight.z)
                },
                new Color(gridColor.r, gridColor.g, gridColor.b, 0.2f), // Màu trong suốt
                gridColor // Màu viền
            );
        }

        private bool IsInsideMatrix(Vector2Int cell) { return cell.x >= 0 && cell.y >= 0 && cell.x < MatrixSize.x && cell.y < MatrixSize.y; }

        private bool IsInsideMatrix(Vector2Int cell, Vector2Int size)
        {
            return cell.x >= 0 && cell.y >= 0 &&
                   cell.x + size.x <= MatrixSize.x &&
                   cell.y + size.y <= MatrixSize.y;
        }

        private bool IsOverlapping(Vector2Int position, Vector2Int size)
        {
            foreach (var box in boardEditor.Boxes)
            {
                bool xOverlap = position.x < box.position.x + box.size.x && position.x + size.x > box.position.x;
                bool yOverlap = position.y < box.position.y + box.size.y && position.y + size.y > box.position.y;

                if (xOverlap && yOverlap) return true;
            }

            return false;
        }


        private bool IsOverlappingWithoutCurrent(Vector2Int newPosition, Vector2Int newSize)
        {
            // Kiểm tra trùng lấp trước khi cập nhật
            foreach (var box in boardEditor.Boxes)
            {
                if (box == currentBox)
                    continue;

                bool xOverlap = newPosition.x < box.position.x + box.size.x && newPosition.x + newSize.x > box.position.x;
                bool yOverlap = newPosition.y < box.position.y + box.size.y && newPosition.y + newSize.y > box.position.y;

                if (xOverlap && yOverlap)
                {
                    return true;
                }
            }

            return false;
        }

        private Vector2Int ClampToMatrix(Vector2Int cell)
        {
            return new Vector2Int(
                Mathf.Clamp(cell.x, 0, MatrixSize.x - 1),
                Mathf.Clamp(cell.y, 0, MatrixSize.y - 1)
            );
        }

        private Vector2Int WorldToGrid(Vector3 worldPos)
        {
            int x = Mathf.FloorToInt(worldPos.x);
            int y = Mathf.FloorToInt(worldPos.z);
            return new Vector2Int(x, y);
        }
        private void DeleteObject(Vector2Int cell)
        {
            // Tìm đối tượng tại vị trí được chỉ định
            BoxData boxToDelete = null;

            foreach (var box in boardEditor.Boxes)
            {
                // Kiểm tra xem cell có nằm trong phạm vi của box hay không
                if (cell.x >= box.position.x && cell.x < box.position.x + box.size.x &&
                    cell.y >= box.position.y && cell.y < box.position.y + box.size.y)
                {
                    boxToDelete = box;
                    boardEditor.Boxes.Remove(boxToDelete);
                    break;
                }
            }

            // Nếu tìm thấy box, hãy xóa nó
            if (boxToDelete != null)
            {
                Debug.Log($"Deleted: ID={boxToDelete.id}, Position={boxToDelete.position}, Size={boxToDelete.size}");
            }
            else
            {
                Debug.LogWarning("No object found to delete at this position!");
            }
        }

        private void EditObject(Vector2Int cell)
        {
            foreach (var box in boardEditor.Boxes)
            {
                // Kiểm tra xem cell có nằm trong phạm vi của box hay không
                if (cell.x >= box.position.x && cell.x < box.position.x + box.size.x &&
                    cell.y >= box.position.y && cell.y < box.position.y + box.size.y)
                {
                    // Hiển thị popup chỉnh sửa nếu là Box
                    //BoxDataEditorWindow.ShowBoxEditor(box, b => { Debug.Log($"Updated Box: ID={b.id}, Position={b.position}, Size={b.size}"); });
                    break;
                }
            }
        }

        private bool IsValidPosition(BoxData box, Vector2Int newPosition)
        {
            if (!IsInsideMatrix(newPosition, box.size)) return false;

            foreach (var other in boardEditor.Boxes)
            {
                if (other == box) continue;

                bool xOverlap = newPosition.x < other.position.x + other.size.x && newPosition.x + box.size.x > other.position.x;
                bool yOverlap = newPosition.y < other.position.y + other.size.y && newPosition.y + box.size.y > other.position.y;

                if (xOverlap && yOverlap) return false;
            }

            return true;
        }
        private BoxData FindBoxAtCell(Vector2Int cell)
        {
            foreach (var box in boardEditor.Boxes)
            {
                if (cell.x >= box.position.x && cell.x < box.position.x + box.size.x &&
                    cell.y >= box.position.y && cell.y < box.position.y + box.size.y)
                {
                    return box;
                }
            }

            return null;
        }

        private void DrawBoxGhost(BoxData box, bool isValid)
        {
            Color c = isValid ? Color.green : Color.red;
            Handles.color = c;

            Vector3 bottomLeft = new Vector3(box.position.x, 0, box.position.y);
            Vector3 topRight   = new Vector3(box.position.x + box.size.x, 0, box.position.y + box.size.y);

            Handles.DrawSolidRectangleWithOutline(
                new[]
                {
                    bottomLeft,
                    new Vector3(topRight.x, 0, bottomLeft.z),
                    topRight,
                    new Vector3(bottomLeft.x, 0, topRight.z)
                },
                new Color(c.r, c.g, c.b, 0.2f),
                c
            );
        }

        private void RePaintAll()
        {
            SceneView.RepaintAll();
            Repaint();
        }
    }
}
#endif