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

namespace ZGame.Editor
{
    [SubPageSetting("全局配置")]
    [ReferenceScriptableObject(typeof(GlobalConfig))]
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
            last = GlobalConfig.instance.entries.FindIndex(x => x.title == GlobalConfig.instance.curEntryName);
            curIndex = EditorGUILayout.Popup("模块入口", last, GlobalConfig.instance.entries.Select(x => x.title).ToArray());

            if (curIndex >= 0 && curIndex < GlobalConfig.instance.entries.Count && last != curIndex)
            {
                GlobalConfig.instance.curEntryName = GlobalConfig.instance.entries[curIndex].title;
            }

            GUILayout.BeginHorizontal();
            last = GlobalConfig.instance.address.FindIndex(x => x.title == GlobalConfig.instance.curAddressName);
            curIndex = EditorGUILayout.Popup("默认地址", last, GlobalConfig.instance.address.Select(x => x.title).ToArray());
            GUILayout.EndHorizontal();
            if (curIndex >= 0 && curIndex < GlobalConfig.instance.address.Count && last != curIndex)
            {
                GlobalConfig.instance.curAddressName = GlobalConfig.instance.address[curIndex].title;
            }

            if (EditorGUI.EndChangeCheck())
            {
                GlobalConfig.OnSave();
            }

            show3 = OnShowFoldoutHeader("VFS", show3);
            if (show3)
            {
                OnShowVFSConfig(GlobalConfig.instance.vfsConfig);
            }

            show2 = OnShowFoldoutHeader("Games", show2, () => GlobalConfig.instance.entries.Add(new EntryConfig()));
            if (show2)
            {
                EditorGUI.BeginChangeCheck();
                for (int i = 0; i < GlobalConfig.instance.entries.Count; i++)
                {
                    OnShowGameConfig(GlobalConfig.instance.entries[i]);
                }

                if (EditorGUI.EndChangeCheck())
                {
                    GlobalConfig.OnSave();
                }
            }

            show1 = OnShowFoldoutHeader("Address", show1, () => GlobalConfig.instance.address.Add(new IPConfig()));
            if (show1)
            {
                EditorGUI.BeginChangeCheck();
                for (int i = 0; i < GlobalConfig.instance.address.Count; i++)
                {
                    OnShowAddressConfig(GlobalConfig.instance.address[i]);
                }

                if (EditorGUI.EndChangeCheck())
                {
                    GlobalConfig.OnSave();
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
                GlobalConfig.instance.address.Remove(config);
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

            config.assembly = (AssemblyDefinitionAsset)EditorGUILayout.ObjectField("Assembly", config.assembly, typeof(AssemblyDefinitionAsset), false);
            if (config.assembly != null)
            {
                config.path = AssetDatabase.GetAssetPath(config.assembly);
                config.entryName = config.assembly.name;
                if (config.references is null || config.references.Count == 0)
                {
                    AssemlyInfo info = JsonConvert.DeserializeObject<AssemlyInfo>(config.assembly.text);
                    Debug.Log(string.Join(" ", info.references));
                    config.references = info.references.Select(x => Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(x.Split(':')[1]))).ToList();
                }
            }

            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Reference Assembly", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
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


            config.resMode = (ResourceMode)EditorGUILayout.EnumPopup("资源模式", config.resMode);
            var resList = BuilderConfig.instance.packages.Select(x => x.name).ToList();
            int last = resList.FindIndex(x => x == config.module);
            int i = EditorGUILayout.Popup("默认资源", last, resList.ToArray());
            if (i >= 0 && i < resList.Count)
            {
                config.module = resList[i];
            }

            resList = BuilderConfig.instance.ossList.Select(x => x.title).ToList();
            last = BuilderConfig.instance.ossList.FindIndex(x => x.title == config.oss);
            i = EditorGUILayout.Popup("默认OSS", last, resList.ToArray());
            if (i >= 0 && i < resList.Count && last != i)
            {
                OSSOptions options = BuilderConfig.instance.ossList.Find(x => x.title == resList[i]);
                config.ossTitle = options.title;
                if (options.type == OSSType.Aliyun)
                {
                    config.oss = $"https://{options.bucket}.oss-{options.region}.aliyuncs.com/";
                }
                else
                {
                    config.oss = $"https://{options.bucket}.cos.{options.region}.myqcloud.com/";
                }
            }

            config.unloadInterval = EditorGUILayout.Slider("包检查间隔时间", config.unloadInterval, 30, ushort.MaxValue);
            GUILayout.EndVertical();
        }

        private void OnShowVFSConfig(VFSConfig config)
        {
            EditorGUI.BeginChangeCheck();
            config.chunkSize = EditorGUILayout.IntField("并发运行数量", config.chunkSize);
            config.chunkCount = EditorGUILayout.IntField("并发运行数量", config.chunkCount);
            if (EditorGUI.EndChangeCheck())
            {
                GlobalConfig.OnSave();
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