﻿using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using HybridCLR.Editor.Settings;
using NUnit.Framework;
using Unity.Collections;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using ZGame.Config;
using ZGame.Editor.Command;
using ZGame.Editor.ResBuild;
using ZGame.Editor.ResBuild.Config;
using ZGame.Resource.Config;

namespace ZGame.Editor
{
    [PageConfig("全局配置", null, false, typeof(BasicConfig))]
    public class RuntimeEditorWindow : ToolbarScene
    {
        private bool show1, show2, show3;
        private List<string> resList;

        public override void OnEnable(params object[] args)
        {
            resList = BuilderConfig.instance.packages?.Select(x => x.name).ToList();
        }

        public override void OnGUI()
        {
            OnShowBasicConfigDrawing();
            OnShowGameConfigDrawing();
            OnShowAddressConfigDrawing();
            if (Event.current.type == EventType.KeyDown && Event.current.control && Event.current.keyCode == KeyCode.S)
            {
                OSSConfig.OnSave();
                BasicConfig.OnSave();
            }
        }

        private void OnShowBasicConfigDrawing()
        {
            int last = 0;
            int curIndex = 0;
            GUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("基础配置", EditorStyles.boldLabel);
            BasicConfig.instance.companyName = EditorGUILayout.TextField("公司名称", BasicConfig.instance.companyName);
            BasicConfig.instance.apkUrl = EditorGUILayout.TextField("安装包下载地址", BasicConfig.instance.apkUrl);
            NativeLeakDetection.Mode = (NativeLeakDetectionMode)EditorGUILayout.EnumPopup("Enable Stack Trace", NativeLeakDetection.Mode);
            BasicConfig.instance.isDebug = EditorGUILayout.Toggle("Enable Debug", BasicConfig.instance.isDebug);
            GUILayout.BeginHorizontal();
            last = BasicConfig.instance.address.FindIndex(x => x.title == BasicConfig.instance.curAddressName);
            curIndex = EditorGUILayout.Popup("服务器地址", last, BasicConfig.instance.address.Select(x => x.title).ToArray());
            GUILayout.EndHorizontal();
            if (curIndex >= 0 && curIndex < BasicConfig.instance.address.Count && last != curIndex)
            {
                BasicConfig.instance.curAddressName = BasicConfig.instance.address[curIndex].title;
            }

            GUILayout.BeginHorizontal();
            BasicConfig.instance.resMode = (ResourceMode)EditorGUILayout.EnumPopup("资源模式", BasicConfig.instance.resMode);
            if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.SETTING_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE, GUILayout.ExpandWidth(false)))
            {
                ToolsWindow.SwitchScene<ResSetting>();
            }

            GUILayout.EndHorizontal();
            BasicConfig.instance.resTimeout = EditorGUILayout.Slider("包检查间隔时间", BasicConfig.instance.resTimeout, 10, byte.MaxValue);
            last = OSSConfig.instance.ossList.FindIndex(x => x.title == OSSConfig.instance.seletion);
            curIndex = EditorGUILayout.Popup("资源服务器地址", last, OSSConfig.instance.ossList.Select(x => x.title).ToArray());

            if (curIndex >= 0 && curIndex < OSSConfig.instance.ossList.Count && last != curIndex)
            {
                OSSConfig.instance.seletion = OSSConfig.instance.ossList[curIndex].title;
                OSSConfig.OnSave();
            }

