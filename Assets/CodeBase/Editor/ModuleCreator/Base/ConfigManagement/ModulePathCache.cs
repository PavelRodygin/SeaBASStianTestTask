#if UNITY_EDITOR
using CodeBase.Editor.ModuleCreator.Configs;
using UnityEditor;
using UnityEngine;

namespace CodeBase.Editor.ModuleCreator.Base.ConfigManagement
{
    //TODO Cache the found config. Read once. If not found - log error. If found - save to field
    [InitializeOnLoad]
    public static class ModulePathCache
    {
        public static string ScriptsFolderName { get; private set; }
        public static string ViewsFolderName { get; private set; }
        public static string ScenesFolderName { get; private set; }

        static ModulePathCache()
        {
            EditorApplication.delayCall += LoadPaths;   //To prevent call before Unity Scene Initialization
        }

        public static void RefreshPaths()
        {
            // Debug.Log("ModulePathCache updates paths...");
            LoadPaths();
        }

        // private static void LoadPaths()
        // {
        //     var configPaths = AssetDatabase.FindAssets($"t:{nameof(ModuleStructureConfig)}");
        //
        //     if (configPaths.Length is 0 or > 1)
        //         Debug.LogError($"There must be only one {nameof(ModuleStructureConfig)} config file in the project");
        //     
        //     var config = AssetDatabase.LoadAssetAtPath<ModuleStructureConfig>(configPaths[0]);
        //
        //     if (config != null)
        //     {
        //         ScriptsFolderName = config.scriptsFolderName;
        //         ViewsFolderName = config.viewsFolderName;
        //         ScenesFolderName = config.scenesFolderName;
        //     }
        //     else
        //     {
        //         // Debug.LogError("ModuleStructureConfig not found! Using default values.");
        //     }
        // } 
        
        private static void LoadPaths()
        {
            var config = AssetDatabase.LoadAssetAtPath<ModuleStructureConfig>(
                "Assets/Configs/ModuleCreatorSettings/ModuleStructureConfig.asset");
        
            if (config != null)
            {
                ScriptsFolderName = config.scriptsFolderName;
                ViewsFolderName = config.viewsFolderName;
                ScenesFolderName = config.scenesFolderName;
            }
            else
            {
                Debug.LogWarning("ModuleStructureConfig not found! Using default values.");
                ScriptsFolderName = "Scripts";
                ViewsFolderName = "Views";
                ScenesFolderName = "Scenes";
            }
        }
    }
}
#endif