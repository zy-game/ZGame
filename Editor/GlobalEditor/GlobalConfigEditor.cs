using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
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
        private List<Type> types;
        private List<string> resList;

        public override void OnEnable(params object[] args)
        {
            types = AppDomain.CurrentDomain.GetAllSubClasses<SubGameEntry>();
            resList = BuilderConfig.instance.packages?.Select(x => x.name).ToList();
        }

        public override void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            GlobalConfig.instance.language = (LanguageDefine)EditorGUILayout.EnumPopup("语言", GlobalConfig.instance.language);
            if (EditorGUI.EndChangeCheck())
            {
                GlobalConfig.OnSave();
            }

            show1 = OnShowFoldoutHeader("Resource Config", show1);
            if (show1)
            {
                OnShowResConfig(GlobalConfig.instance.resConfig);
            }

            show2 = OnShowFoldoutHeader("Game Config", show2);
            if (show2)
            {
                OnShowGameConfig(GlobalConfig.instance.gameConfig);
            }

            show3 = OnShowFoldoutHeader("VFS Config", show3);
            if (show3)
            {
                OnShowVFSConfig(GlobalConfig.instance.vfsConfig);
            }
        }


        private void OnShowResConfig(ResConfig config)
        {
            EditorGUI.BeginChangeCheck();
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
                config.oss = options.title;
                if (options.type == OSSType.Aliyun)
                {
                    config.address = $"https://{options.bucket}.oss-{options.region}.aliyuncs.com/";
                }
                else
                {
                    config.address = $"https://{options.bucket}.cos.{options.region}.myqcloud.com/";
                }
            }

            config.unloadInterval = EditorGUILayout.Slider("包检查间隔时间", config.unloadInterval, 30, ushort.MaxValue);
            if (EditorGUI.EndChangeCheck())
            {
                GlobalConfig.OnSave();
            }
        }

        private void OnShowGameConfig(GameConfig config)
        {
            EditorGUI.BeginChangeCheck();

            int last = types.FindIndex(x => x.Assembly.GetName().Name == config.dll);
            int i = EditorGUILayout.Popup("模块入口", last, types.Select(x => x.FullName).ToArray());
            if (i >= 0 && i < types.Count && last != i)
            {
                config.dll = types[i].Assembly.GetName().Name;
            }

            if (EditorGUI.EndChangeCheck())
            {
                GlobalConfig.OnSave();
            }
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
}