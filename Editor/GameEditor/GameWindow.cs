using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HybridCLR.Editor;
using HybridCLR.Editor.Commands;
using HybridCLR.Editor.Settings;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using ZGame.Config;
using ZGame.Editor.Command;
using ZGame.Editor.LinkerEditor;
using ZGame.Editor.ResBuild;
using ZGame.Editor.ResBuild.Config;
using ZGame.Resource.Config;
using Object = UnityEngine.Object;

namespace ZGame.Editor
{
    [PageConfig("游戏设置", typeof(RuntimeEditorWindow))]
    public class GameWindow : ToolbarScene
    {
        public override void SearchRightDrawing()
        {
            if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.ADD_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE))
            {
                BasicConfig.instance.entries.Add(new EntryConfig());
            }
        }

        public override void OnDrawingHeaderRight(object userData)
        {
            if (userData is not EntryConfig config)
            {
                return;
            }

            if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.DELETE_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE))
            {
                BasicConfig.instance.entries.Remove(config);
            }


            if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.PLAY_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE))
            {
                ZGame.CommandManager.OnExecuteCommand<SubGameBuildCommand>(config, true);
            }
        }

        public override void OnGUI()
        {
            for (int i = 0; i < BasicConfig.instance.entries.Count; i++)
            {
                EntryConfig config = BasicConfig.instance.entries[i];
                config.isOn = OnBeginHeader(config.title, config.isOn, config);
                if (config.isOn)
                {
                    EditorGUI.BeginChangeCheck();
                    OnShowGameConfig(config);
                    if (EditorGUI.EndChangeCheck())
                    {
                        BasicConfig.OnSave();
                    }
                }
            }
        }

        private void OnShowGameConfig(EntryConfig config)
        {
            if (config.references is null)
            {
                config.references = new();
            }

            GUILayout.BeginVertical(EditorStyles.helpBox);

            config.title = EditorGUILayout.TextField("游戏名", config.title);

            if (config.path.IsNullOrEmpty() is false && config.assembly == null)
            {
                config.assembly = AssetDatabase.LoadAssetAtPath<AssemblyDefinitionAsset>(config.path);
            }

            config.mode = (CodeMode)EditorGUILayout.EnumPopup("模式", config.mode);
            config.args = EditorGUILayout.TextField("参数", config.args);
            var resList = BuilderConfig.instance.packages.Select(x => x.name).ToList();
            int last = resList.FindIndex(x => x == config.module);
            int curIndex = EditorGUILayout.Popup("资源包", last, resList.ToArray());
            if (curIndex >= 0 && curIndex < resList.Count)
            {
                config.module = resList[curIndex];
            }

            config.assembly = (AssemblyDefinitionAsset)EditorGUILayout.ObjectField("Assembly", config.assembly, typeof(AssemblyDefinitionAsset), false);
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Reference Assembly", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            if (config.assembly != null)
            {
                config.entryName = config.assembly.name;
            }

            if (config.references is null)
            {
                config.references = new List<string>();
            }

            if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.ADD_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE))
            {
                config.references.Add(string.Empty);
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(5);
            for (int j = config.references.Count - 1; j >= 0; j--)
            {
                GUILayout.BeginHorizontal(ZStyle.ITEM_BACKGROUND_STYLE);


                config.references[j] = EditorGUILayout.TextField("Element " + j, config.references[j], GUILayout.Width(500));
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.DELETE_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE))
                {
                    config.references.RemoveAt(j);
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
            GUILayout.EndVertical();
        }
    }
}