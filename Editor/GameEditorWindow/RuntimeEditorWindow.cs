﻿// using System.Collections.Generic;
// using System.Linq;
// using Cysharp.Threading.Tasks;
// using HybridCLR.Editor.Settings;
// using NUnit.Framework;
// using Unity.Collections;
// using UnityEditor;
// using UnityEditorInternal;
// using UnityEngine;
// using ZGame.Config;
// using ZGame.Editor.Command;
// using ZGame.Editor.ResBuild;
// using ZGame.Editor.ResBuild.Config;
// using ZGame.VFS;
//
// namespace ZGame.Editor
// {
//     [GameSubEditorWindowOptions("全局配置", null, false)]
//     public class RuntimeEditorWindow : GameSubEditorWindow
//     {
//         private bool show1, show2, show3;
//
//         public override void OnEnable(params object[] args)
//         {
//         }
//
//         public override void SaveChanges()
//         {
//             GameConfig.Save();
//             ResConfig.Save();
//             IPConfig.Save();
//         }
//
//         public override void OnGUI()
//         {
//             int last = 0;
//             int curIndex = 0;
//             GUILayout.BeginVertical(EditorStyles.helpBox);
//             EditorGUILayout.LabelField("基础配置", EditorStyles.boldLabel);
//             GameConfig.instance.companyName = EditorGUILayout.TextField("公司名称", GameConfig.instance.companyName);
//             GameConfig.instance.apkUrl = EditorGUILayout.TextField("安装包下载地址", GameConfig.instance.apkUrl);
//             NativeLeakDetection.Mode = (NativeLeakDetectionMode)EditorGUILayout.EnumPopup("Enable Stack Trace", NativeLeakDetection.Mode);
//             GameConfig.instance.isDebug = EditorGUILayout.Toggle("Enable Debug", GameConfig.instance.isDebug);
//             GUILayout.BeginHorizontal();
//             last = IPConfig.instance.ipList.FindIndex(x => x.title == IPConfig.instance.selected);
//             curIndex = EditorGUILayout.Popup("服务器地址", last, IPConfig.instance.ipList.Select(x => x.title).ToArray());
//             GUILayout.EndHorizontal();
//             if (curIndex >= 0 && curIndex < IPConfig.instance.ipList.Count && last != curIndex)
//             {
//                 IPConfig.instance.selected = IPConfig.instance.ipList[curIndex].title;
//                 IPConfig.Save();
//             }
//
//             GUILayout.BeginHorizontal();
//             ResConfig.instance.resMode = (ResourceMode)EditorGUILayout.EnumPopup("资源模式", ResConfig.instance.resMode);
//             if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.SETTING_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE, GUILayout.ExpandWidth(false)))
//             {
//                 GameBaseEditorWindow.SwitchScene<ResSetting>();
//             }
//
//             GUILayout.EndHorizontal();
//             last = BuilderConfig.instance.packages.FindIndex(x => x.title == ResConfig.instance.defaultPackageName);
//             curIndex = EditorGUILayout.Popup("资源包", last, BuilderConfig.instance.packages.Select(x => x.title).ToArray());
//             if (curIndex >= 0 && curIndex < BuilderConfig.instance.packages.Count)
//             {
//                 ResConfig.instance.defaultPackageName = BuilderConfig.instance.packages[curIndex].title;
//             }
//
//             ResConfig.instance.timeout = EditorGUILayout.Slider("包检查间隔时间", ResConfig.instance.timeout, 10, byte.MaxValue);
//             last = ResConfig.instance.ossList.FindIndex(x => x.title == ResConfig.instance.seletion);
//             curIndex = EditorGUILayout.Popup("资源服务器地址", last, ResConfig.instance.ossList.Select(x => x.title).ToArray());
//
//             if (curIndex >= 0 && curIndex < ResConfig.instance.ossList.Count && last != curIndex)
//             {
//                 ResConfig.instance.seletion = ResConfig.instance.ossList[curIndex].title;
//                 ResConfig.Save();
//             }
//
//
//             GUILayout.BeginHorizontal();
//             last = ChannelConfigList.instance.pckList.FindIndex(x => x.title == ChannelConfigList.instance.selected);
//             curIndex = EditorGUILayout.Popup("当前渠道", last, ChannelConfigList.instance.pckList.Select(x => x.title).ToArray());
//             if (curIndex >= 0 && curIndex < ChannelConfigList.instance.pckList.Count && last != curIndex)
//             {
//                 ChannelConfigList.instance.selected = ChannelConfigList.instance.pckList[curIndex].title;
//                 ChannelConfigList.Save();
//                 BuildGameChannelCommand.SetPlayerSetting(ChannelConfigList.instance.pckList[curIndex], GameConfig.instance.version);
//             }
//
//             GUILayout.EndHorizontal();
//
//             GUILayout.EndVertical();
//         }
//
//         public override void SearchRightDrawing()
//         {
//             if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.PLAY_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE, GUILayout.ExpandWidth(false)))
//             {
//                 GenericMenu menu = new GenericMenu();
//                 if (GameConfig.instance is not null && GameConfig.instance.mode == CodeMode.Hotfix)
//                 {
//                     menu.AddItem(new GUIContent("Build Hotfix Library Assets"), false, () =>
//                     {
//                         PackageSeting seting = BuilderConfig.instance.packages.Find(x => x.title == ResConfig.instance.defaultPackageName);
//                         BuildHotfixLibraryCommand.Execute();
//                     });
//                 }
//
//
//                 if (ChannelConfigList.instance.pckList != null && ChannelConfigList.instance.pckList.Count > 0)
//                 {
//                     foreach (var UPPER in BuilderConfig.instance.packages)
//                     {
//                         menu.AddItem(new GUIContent("Build Hotfix Package Assets/" + UPPER.title), false, () => { BuildResourcePackageCommand.Executer(UPPER); });
//                     }
//
//                     if (GameConfig.instance is not null && GameConfig.instance.mode == CodeMode.Hotfix)
//                     {
//                         menu.AddItem(new GUIContent("Build Hotfix Package and Library Assets"), false, () =>
//                         {
//                             BuildHotfixLibraryCommand.Execute();
//                             BuildResourcePackageCommand.Executer(null);
//                         });
//                     }
//
//                     foreach (var VARIABLE in ChannelConfigList.instance.pckList)
//                     {
//                         menu.AddItem(new GUIContent("Build Channel Package/" + VARIABLE.title), false, () => { BuildGameChannelCommand.Executer(VARIABLE); });
//                     }
//                 }
//
//                 menu.ShowAsContext();
//             }
//         }
//     }
// }