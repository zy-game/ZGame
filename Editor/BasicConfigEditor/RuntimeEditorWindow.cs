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

            BasicConfig.instance.companyName = EditorGUILayout.TextField("公司名称", BasicConfig.instance.companyName);
            BasicConfig.instance.apkUrl = EditorGUILayout.TextField("安装包下载地址", BasicConfig.instance.apkUrl);
            BasicConfig.instance.language = (LanguageDefine)EditorGUILayout.EnumPopup("默认语言", BasicConfig.instance.language);
            NativeLeakDetection.Mode = (NativeLeakDetectionMode)EditorGUILayout.EnumPopup("Enable Stack Trace", NativeLeakDetection.Mode);
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
        }

        private void OnShowGameConfigDrawing()
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (BasicConfig.instance.curEntry is not null && BasicConfig.instance.curEntry.mode == CodeMode.Hotfix)
            {
                if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.PLAY_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE, GUILayout.ExpandWidth(false)))
                {
                    GenericMenu menu = new GenericMenu();
                    menu.AddItem(new GUIContent("Update Hotfix Assets"), false, () => SubGameBuildCommand.Executer(BasicConfig.instance.curEntry, false));
                    if (BasicConfig.instance.curEntry.channels != null && BasicConfig.instance.curEntry.channels.Count > 0)
                    {
                        foreach (var VARIABLE in BasicConfig.instance.curEntry.channels)
                        {
                            menu.AddItem(new GUIContent("Channels/" + VARIABLE.title), false, () =>
                            {
                                BasicConfig.instance.curEntry.currentChannel = VARIABLE.title;
                                SubGameBuildCommand.Executer(BasicConfig.instance.curEntry, true);
                            });
                        }
                    }

                    menu.ShowAsContext();
                }
            }

            GUILayout.EndHorizontal();
            BasicConfig.instance.curEntry.title = EditorGUILayout.TextField("游戏名", BasicConfig.instance.curEntry.title);
            if (BasicConfig.instance.curEntry.path.IsNullOrEmpty() is false && BasicConfig.instance.curEntry.assembly == null)
            {
                BasicConfig.instance.curEntry.assembly = AssetDatabase.LoadAssetAtPath<AssemblyDefinitionAsset>(BasicConfig.instance.curEntry.path);
            }

            BasicConfig.instance.curEntry.version = EditorGUILayout.TextField("版本", BasicConfig.instance.curEntry.version);
            var mode = (CodeMode)EditorGUILayout.EnumPopup("模式", BasicConfig.instance.curEntry.mode);
            if (mode != BasicConfig.instance.curEntry.mode)
            {
                BasicConfig.instance.curEntry.mode = mode;
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

            var resList = BuilderConfig.instance.packages.Select(x => x.name).ToList();
            int last = resList.FindIndex(x => x == BasicConfig.instance.curEntry.module);
            int curIndex = EditorGUILayout.Popup("资源包", last, resList.ToArray());
            if (curIndex >= 0 && curIndex < resList.Count)
            {
                BasicConfig.instance.curEntry.module = resList[curIndex];
            }

            BasicConfig.instance.curEntry.assembly = (AssemblyDefinitionAsset)EditorGUILayout.ObjectField("Assembly", BasicConfig.instance.curEntry.assembly, typeof(AssemblyDefinitionAsset), false);
            if (BasicConfig.instance.curEntry.assembly != null)
            {
                BasicConfig.instance.curEntry.path = AssetDatabase.GetAssetPath(BasicConfig.instance.curEntry.assembly);
            }

            GUILayout.BeginHorizontal();
            resList = BasicConfig.instance.curEntry.channels?.Select(x => x.title).ToList();
            last = resList.FindIndex(x => x == BasicConfig.instance.curEntry.currentChannel);
            curIndex = EditorGUILayout.Popup("当前渠道", last, resList.ToArray());
            if (curIndex >= 0 && curIndex < resList.Count)
            {
                BasicConfig.instance.curEntry.currentChannel = resList[curIndex];
            }

            GUILayout.EndHorizontal();
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.BeginHorizontal();
            BasicConfig.instance.curEntry.isShowReferences = EditorGUILayout.Foldout(BasicConfig.instance.curEntry.isShowReferences, "Reference Assembly");
            GUILayout.FlexibleSpace();
            if (BasicConfig.instance.curEntry.assembly != null)
            {
                BasicConfig.instance.curEntry.entryName = BasicConfig.instance.curEntry.assembly.name;
            }

            if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.ADD_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE))
            {
                BasicConfig.instance.curEntry.references.Add(string.Empty);
                BasicConfig.instance.curEntry.referenceAssemblyList.Add(null);
                BasicConfig.OnSave();
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(5);
            if (BasicConfig.instance.curEntry.isShowReferences)
            {
                for (int j = BasicConfig.instance.curEntry.referenceAssemblyList.Count - 1; j >= 0; j--)
                {
                    GUILayout.BeginHorizontal(ZStyle.ITEM_BACKGROUND_STYLE);
                    BasicConfig.instance.curEntry.referenceAssemblyList[j] = (AssemblyDefinitionAsset)EditorGUILayout.ObjectField("Element " + j, BasicConfig.instance.curEntry.referenceAssemblyList[j], typeof(AssemblyDefinitionAsset), false, GUILayout.Width(300));
                    if (BasicConfig.instance.curEntry.referenceAssemblyList[j] != null)
                    {
                        BasicConfig.instance.curEntry.references[j] = BasicConfig.instance.curEntry.referenceAssemblyList[j].name;
                    }

                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.DELETE_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE))
                    {
                        BasicConfig.instance.curEntry.references.RemoveAt(j);
                        BasicConfig.instance.curEntry.referenceAssemblyList.RemoveAt(j);
                        BasicConfig.OnSave();
                    }

                    GUILayout.EndHorizontal();
                }
            }


            GUILayout.EndVertical();
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.BeginHorizontal();
            BasicConfig.instance.curEntry.isShowChannels = EditorGUILayout.Foldout(BasicConfig.instance.curEntry.isShowChannels, "Channels");
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.ADD_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE))
            {
                BasicConfig.instance.curEntry.channels.Add(new ChannelPackageOptions());
                BasicConfig.OnSave();
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(5);
            if (BasicConfig.instance.curEntry.isShowChannels)
            {
                for (int j = BasicConfig.instance.curEntry.channels.Count - 1; j >= 0; j--)
                {
                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    GUILayout.BeginHorizontal();
                    BasicConfig.instance.curEntry.channels[j].title = EditorGUILayout.TextField("Channel Name", BasicConfig.instance.curEntry.channels[j].title);
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.DELETE_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE))
                    {
                        BasicConfig.instance.curEntry.channels.RemoveAt(j);
                        BasicConfig.OnSave();
                    }

                    GUILayout.EndHorizontal();
                    BasicConfig.instance.curEntry.channels[j].packageName = EditorGUILayout.TextField("Package Name", BasicConfig.instance.curEntry.channels[j].packageName);
                    BasicConfig.instance.curEntry.channels[j].appName = EditorGUILayout.TextField("App Name", BasicConfig.instance.curEntry.channels[j].appName);
                    BasicConfig.instance.curEntry.channels[j].icon = (EditorGUILayout.ObjectField("Channel Icon", BasicConfig.instance.curEntry.channels[j].icon, typeof(Texture2D), false) as Texture2D);
                    BasicConfig.instance.curEntry.channels[j].splash = (EditorGUILayout.ObjectField("Channel Splash", BasicConfig.instance.curEntry.channels[j].splash, typeof(Sprite), false) as Sprite);
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