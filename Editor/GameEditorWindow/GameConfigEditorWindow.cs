using System.Collections.Generic;
using System.Linq;
using HybridCLR.Editor.Settings;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using ZGame.Editor.Command;
using ZGame.Editor.ResBuild.Config;

namespace ZGame.Editor
{
    [GameSubEditorWindowOptions("游戏配置", typeof(RuntimeEditorWindow))]
    public class GameConfigEditorWindow : GameSubEditorWindow
    {
        public override void OnGUI()
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
         
            GameConfig.instance.title = EditorGUILayout.TextField("游戏名", GameConfig.instance.title);
            if (GameConfig.instance.path.IsNullOrEmpty() is false && GameConfig.instance.assembly == null)
            {
                GameConfig.instance.assembly = AssetDatabase.LoadAssetAtPath<AssemblyDefinitionAsset>(GameConfig.instance.path);
            }

            GameConfig.instance.version = EditorGUILayout.TextField("版本", GameConfig.instance.version);
            var mode = (CodeMode)EditorGUILayout.EnumPopup("模式", GameConfig.instance.mode);
            if (mode != GameConfig.instance.mode)
            {
                GameConfig.instance.mode = mode;
                List<AssemblyDefinitionAsset> hotfixAssemblies = new List<AssemblyDefinitionAsset>();
                if (GameConfig.instance.mode == CodeMode.Hotfix)
                {
                    if (GameConfig.instance.path.IsNullOrEmpty() is false && GameConfig.instance.assembly == null)
                    {
                        GameConfig.instance.assembly = AssetDatabase.LoadAssetAtPath<AssemblyDefinitionAsset>(GameConfig.instance.path);
                    }

                    hotfixAssemblies.Add(GameConfig.instance.assembly);
                }

                HybridCLRSettings.Instance.hotUpdateAssemblyDefinitions = hotfixAssemblies.ToArray();
                HybridCLRSettings.Instance.enable = hotfixAssemblies.Count > 0;
                HybridCLRSettings.Save();
            }

           

            GameConfig.instance.assembly = (AssemblyDefinitionAsset)EditorGUILayout.ObjectField("Assembly", GameConfig.instance.assembly, typeof(AssemblyDefinitionAsset), false);
            if (GameConfig.instance.assembly != null)
            {
                GameConfig.instance.path = AssetDatabase.GetAssetPath(GameConfig.instance.assembly);
            }

        
            GUILayout.EndVertical();
        }

        public override void SaveChanges()
        {
            GameConfig.Save();
        }
    }
}