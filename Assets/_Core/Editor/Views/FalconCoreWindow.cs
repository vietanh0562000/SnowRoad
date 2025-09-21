using System;
using System.Collections;
using System.Collections.Generic;
using BasePuzzle.Core.Editor.Models;
using BasePuzzle.Core.Editor.Repositories;
using BasePuzzle.Core.Editor.Services;
using BasePuzzle.Core.Editor.Utils;
using BasePuzzle.Core.Scripts.Repositories;
using BasePuzzle.Core.Scripts.Utils;
using Unity.CodeEditor;
using UnityEditor;

namespace BasePuzzle.Core.Editor.Views
{
    using BasePuzzle.Core.Editor.Models;
    using BasePuzzle.Core.Editor.Repositories;
    using BasePuzzle.Core.Editor.Services;
    using BasePuzzle.Core.Editor.Utils;
    using BasePuzzle.Core.Scripts.Repositories;
    using BasePuzzle.Core.Scripts.Utils;

    public class FalconCoreWindow : EditorWindow
    {
        [MenuItem("Falcon/FalconCore/Refresh")]
        public static void ShowWindow()
        {
            new EditorSequence(Refresh()).Start();
        }
        
        [MenuItem("Falcon/FalconCore/DebugLog/Enable")]
        public static void EnableDebugLog()
        {
            DefineSymbols.Add("FALCON_LOG_DEBUG");
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
                
            EditorUtility.RequestScriptReload();
            CodeEditor.CurrentEditor.Initialize(CodeEditor.CurrentEditorInstallation);
        }
        
        [MenuItem("Falcon/FalconCore/DebugLog/Disable")]
        public static void DisableDebugLog()
        {
            DefineSymbols.Remove("FALCON_LOG_DEBUG");
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
                
            EditorUtility.RequestScriptReload();
            CodeEditor.CurrentEditor.Initialize(CodeEditor.CurrentEditorInstallation);
        }
        
        [MenuItem("Falcon/FalconCore/ClearData")]
        public static void ClearData()
        {
            new FFile(FDataPool.DataFile).Save(new Dictionary<String, String>());
        }

        private static IEnumerator Refresh()
        {
            var a = new FalconCoreInstallResponder();
            
            FPlugin plugin;
            while (!FPluginRepo.TryGet("FalconCore", out plugin))
            {
                yield return null;
            }
            yield return a.OnPluginInstalled(plugin.InstalledDirectory);
        }
    }
}