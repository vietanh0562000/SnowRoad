#if UNITY_EDITOR
namespace HoleBox
{
    using HoleBox;
    using UnityEditor;
    using UnityEngine;

    public partial class CreateLevelEditor : Editor
    {
        public static bool        SelectingKeyHole = false; // Whether keyHole selection mode is active
        public static HoleBoxData TargetHoleData; // Target HoleBoxData being edited

        private void HandleKeyHoleSelection()
        {
            Event e = Event.current;

            if (e.type == EventType.MouseDown && e.button == 0) // Left Mouse Click
            {
                // Get the clicked position in the scene
                Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hitPosition))
                {
                    Vector2Int gridPosition = WorldToGrid(hitPosition.point); // Convert to grid coordinates

                    // Find the `BoxData` at that position
                    var createLevel  = (CreateLevel)target;
                    var selectedHole = FindBoxAtCell(gridPosition) as HoleBoxData;

                    if (selectedHole != null && selectedHole != TargetHoleData)
                    {
                        // Set the selected hole as the keyHole
                        TargetHoleData.keyPos = selectedHole.position;

                        // Exit selection mode
                        SelectingKeyHole = false;
                        TargetHoleData   = null;

                        // Save changes
                        EditorUtility.SetDirty(createLevel);
                        RePaintAll();
                    }
                    else
                    {
                        // Display a warning if no valid selection
                        Debug.LogWarning("No valid hole selected or the same hole was clicked.");
                    }

                    e.Use(); // Mark the event as used
                }
            }
        }
    }
}
#endif