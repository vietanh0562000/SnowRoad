
namespace PuzzleGames
{
    public static class FileBrowser
    {
        public static string OpenFile(string name, string path, string extension)
        {
#if UNITY_EDITOR
            return UnityEditor.EditorUtility.OpenFilePanel(name, path, extension);
#endif
            return string.Empty;
        }

        public static string SaveFile(string title, string path, string name, string extension)
        {
#if UNITY_EDITOR
            return UnityEditor.EditorUtility.SaveFilePanel(title, path, name, extension);
#endif
            return string.Empty;
        }
    }
}