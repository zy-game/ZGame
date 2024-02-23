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
            if (config.referenceAssemblyList is null)
            {
                config.references = new();
                config.referenceAssemblyList = new();
            }

            GUILayout.BeginVertical(EditorStyles.helpBox);

            config.title = EditorGUILayout.TextField("游戏名", config.title);

            if (config.path.IsNullOrEmpty() is false && config.assembly == null)
            {
                config.assembly = AssetDatabase.LoadAssetAtPath<AssemblyDefinitionAsset>(config.path);
            }

            config.version = EditorGUILayout.TextField("版本", config.version);
            config.mode = (CodeMode)EditorGUILayout.EnumPopup("模式", config.mode);

            var resList = BuilderConfig.instance.packages.Select(x => x.name).ToList();
            int last = resList.FindIndex(x => x == config.module);
            int curIndex = EditorGUILayout.Popup("资源包", last, resList.ToArray());
            if (curIndex >= 0 && curIndex < resList.Count)
            {
                config.module = resList[curIndex];
            }

            config.assembly = (AssemblyDefinitionAsset)EditorGUILayout.ObjectField("Assembly", config.assembly, typeof(AssemblyDefinitionAsset), false);
            GUILayout.BeginHorizontal();
            resList = config.channels?.Select(x => x.title).ToList();
            last = resList.FindIndex(x => x == config.currentChannel);
            curIndex = EditorGUILayout.Popup("当前渠道", last, resList.ToArray());
            if (curIndex >= 0 && curIndex < resList.Count)
            {
                config.currentChannel = resList[curIndex];
            }


            GUILayout.EndHorizontal();
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Reference Assembly", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            if (config.assembly != null)
            {
                config.entryName = config.assembly.name;
            }

            if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.ADD_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE))
            {
                config.references.Add(string.Empty);
                config.referenceAssemblyList.Add(null);
                BasicConfig.OnSave();
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(5);
            for (int j = config.referenceAssemblyList.Count - 1; j >= 0; j--)
            {
                GUILayout.BeginHorizontal(ZStyle.ITEM_BACKGROUND_STYLE);

                config.referenceAssemblyList[j] = (AssemblyDefinitionAsset)EditorGUILayout.ObjectField("Element " + j, config.referenceAssemblyList[j], typeof(AssemblyDefinitionAsset), false, GUILayout.Width(300));
                if (config.referenceAssemblyList[j] != null)
                {
                    config.references[j] = config.referenceAssemblyList[j].name;
                }

                GUILayout.FlexibleSpace();
                if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.DELETE_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE))
                {
                    config.references.RemoveAt(j);
                    config.referenceAssemblyList.RemoveAt(j);
                    BasicConfig.OnSave();
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();

            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Channels", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            if (config.channels == null)
            {
                config.channels = new();
            }

            if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.ADD_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE))
            {
                config.channels.Add(new ChannelPackageOptions());
                BasicConfig.OnSave();
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(5);
            for (int j = config.channels.Count - 1; j >= 0; j--)
            {
                GUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.BeginHorizontal();
                config.channels[j].title = EditorGUILayout.TextField("Channel Name", config.channels[j].title);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.DELETE_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE))
                {
                    config.channels.RemoveAt(j);
                    BasicConfig.OnSave();
                }

                GUILayout.EndHorizontal();
                config.channels[j].packageName = EditorGUILayout.TextField("Package Name", config.channels[j].packageName);
                config.channels[j].appName = EditorGUILayout.TextField("App Name", config.channels[j].appName);
                config.channels[j].icon = (EditorGUILayout.ObjectField("Channel Icon", config.channels[j].icon, typeof(Texture2D), false) as Texture2D);
                config.channels[j].splash = (EditorGUILayout.ObjectField("Channel Splash", config.channels[j].splash, typeof(Sprite), false) as Sprite);
                GUILayout.EndVertical();
            }

            GUILayout.EndVertical();

            GUILayout.EndVertical();
        }
    }
}