using System.Collections.Generic;
using System.Linq;
using HybridCLR.Editor.Settings;
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
            int last = 0;
            int curIndex = 0;
            EditorGUI.BeginChangeCheck();
            BasicConfig.instance.companyName = EditorGUILayout.TextField("公司名称", BasicConfig.instance.companyName);
            GUILayout.BeginHorizontal();
            last = BasicConfig.instance.entries.FindIndex(x => x.title == BasicConfig.instance.curEntryName);
            curIndex = EditorGUILayout.Popup("模块入口", last, BasicConfig.instance.entries.Select(x => x.title).ToArray());

            if (curIndex >= 0 && curIndex < BasicConfig.instance.entries.Count && last != curIndex)
            {
                BasicConfig.instance.curEntryName = BasicConfig.instance.entries[curIndex].title;
                List<AssemblyDefinitionAsset> hotfixAssemblies = new List<AssemblyDefinitionAsset>();
                if (BasicConfig.instance.curEntry.mode == CodeMode.Hotfix)
                {
                    if (BasicConfig.instance.curEntry.path.IsNullOrEmpty() is false && BasicConfig.instance.curEntry.assembly == null)
                    {
                        BasicConfig.instance.curEntry.assembly = AssetDatabase.LoadAssetAtPath<AssemblyDefinitionAsset>(BasicConfig.instance.curEntry.path);
                    }

                    hotfixAssemblies.Add(BasicConfig.instance.curEntry.assembly);
                }

                HybridCLRSettings.Instance.hotUpdateAssemblyDefinitions = hotfixAssemblies.ToArray();
                HybridCLRSettings.Instance.enable = hotfixAssemblies.Count > 0;
                HybridCLRSettings.Save();
            }

            if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.SETTING_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE, GUILayout.ExpandWidth(false)))
            {
                ToolsWindow.SwitchScene<GameWindow>();
            }

            if (BasicConfig.instance.curEntry is not null && BasicConfig.instance.curEntry.mode == CodeMode.Hotfix)
            {
                if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.PLAY_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE, GUILayout.ExpandWidth(false)))
                {
                    GenericMenu menu = new GenericMenu();
                    menu.AddItem(new GUIContent("Generic Dll"), false, () => ZGame.CommandManager.OnExecuteCommand<SubGameBuildCommand>(BasicConfig.instance.curEntry, false));
                    menu.AddItem(new GUIContent("Generic Porject"), false, () => ZGame.CommandManager.OnExecuteCommand<SubGameBuildCommand>(BasicConfig.instance.curEntry, true));
                    if (BasicConfig.instance.curEntry.channels != null && BasicConfig.instance.curEntry.channels.Count > 0)
                    {
                        foreach (var VARIABLE in BasicConfig.instance.curEntry.channels)
                        {
                            menu.AddItem(new GUIContent("Channels/" + VARIABLE.title), false, () =>
                            {
                                BasicConfig.instance.curEntry.currentChannel = VARIABLE.title;
                                ZGame.CommandManager.OnExecuteCommand<SubGameBuildCommand>(BasicConfig.instance.curEntry, false);
                            });
                        }
                    }

                    menu.ShowAsContext();
                }
            }

            GUILayout.EndHorizontal();
            BasicConfig.instance.language = (LanguageDefine)EditorGUILayout.EnumPopup("默认语言", BasicConfig.instance.language);
            NativeLeakDetection.Mode = (NativeLeakDetectionMode)EditorGUILayout.EnumPopup("Enable Stack Trace", NativeLeakDetection.Mode);
            GUILayout.BeginHorizontal();
            last = BasicConfig.instance.address.FindIndex(x => x.title == BasicConfig.instance.curAddressName);
            curIndex = EditorGUILayout.Popup("服务器地址", last, BasicConfig.instance.address.Select(x => x.title).ToArray());
            if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.SETTING_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE, GUILayout.ExpandWidth(false)))
            {
                ToolsWindow.SwitchScene<AddressWindow>();
            }

            GUILayout.EndHorizontal();
            if (curIndex >= 0 && curIndex < BasicConfig.instance.address.Count && last != curIndex)
            {
                BasicConfig.instance.curAddressName = BasicConfig.instance.address[curIndex].title;
            }

            GUILayout.BeginHorizontal();
            BasicConfig.instance.resMode = (ResourceMode)EditorGUILayout.EnumPopup("资源模式", BasicConfig.instance.resMode);
            // GUILayout.FlexibleSpace();
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

            if (EditorGUI.EndChangeCheck())
            {
                OSSConfig.OnSave();
                BasicConfig.OnSave();
            }
        }
    }
}