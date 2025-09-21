#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace HoleBox
{
    [CustomEditor(typeof(HoleBoxData), true)]
    public class HoleDataEditor : BoxDataEditor
    {
        // private SerializedProperty keyHoleProperty;
        //
        // protected override void OnEnable()
        // {
        //     base.OnEnable();
        //     keyHoleProperty = serializedObject.FindProperty("keyPos");
        //
        //     CreateLevelEditor.SelectingKeyHole = false;
        // }
        //
        // public override void OnInspectorGUI()
        // {
        //     serializedObject.Update();
        //
        //     // Draw inherited fields using the base implementation
        //     base.OnInspectorGUI();
        //
        //     // Draw specific UI for HoleBoxData
        //     DrawHoleBoxDataEditor(serializedObject, (HoleBoxData)target);
        //
        //     serializedObject.ApplyModifiedProperties();
        // }
        //
        // /// <summary>
        // /// Draws custom editor UI for HoleBoxData.
        // /// </summary>
        // public static void DrawHoleBoxDataEditor(SerializedObject serializedObject, HoleBoxData currentHole)
        // {
        //     EditorGUILayout.Space();
        //     EditorGUILayout.LabelField("Key Hole Settings", EditorStyles.boldLabel);
        //
        //     // Ensure `CreateLevel` instance is available in the scene
        //     CreateLevel createLevel = Object.FindObjectOfType<CreateLevel>();
        //     if (createLevel == null)
        //     {
        //         EditorGUILayout.HelpBox("No CreateLevel instance exists in the current scene.", MessageType.Warning);
        //         return;
        //     }
        //
        //     // Get all HoleBoxData objects except the current one
        //     var holes = createLevel.Boxes;
        //     if (holes.Count == 0)
        //     {
        //         EditorGUILayout.HelpBox("No holes available to select as a key.", MessageType.Info);
        //         return;
        //     }
        //
        //     // Build dropdown list for Holes excluding currentHole
        //     var keyHoleNames  = new string[holes.Count];
        //     int selectedIndex = -1;
        //
        //     for (int i = 0; i < holes.Count; i++)
        //     {
        //         if (holes[i] is not HoleBoxData)
        //         {
        //             continue;
        //         }
        //
        //         if (holes[i] == currentHole)
        //         {
        //             keyHoleNames[i] = "[Current Hole]";
        //         }
        //         else
        //         {
        //             keyHoleNames[i] = $"Hole ID: {holes[i].id}";
        //             if (holes[i].InsideBox(currentHole.keyPos))
        //             {
        //                 selectedIndex = i; // Highlight currently selected keyHole
        //             }
        //         }
        //     }
        //
        //     // Add Dropdown to select keyHole
        //     int newSelectedIndex = EditorGUILayout.Popup("Key Pos", selectedIndex, keyHoleNames);
        //
        //     // Apply changes if a different keyHole is selected
        //     if (newSelectedIndex >= 0 && newSelectedIndex < holes.Count && holes[newSelectedIndex] is HoleBoxData selectedHole && selectedHole != currentHole)
        //     {
        //         var keyPosProperty = serializedObject.FindProperty("keyPos");
        //         if (keyPosProperty != null)
        //         {
        //             keyPosProperty.vector2IntValue = selectedHole.position;
        //             serializedObject.ApplyModifiedProperties();
        //         }
        //     }
        //
        //     // Add Scene View selection button
        //     if (GUILayout.Button("Select Key Hole in Scene"))
        //     {
        //         CreateLevelEditor.SelectingKeyHole = true;
        //         CreateLevelEditor.TargetHoleData   = currentHole; // Notify editor about the current selection
        //     }
        // }
    }
}
#endif