            GUILayout.EndVertical();
        }

        private void OnShowGameConfigDrawing()
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("游戏设置", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            if (BasicConfig.instance.curGame is not null && BasicConfig.instance.curGame.mode == CodeMode.Hotfix)
            {
                if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.PLAY_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE, GUILayout.ExpandWidth(false)))
                {
                    GenericMenu menu = new GenericMenu();
                    menu.AddItem(new GUIContent("Build Hotfix Assets"), false, () => BuildGameChannelCommand.Executer(null));
                    if (BasicConfig.instance.curGame.channels != null && BasicConfig.instance.curGame.channels.Count > 0)
                    {
                        foreach (var VARIABLE in BasicConfig.instance.curGame.channels)
                        {
                            menu.AddItem(new GUIContent("Build Install Package/" + VARIABLE.title), false, () => { BuildGameChannelCommand.Executer(VARIABLE); });
                        }
                    }

                    menu.ShowAsContext();
                }
            }

            GUILayout.EndHorizontal();
            BasicConfig.instance.curGame.title = EditorGUILayout.TextField("游戏名", BasicConfig.instance.curGame.title);
            if (BasicConfig.instance.curGame.path.IsNullOrEmpty() is false && BasicConfig.instance.curGame.assembly == null)
            {
                BasicConfig.instance.curGame.assembly = AssetDatabase.LoadAssetAtPath<AssemblyDefinitionAsset>(BasicConfig.instance.curGame.path);
            }

            BasicConfig.instance.curGame.version = EditorGUILayout.TextField("版本", BasicConfig.instance.curGame.version);
            var mode = (CodeMode)EditorGUILayout.EnumPopup("模式", BasicConfig.instance.curGame.mode);
            if (mode != BasicConfig.instance.curGame.mode)
            {
                BasicConfig.instance.curGame.mode = mode;
                List<AssemblyDefinitionAsset> hotfixAssemblies = new List<AssemblyDefinitionAsset>();
                if (BasicConfig.instance.curGame.mode == CodeMode.Hotfix)
                {
                    if (BasicConfig.instance.curGame.path.IsNullOrEmpty() is false && BasicConfig.instance.curGame.assembly == null)
                    {
                        BasicConfig.instance.curGame.assembly = AssetDatabase.LoadAssetAtPath<AssemblyDefinitionAsset>(BasicConfig.instance.curGame.path);
                    }

                    hotfixAssemblies.Add(BasicConfig.instance.curGame.assembly);
                }

                HybridCLRSettings.Instance.hotUpdateAssemblyDefinitions = hotfixAssemblies.ToArray();
                HybridCLRSettings.Instance.enable = hotfixAssemblies.Count > 0;
                HybridCLRSettings.Save();
            }

            var resList = BuilderConfig.instance.packages.Select(x => x.name).ToList();
            int last = resList.FindIndex(x => x == BasicConfig.instance.curGame.module);
            int curIndex = EditorGUILayout.Popup("资源包", last, resList.ToArray());
            if (curIndex >= 0 && curIndex < resList.Count)
            {
                BasicConfig.instance.curGame.module = resList[curIndex];
            }

            BasicConfig.instance.curGame.assembly = (AssemblyDefinitionAsset)EditorGUILayout.ObjectField("Assembly", BasicConfig.instance.curGame.assembly, typeof(AssemblyDefinitionAsset), false);
            if (BasicConfig.instance.curGame.assembly != null)
            {
                BasicConfig.instance.curGame.path = AssetDatabase.GetAssetPath(BasicConfig.instance.curGame.assembly);
            }

            last = BasicConfig.instance.curGame.channels.FindIndex(x => x.title == BasicConfig.instance.curGame.currentChannel);
            curIndex = EditorGUILayout.Popup("当前渠道", last, BasicConfig.instance.curGame.channels.Select(x => x.title).ToArray());
            if (curIndex >= 0 && curIndex < BasicConfig.instance.curGame.channels.Count && last != curIndex)
            {
                BasicConfig.instance.curGame.currentChannel = BasicConfig.instance.curGame.channels[curIndex].title;
                BuildGameChannelCommand.SetPlayerSetting(BasicConfig.instance.curGame.currentChannelOptions, BasicConfig.instance.curGame.version);
            }

            // GUILayout.BeginVertical(EditorStyles.helpBox);
            // GUILayout.BeginHorizontal();
            // BasicConfig.instance.curEntry.isShowReferences = EditorGUILayout.Foldout(BasicConfig.instance.curEntry.isShowReferences, "Reference Assembly");
            // GUILayout.FlexibleSpace();
            // if (BasicConfig.instance.curEntry.assembly != null)
            // {
            //     BasicConfig.instance.curEntry.entryName = BasicConfig.instance.curEntry.assembly.name;
            // }
            //
            // if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.ADD_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE))
            // {
            //     BasicConfig.instance.curEntry.references.Add(string.Empty);
            //     BasicConfig.instance.curEntry.referenceAssemblyList.Add(null);
            //     BasicConfig.OnSave();
            // }
            //
            // GUILayout.EndHorizontal();
            // GUILayout.Space(5);
            // if (BasicConfig.instance.curEntry.isShowReferences)
            // {
            //     for (int j = BasicConfig.instance.curEntry.referenceAssemblyList.Count - 1; j >= 0; j--)
            //     {
            //         GUILayout.BeginHorizontal(ZStyle.ITEM_BACKGROUND_STYLE);
            //         BasicConfig.instance.curEntry.referenceAssemblyList[j] = (AssemblyDefinitionAsset)EditorGUILayout.ObjectField("Element " + j, BasicConfig.instance.curEntry.referenceAssemblyList[j], typeof(AssemblyDefinitionAsset), false, GUILayout.Width(300));
            //         if (BasicConfig.instance.curEntry.referenceAssemblyList[j] != null)
            //         {
            //             BasicConfig.instance.curEntry.references[j] = BasicConfig.instance.curEntry.referenceAssemblyList[j].name;
            //         }
            //
            //         GUILayout.FlexibleSpace();
            //         if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.DELETE_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE))
            //         {
            //             BasicConfig.instance.curEntry.references.RemoveAt(j);
            //             BasicConfig.instance.curEntry.referenceAssemblyList.RemoveAt(j);
            //             BasicConfig.OnSave();
            //         }
            //
            //         GUILayout.EndHorizontal();
            //     }
            // }
            //
            //
            // GUILayout.EndVertical();
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.BeginHorizontal();
            BasicConfig.instance.curGame.isShowChannels = EditorGUILayout.Foldout(BasicConfig.instance.curGame.isShowChannels, "Channels");
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.ADD_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE))
            {
                BasicConfig.instance.curGame.channels.Add(new ChannelOptions());
                BasicConfig.OnSave();
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(5);
            if (BasicConfig.instance.curGame.isShowChannels)
            {
                for (int j = BasicConfig.instance.curGame.channels.Count - 1; j >= 0; j--)
                {
                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    GUILayout.BeginHorizontal();
                    // BasicConfig.instance.curEntry.channels[j].title = EditorGUILayout.TextField("Channel Name", BasicConfig.instance.curEntry.channels[j].title);
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.DELETE_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE))
                    {
                        BasicConfig.instance.curGame.channels.RemoveAt(j);
                        BasicConfig.OnSave();
                    }

                    GUILayout.EndHorizontal();
                    BasicConfig.instance.curGame.channels[j].title = EditorGUILayout.TextField("名称", BasicConfig.instance.curGame.channels[j].title);

                    List<string> title = LanguageConfig.instance.languageTempletes.Select(x => x.name).ToList();
                    List<string> filters = LanguageConfig.instance.languageTempletes.Select(x => x.filter).ToList();
                    last = filters.FindIndex(x => x == BasicConfig.instance.curGame.channels[j].language);
                    curIndex = EditorGUILayout.Popup("语言", last, title.ToArray());
                    if (curIndex != last)
                    {
                        BasicConfig.instance.curGame.channels[j].language = LanguageConfig.instance.languageTempletes[curIndex].filter;
                    }

                    BasicConfig.instance.curGame.channels[j].args = EditorGUILayout.TextField("Args", BasicConfig.instance.curGame.channels[j].args);
                    BasicConfig.instance.curGame.channels[j].packageName = EditorGUILayout.TextField("Package Name", BasicConfig.instance.curGame.channels[j].packageName);
                    BasicConfig.instance.curGame.channels[j].appName = EditorGUILayout.TextField("App Name", BasicConfig.instance.curGame.channels[j].appName);
                    BasicConfig.instance.curGame.channels[j].icon = (EditorGUILayout.ObjectField("Channel Icon", BasicConfig.instance.curGame.channels[j].icon, typeof(Texture2D), false) as Texture2D);
                    BasicConfig.instance.curGame.channels[j].splash = (EditorGUILayout.ObjectField("Channel Splash", BasicConfig.instance.curGame.channels[j].splash, typeof(Sprite), false) as Sprite);
                    GUILayout.EndVertical();
                }
            }

            GUILayout.EndVertical();
            GUILayout.EndVertical();
        }

        private void OnShowAddressConfigDrawing()
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Address", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.ADD_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE))
            {
                BasicConfig.instance.address.Add(new IPConfig());
            }

            GUILayout.EndHorizontal();
            for (int i = 0; i < BasicConfig.instance.address.Count; i++)
            {
                IPConfig config = BasicConfig.instance.address[i];
                config.isOn = OnBeginHeader(config.title, config.isOn, config);
                if (config.isOn)
                {
                    config.title = EditorGUILayout.TextField("别名", config.title);
                    config.address = EditorGUILayout.TextField("IP", config.address);
                    config.port = EditorGUILayout.IntField("端口", config.port);
                }
            }

            GUILayout.EndVertical();
        }
    }
}