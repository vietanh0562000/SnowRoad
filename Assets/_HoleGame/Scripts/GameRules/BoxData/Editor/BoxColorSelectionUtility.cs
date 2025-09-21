using System;
using UnityEditor;
using UnityEngine;

public static class BoxDataEditorUtils
{
    /// <summary>
    /// Displays the color selection toggles and returns the updated ID.
    /// </summary>
    /// <param name="currentId">The current ID of the color.</param>
    /// <param name="colors">The available colors.</param>
    /// <returns>The newly selected ID.</returns>
    public static int DrawColorSelection(int currentId, Color[] colors)
    {
        int selectedId = currentId;

        for (int i = 0; i < colors.Length; i++)
        {
            EditorGUILayout.BeginHorizontal();

            // Display the color as a square box
            GUIStyle colorStyle = new GUIStyle(GUI.skin.box);
            colorStyle.normal.background = MakeTex(16, 16, colors[i]);
            GUILayout.Box(GUIContent.none, colorStyle, GUILayout.Width(25), GUILayout.Height(25));

            // Add a toggle next to the color
            bool toggle = GUILayout.Toggle(selectedId == i + 1, String.Empty);
            if (toggle && selectedId != i + 1)
            {
                selectedId = i + 1; // Update the selected ID
            }

            EditorGUILayout.EndHorizontal();
        }

        return selectedId;
    }

    /// <summary>
    /// Helper method to create a solid texture for a given color.
    /// </summary>
    private static Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; i++)
        {
            pix[i] = col;
        }

        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }

    public static int[] MatrixSizes   => new[] { 8, 10, 12, 14, 16, 18, 20, 22, 24, 26, 28, 30, 32, 34, 36, 38, 40 };
    public static int[] CapacitySizes => new[] { 4, 8, 16, 32, 64 };
}