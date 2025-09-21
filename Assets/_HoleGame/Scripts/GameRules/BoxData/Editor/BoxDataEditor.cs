#if UNITY_EDITOR
using HoleBox;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BoxData), true)]
public class BoxDataEditor : Editor
{
//     private Color[]          idColors => GameConstants.ColorID; // Available colors
//     private SerializedObject serializedBoxData; // Reference to SerializedObject
//
//     protected virtual void OnEnable()
//     {
//         // Initialize SerializedObject for the target
//         serializedBoxData = serializedObject;
//     }
//
//     public override void OnInspectorGUI()
//     {
//         // Call the shared DrawBoxDataEditor to render the custom content
//         DrawBoxDataEditor(serializedBoxData);
//     }
//
//     /// <summary>
//     /// Shared method to draw the BoxData editor, reusable in external windows.
//     /// </summary>
//     public static void DrawBoxDataEditor(SerializedObject serializedBoxData)
//     {
//         // Update SerializedObject to reflect current data
//         serializedBoxData.Update();
//
//         // Color selection toggles
//         GUILayout.Space(10);
//         GUILayout.Label("Select a Color (ID):");
//         SerializedProperty idProperty = serializedBoxData.FindProperty("id");
//
// // Display colors horizontally
//         EditorGUILayout.BeginHorizontal();
//         idProperty.intValue = BoxDataEditorUtils.DrawColorSelection(idProperty.intValue, GameConstants.ColorID);
//         EditorGUILayout.EndHorizontal();
//
//         // Apply changes to the SerializedObject
//         serializedBoxData.ApplyModifiedProperties();
//     }
}
#endif