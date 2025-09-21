using UnityEditor;
using UnityEngine;

namespace HoleBox
{
    using System;

    public class ContainerDataEditorWindow : EditorWindow
    {
        private ContainerData containerData;

        // Tạo cửa sổ chỉnh sửa
        public static void Open(ContainerData containerData)
        {
            var window = GetWindow<ContainerDataEditorWindow>("Edit Container Data");
            window.containerData = containerData;
        }

        private void OnGUI()
        {
            // Kiểm tra nếu đối tượng ContainerData đã được truyền vào
            if (containerData == null)
            {
                EditorGUILayout.HelpBox("No Container Data assigned.", MessageType.Warning);
                return;
            }

            // Sửa các thuộc tính của ContainerData
            EditorGUILayout.LabelField("Container Capacity", EditorStyles.boldLabel);

// Lấy danh sách giá trị hợp lệ từ BoxDataEditorUtils
            int[]    validCapacities = BoxDataEditorUtils.CapacitySizes;
            string[] dropdownOptions = Array.ConvertAll(validCapacities, size => size.ToString());

// Lấy chỉ số hiện tại của capacity trong danh sách
            int currentCapacityIndex = Array.IndexOf(validCapacities, containerData.capacity);

// Xử lý nếu giá trị hiện tại không nằm trong danh sách (không hợp lệ)
            if (currentCapacityIndex == -1) currentCapacityIndex = 0;

// Hiển thị dropdown cho capacity và cập nhật khi thay đổi
            int newCapacityIndex = EditorGUILayout.Popup("Capacity", currentCapacityIndex, dropdownOptions);
            int newCapacity      = validCapacities[newCapacityIndex];

// Cập nhật giá trị khi phát hiện thay đổi
            if (newCapacity != containerData.capacity)
            {
                containerData.capacity = newCapacity; // Gán giá trị mới
            }

            // Color selection toggles
            GUILayout.Space(10);
            GUILayout.Label("Select a Color (ID):");
            if (containerData == null)
            {
                EditorGUILayout.HelpBox("No Container Data assigned.", MessageType.Warning);
                return;
            }

            // Display colors horizontally
            EditorGUILayout.BeginHorizontal();
            containerData.id = BoxDataEditorUtils.DrawColorSelection(containerData.id, GameConstants.ColorID);
            EditorGUILayout.EndHorizontal();

            // Apply changes to the SerializedObject

            GUILayout.Space(10);
        }
    }
}