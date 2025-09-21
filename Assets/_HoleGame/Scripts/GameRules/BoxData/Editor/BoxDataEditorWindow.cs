#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;
using HoleBox;

public class BoxDataEditorWindow : EditorWindow
{
    // private BoxData          currentBoxData; // Reference to the object being edited
    // private SerializedObject serializedBoxData; // Persistent SerializedObject
    // private Action<BoxData>  onSaveCallback;
    //
    // /// <summary>
    // /// Opens the Box Data Editor Window.
    // /// </summary>
    // public static void ShowBoxEditor(BoxData box, Action<BoxData> onSave)
    // {
    //     var window = GetWindow<BoxDataEditorWindow>("Box Data Editor");
    //     window.currentBoxData    = box;
    //     window.serializedBoxData = new SerializedObject(box); // Create SerializedObject once
    //     window.onSaveCallback    = onSave;
    //     window.Show();
    // }
    //
    // private void OnGUI()
    // {
    //     // Prevent updates if no BoxData is loaded
    //     if (currentBoxData == null || serializedBoxData == null)
    //     {
    //         EditorGUILayout.HelpBox("No BoxData object to edit!", MessageType.Warning);
    //         return;
    //     }
    //     
    //     serializedBoxData.Update(); // Update SerializedObject state
    //     BoxDataEditor.DrawBoxDataEditor(serializedBoxData);
    //
    //     if (currentBoxData is HoleBoxData holeBoxData)
    //     {
    //         HoleDataEditor.DrawHoleBoxDataEditor(serializedBoxData, holeBoxData);
    //     }
    //     
    //     GUILayout.Label($"Edit {currentBoxData.GetType().Name} Data", EditorStyles.boldLabel);
    //
    //     // Draw serialized fields
    //     SerializedProperty property = serializedBoxData.GetIterator();
    //     property.NextVisible(true); // Skip the object header
    //     do
    //     {
    //         EditorGUILayout.PropertyField(property, true); // Draw each property
    //     } while (property.NextVisible(false));
    //
    //     serializedBoxData.ApplyModifiedProperties(); // Apply changes to the object
    //
    //     GUILayout.Space(10);
    //
    //     // Save Button
    //     if (GUILayout.Button("Save"))
    //     {
    //         onSaveCallback?.Invoke(currentBoxData); // Trigger save callback
    //         Close();
    //     }
    //
    //     // Cancel Button
    //     if (GUILayout.Button("Cancel"))
    //     {
    //         Close();
    //     }
    // }
}
#endif