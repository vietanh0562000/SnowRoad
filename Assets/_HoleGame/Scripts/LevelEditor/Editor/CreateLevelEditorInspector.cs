using UnityEditor;
using UnityEngine;

namespace HoleBox
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public partial class CreateLevelEditor
    {
        private Vector2 scrollPosition; // Lưu vị trí thanh cuộn ngang

        public void OnGUIDrawContainer()
        {
            serializedObject.Update();

            // Header
            EditorGUILayout.Space();

            // Lấy đối tượng `CreateLevel`
            var createLevel = (CreateLevel)target;

// Hiển thị tiêu đề in đậm
            EditorGUILayout.LabelField("Matrix Size", EditorStyles.boldLabel);

            int[]    validSizes      = BoxDataEditorUtils.MatrixSizes;
            string[] dropdownOptions = Array.ConvertAll(validSizes, size => size.ToString()); // Dạng chuỗi dùng cho dropdown

// Lấy chỉ số trong danh sách `validSizes` ứng với giá trị hiện tại của `Matrix`
            int currentXIndex = Array.IndexOf(validSizes, createLevel.Matrix.x);
            int currentYIndex = Array.IndexOf(validSizes, createLevel.Matrix.y);

// Xử lý nếu giá trị hiện tại của `Matrix` không hợp lệ (không nằm trong danh sách validSizes)
            if (currentXIndex == -1) currentXIndex = 0;
            if (currentYIndex == -1) currentYIndex = 0;

// Hiển thị dropdown cho X và Y
            int newXIndex = EditorGUILayout.Popup("Width", currentXIndex, dropdownOptions); // Dropdown cho Width
            int newYIndex = EditorGUILayout.Popup("Height", currentYIndex, dropdownOptions); // Dropdown cho Height

// Lấy giá trị mới tương ứng từ danh sách
            int newX = validSizes[newXIndex];
            int newY = validSizes[newYIndex];

// Tập hợp thành một Vector2Int mới
            Vector2Int newMatrixSize = new Vector2Int(newX, newY);

// Cập nhật giá trị khi phát hiện thay đổi
            if (newMatrixSize != createLevel.Matrix)
            {
                Undo.RecordObject(createLevel, "Change Matrix Size"); // Hỗ trợ Undo
                createLevel.Matrix = newMatrixSize; // Gán giá trị mới
                RePaintAll(); // Vẽ lại (nếu cần thiết)
            }

            // Tự động refresh
            if (GUI.changed)
            {
                EditorUtility.SetDirty(createLevel); // Đánh dấu đối tượng đã thay đổi
            }

            EditorGUILayout.LabelField("Container Queues Editor", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

// Thêm nhóm "Count" với dropdown
            EditorGUILayout.BeginVertical(GUILayout.Width(200)); // Nhóm Count với chiều rộng riêng
            EditorGUILayout.PrefixLabel(new GUIContent("Count")); // Nhãn "Count"
            createLevel.StaticContainerConfig.Count = Mathf.Clamp( // Giới hạn giá trị từ 1 đến 10
                EditorGUILayout.IntField(
                    createLevel.StaticContainerConfig.Count, // Giá trị hiện tại
                    GUILayout.Width(80)), // Chiều rộng của ô nhập
                1, // Giá trị tối thiểu
                10 // Giá trị tối đa
            );
            EditorGUILayout.EndVertical();

// Khoảng cách giữa Count và Capacity
            GUILayout.Space(20);

// Thêm nhóm "Capacity" với dropdown
            EditorGUILayout.BeginVertical(GUILayout.Width(200)); // Nhóm Capacity với chiều rộng riêng
            EditorGUILayout.PrefixLabel(new GUIContent("Capacity")); // Nhãn "Capacity"

            int[]    validCapacities = BoxDataEditorUtils.CapacitySizes;
            string[] capacityOptions = Array.ConvertAll(validCapacities, size => size.ToString());

            int currentCapacityIndex = System.Array.IndexOf(
                validCapacities,
                createLevel.StaticContainerConfig.Capacity); // Tìm index hiện tại

            currentCapacityIndex = EditorGUILayout.Popup(
                currentCapacityIndex < 0 ? 0 : currentCapacityIndex, // Đảm bảo giá trị hợp lệ
                capacityOptions,
                GUILayout.Width(80));
            createLevel.StaticContainerConfig.Capacity = int.Parse(capacityOptions[currentCapacityIndex]); // Chuyển đổi về số nguyên
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

            if (createLevel.ContainerQueues != null && createLevel.ContainerQueues.Count > 0)
            {
                // Bắt đầu một scroll view để cuộn ngang
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

                // Layout chính: dùng `BeginHorizontal` để thêm cột ngang
                EditorGUILayout.BeginHorizontal();

                // Tính toán chiều rộng cột linh hoạt, đảm bảo cuộn ngang hoạt động
                float columnWidth = 100; // Thay giá trị này để tùy chỉnh chiều rộng cột

                for (int i = 0; i < createLevel.ContainerQueues.Count; i++)
                {
                    var containerQueue = createLevel.ContainerQueues[i];

                    // Tạo mỗi cột trong một Vertical Layout
                    GUILayoutOption[] columnOptions = { GUILayout.Width(columnWidth), GUILayout.ExpandHeight(true) };
                    EditorGUILayout.BeginVertical("box", columnOptions);

                    // Header cho từng cột
                    EditorGUILayout.LabelField($"Queue {i + 1}", EditorStyles.boldLabel, GUILayout.Width(columnWidth));

                    if (containerQueue.containerDatas != null)
                    {
// Hiển thị danh sách ContainerData
                        for (int j = 0; j < containerQueue.containerDatas.Count; j++)
                        {
                            var containerData = containerQueue.containerDatas[j];

                            Color originalBackgroundColor = GUI.backgroundColor;
                            GUI.backgroundColor = GameLogicUltils.GetColor(containerData.id);

                            // Bắt đầu một dòng ngang
                            EditorGUILayout.BeginHorizontal();

                            // Hiển thị nút với caption là capacity
                            if (GUILayout.Button($"Cap: {containerData.capacity}", GUILayout.Width(columnWidth - 30), GUILayout.Height(30)))
                            {
                                // Mở cửa sổ chỉnh sửa ContainerData
                                ContainerDataEditorWindow.Open(containerData);
                            }

                            GUI.backgroundColor = originalBackgroundColor;

                            // Nút delete (hiển thị dấu "-") bên cạnh
                            if (GUILayout.Button("X", GUILayout.Width(30), GUILayout.Height(30)))
                            {
                                // Xóa ContainerData khỏi danh sách
                                containerQueue.containerDatas.RemoveAt(j);
                                break; // Thoát khỏi vòng lặp để tránh lỗi khi sửa danh sách
                            }

                            // Kết thúc dòng ngang
                            EditorGUILayout.EndHorizontal();
                        }
                    }

                    // Nút thêm ContainerData vào Queue
                    if (GUILayout.Button("Add Container", GUILayout.Width(columnWidth), GUILayout.Height(25)))
                    {
                        var c = new ContainerData(1, createLevel.StaticContainerConfig.Capacity);

                        containerQueue.containerDatas.Add(c);

                        // Mở cửa sổ chỉnh sửa ContainerData
                        ContainerDataEditorWindow.Open(c);
                    }

                    // Nút xóa Queue
                    if (GUILayout.Button("Delete Queue", GUILayout.Width(columnWidth), GUILayout.Height(25)))
                    {
                        createLevel.ContainerQueues.RemoveAt(i);
                        break; // Thoát khỏi vòng lặp để tránh lỗi
                    }

                    EditorGUILayout.EndVertical(); // Kết thúc 1 cột
                }

                EditorGUILayout.EndHorizontal(); // Kết thúc layout ngang

                EditorGUILayout.EndScrollView(); // Kết thúc ScrollView
            }
            else
            {
                // Hiển thị thông báo nếu không có Queue nào
                EditorGUILayout.HelpBox("No Container Queues found. Add new queues below.", MessageType.Info);
            }

            // Nút thêm Queue mới
            if (GUILayout.Button("Add New Queue", GUILayout.Height(30)))
            {
                int count = createLevel.ContainerQueues?.Count > 0
                    ? createLevel.ContainerQueues[^1].containerDatas.Count
                    : 1;
                createLevel.ContainerQueues ??= new List<ContainerQueueData>(); // Mở danh sách nếu null
                createLevel.ContainerQueues.Add(new ContainerQueueData(count)); // Thêm phần tử mới
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void OnGUIValidateLevel()
        {
            // Nút Validate Level
            if (GUILayout.Button("Validate Level", GUILayout.Height(35)))
            {
                ValidateLevel();
            }

            if (_validationResults != null && _validationResults.Count > 0)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Validation Details", EditorStyles.boldLabel);

                foreach (var result in _validationResults)
                {
                    DisplayValidationResult(result);
                }
            }
            else if (_lastValidationResult)
            {
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox(_overallMessage, MessageType.Info);
            }
        }

        private List<CreateLevelValidator.ValidationResult> _validationResults;
        private bool                                        _lastValidationResult;
        private string                                      _overallMessage;
        private void ValidateLevel()
        {
            // Truy cập đối tượng CreateLevel hiện tại
            CreateLevel createLevel = (CreateLevel)target;

            // Tìm danh sách ContainerQueueData
            List<ContainerQueueData> containerQueueDatas = createLevel.ContainerQueues;

            if (containerQueueDatas == null || containerQueueDatas.Count == 0)
            {
                _lastValidationResult = false;
                _overallMessage       = "Không tìm thấy danh sách ContainerQueueData. Vui lòng gắn các ContainerQueueData.";
                return;
            }

            //_lastValidationResult = CreateLevelValidator.ValidateLevel(createLevel, containerQueueDatas, out _validationResults);

            _overallMessage = _lastValidationResult ? "Level hợp lệ!" : "Level không hợp lệ! Kiểm tra chi tiết bên dưới:";
        }

        private void DisplayValidationResult(CreateLevelValidator.ValidationResult result)
        {
            GUILayout.BeginHorizontal();

            // Vẽ khối màu với kích thước cố định
            Rect rect = GUILayoutUtility.GetRect(20, 20, GUILayout.Width(20), GUILayout.Height(20));
            EditorGUI.DrawRect(rect, result.IdColor); // Vẽ màu trực tiếp

            // Tạo GUIStyle tùy chỉnh với font lớn hơn
            GUIStyle customStyle = new GUIStyle(EditorStyles.wordWrappedLabel)
            {
                fontSize = 14, // Thay đổi size font tại đây (ví dụ 14)
            };

            // Hiển thị nội dung với style tùy chỉnh
            GUILayout.Label($"ID {result.ContainerId}: {result.ErrorMessage}", customStyle);

            GUILayout.EndHorizontal();
        }
    }
}