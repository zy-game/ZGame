using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using ZGame.Config;
using ZGame.Editor.ResBuild;
using ZGame.Editor.ResBuild.Config;
using ZGame.Game;
using ZGame.Resource.Config;

namespace ZGame.Editor
{
    [SubPageSetting("全局配置")]
    [ReferenceScriptableObject(typeof(BasicConfig))]
    public class GlobalWindow : SubPage
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
            GUILayout.BeginHorizontal();
            last = BasicConfig.instance.entries.FindIndex(x => x.title == BasicConfig.instance.curEntryName);
            curIndex = EditorGUILayout.Popup("模块入口", last, BasicConfig.instance.entries.Select(x => x.title).ToArray());

            if (curIndex >= 0 && curIndex < BasicConfig.instance.entries.Count && last != curIndex)
            {
                BasicConfig.instance.curEntryName = BasicConfig.instance.entries[curIndex].title;
            }

            if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.SETTING_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE, GUILayout.ExpandWidth(false)))
            {
                EditorManager.SwitchScene<GameWindow>();
            }

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            last = BasicConfig.instance.address.FindIndex(x => x.title == BasicConfig.instance.curAddressName);
            curIndex = EditorGUILayout.Popup("服务器地址", last, BasicConfig.instance.address.Select(x => x.title).ToArray());
            if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.SETTING_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE, GUILayout.ExpandWidth(false)))
            {
                EditorManager.SwitchScene<AddressWindow>();
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
                EditorManager.SwitchScene<ResSetting>();
            }

            GUILayout.EndHorizontal();
            if (BasicConfig.instance.resMode == ResourceMode.Simulator)
            {
                BasicConfig.instance.resTimeout = EditorGUILayout.Slider("包检查间隔时间", BasicConfig.instance.resTimeout, 10, byte.MaxValue);
                last = OSSConfig.instance.ossList.FindIndex(x => x.title == OSSConfig.instance.seletion);
                curIndex = EditorGUILayout.Popup("资源服务器地址", last, OSSConfig.instance.ossList.Select(x => x.title).ToArray());

                if (curIndex >= 0 && curIndex < OSSConfig.instance.ossList.Count && last != curIndex)
                {
                    OSSConfig.instance.seletion = OSSConfig.instance.ossList[curIndex].title;
                    OSSConfig.OnSave();
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                OSSConfig.OnSave();
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

        public List<string> GetReferenceList()
        {
            if (references is null)
            {
                return new List<string>();
            }

            return references.Select(x => Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(x.Split(':')[1]))).ToList();
        }
    }
}