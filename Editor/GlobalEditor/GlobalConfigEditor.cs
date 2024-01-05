using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using ZGame.Config;
using ZGame.Editor.ResBuild.Config;
using ZGame.Game;
using ZGame.Resource.Config;

namespace ZGame.Editor
{
    [SubPageSetting("全局配置")]
    [ReferenceScriptableObject(typeof(BasicConfig))]
    public class GlobalConfigEditor : SubPage
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
            last = BasicConfig.instance.entries.FindIndex(x => x.title == BasicConfig.instance.curEntryName);
            curIndex = EditorGUILayout.Popup("模块入口", last, BasicConfig.instance.entries.Select(x => x.title).ToArray());

            if (curIndex >= 0 && curIndex < BasicConfig.instance.entries.Count && last != curIndex)
            {
                BasicConfig.instance.curEntryName = BasicConfig.instance.entries[curIndex].title;
            }

            GUILayout.BeginHorizontal();
            last = BasicConfig.instance.address.FindIndex(x => x.title == BasicConfig.instance.curAddressName);
            curIndex = EditorGUILayout.Popup("服务器地址", last, BasicConfig.instance.address.Select(x => x.title).ToArray());
            GUILayout.EndHorizontal();
            if (curIndex >= 0 && curIndex < BasicConfig.instance.address.Count && last != curIndex)
            {
                BasicConfig.instance.curAddressName = BasicConfig.instance.address[curIndex].title;
            }

            BasicConfig.instance.resMode = (ResourceMode)EditorGUILayout.EnumPopup("资源模式", BasicConfig.instance.resMode);
            if (EditorGUI.EndChangeCheck())
            {
                BasicConfig.OnSave();
            }

            show3 = OnShowFoldoutHeader("VFS", show3);
            if (show3)
            {
                OnShowVFSConfig(BasicConfig.instance.vfsConfig);
            }

            show2 = OnShowFoldoutHeader("Games", show2, () => BasicConfig.instance.entries.Add(new EntryConfig()));
            if (show2)
            {
                EditorGUI.BeginChangeCheck();
                for (int i = 0; i < BasicConfig.instance.entries.Count; i++)
                {
                    OnShowGameConfig(BasicConfig.instance.entries[i]);
                }

                if (EditorGUI.EndChangeCheck())
                {
                    BasicConfig.OnSave();
                }
            }

            show1 = OnShowFoldoutHeader("Address", show1, () => BasicConfig.instance.address.Add(new IPConfig()));
            if (show1)
            {
                EditorGUI.BeginChangeCheck();
                for (int i = 0; i < BasicConfig.instance.address.Count; i++)
                {
                    OnShowAddressConfig(BasicConfig.instance.address[i]);
                }

                if (EditorGUI.EndChangeCheck())
                {
                    BasicConfig.OnSave();
                }
            }
        }

        private void OnShowAddressConfig(IPConfig config)
        {
            GUILayout.BeginHorizontal(ZStyle.ITEM_BACKGROUND_STYLE);
            config.title = EditorGUILayout.TextField(config.title);
            config.address = EditorGUILayout.TextField(config.address);
            config.port = EditorGUILayout.IntField(config.port);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.DELETE_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE))
            {
                BasicConfig.instance.address.Remove(config);
            }

            GUILayout.EndHorizontal();
        }


        private void OnShowGameConfig(EntryConfig config)
        {
            if (config.references is null)
            {
                config.references = new();
            }

            GUILayout.BeginVertical(EditorStyles.helpBox);

            config.title = EditorGUILayout.TextField("游戏名", config.title);
            config.language = (LanguageDefine)EditorGUILayout.EnumPopup("语言", config.language);
            if (config.path.IsNullOrEmpty() is false && config.assembly == null)
            {
                config.assembly = AssetDatabase.LoadAssetAtPath<AssemblyDefinitionAsset>(config.path);
            }

            config.mode = (CodeMode)EditorGUILayout.EnumPopup("模式", config.mode);

            var resList = BuilderConfig.instance.packages.Select(x => x.name).ToList();
            int last = resList.FindIndex(x => x == config.module);
            int curIndex = EditorGUILayout.Popup("资源包", last, resList.ToArray());
            if (curIndex >= 0 && curIndex < resList.Count)
            {
                config.module = resList[curIndex];
            }

            last = OSSConfig.instance.ossList.FindIndex(x => x.title == config.ossTitle);
            curIndex = EditorGUILayout.Popup("资源服务器地址", last, OSSConfig.instance.ossList.Select(x => x.title).ToArray());

            if (curIndex >= 0 && curIndex < OSSConfig.instance.ossList.Count && last != curIndex)
            {
                OSSOptions ossOptions = OSSConfig.instance.ossList[curIndex];
                config.ossTitle = ossOptions.title;
            }

            config.unloadInterval = EditorGUILayout.Slider("包检查间隔时间", config.unloadInterval, 30, ushort.MaxValue);
            config.assembly = (AssemblyDefinitionAsset)EditorGUILayout.ObjectField("Assembly", config.assembly, typeof(AssemblyDefinitionAsset), false);
            if (config.assembly != null)
            {
                config.path = AssetDatabase.GetAssetPath(config.assembly);
                config.entryName = config.assembly.name;
                if (config.references is null || config.references.Count == 0)
                {
                    AssemlyInfo info = JsonConvert.DeserializeObject<AssemlyInfo>(config.assembly.text);
                    config.references = info.references.Select(x => Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(x.Split(':')[1]))).ToList();
                }
            }

            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Reference Assembly", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.REFRESH_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE))
            {
                if (config.references is null || config.references.Count == 0)
                {
                    AssemlyInfo info = JsonConvert.DeserializeObject<AssemlyInfo>(config.assembly.text);
                    config.references = info.references.Select(x => Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(x.Split(':')[1]))).ToList();
                }
            }

            if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.ADD_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE))
            {
                config.references.Add(string.Empty);
            }

            GUILayout.EndHorizontal();
            for (int j = config.references.Count - 1; j >= 0; j--)
            {
                GUILayout.BeginHorizontal(ZStyle.ITEM_BACKGROUND_STYLE);
                GUILayout.Label(config.references[j]);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.DELETE_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE))
                {
                    config.references.RemoveAt(j);
                    j--;
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();


            GUILayout.EndVertical();
        }

        private void OnShowVFSConfig(VFSConfig config)
        {
            EditorGUI.BeginChangeCheck();
            config.chunkSize = EditorGUILayout.IntField("并发运行数量", config.chunkSize);
            config.chunkCount = EditorGUILayout.IntField("并发运行数量", config.chunkCount);
            if (EditorGUI.EndChangeCheck())
            {
                BasicConfig.OnSave();
            }
        }
    }

    public class AssemlyInfo
    {
        /// <summary>
        /// 
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string rootNamespace { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<string> references { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<string> includePlatforms { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<string> excludePlatforms { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string allowUnsafeCode { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string overrideReferences { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<string> precompiledReferences { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string autoReferenced { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<string> defineConstraints { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<string> versionDefines { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string noEngineReferences { get; set; }
    }
}