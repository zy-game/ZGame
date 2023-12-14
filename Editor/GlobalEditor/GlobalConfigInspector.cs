using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using ZGame.Editor.ResBuild.Config;
using ZGame.FileSystem;
using ZGame.Game;
using ZGame.Networking;
using ZGame.Resource;
using ZGame.Window;

namespace ZGame.Editor
{
    [CustomEditor(typeof(GlobalConfig))]
    public class GlobalConfigInspector : UnityEditor.Editor
    {
        private GlobalConfig globalConfig;
        private List<Type> types;
        private List<string> resList;


        public void OnEnable()
        {
            globalConfig = (GlobalConfig)target;
            if (globalConfig.entrys == null)
            {
                globalConfig.entrys = new List<EntryConfig>();
            }

            types = AppDomain.CurrentDomain.GetAllSubClasses<SubGameEntry>();
            resList = BuilderConfig.instance.packages?.Select(x => x.name).ToList();
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            globalConfig.parallelRunnableCount = EditorGUILayout.IntField("并发运行数量", globalConfig.parallelRunnableCount);
            globalConfig.unloadBundleInterval = EditorGUILayout.FloatField("并发运行数量", globalConfig.unloadBundleInterval);
            for (int i = 0; i < globalConfig.entrys.Count; i++)
            {
                GUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("", ZStyle.GUI_STYLE_MINUS))
                {
                    globalConfig.entrys.Remove(globalConfig.entrys[i]);
                }

                GUILayout.EndHorizontal();
                DrawingSetting(globalConfig.entrys[i]);
                GUILayout.EndVertical();
            }

            if (GUILayout.Button("Add Entry"))
            {
                globalConfig.entrys.Add(new EntryConfig());
            }

            if (EditorGUI.EndChangeCheck())
            {
                this.serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(globalConfig);
            }
        }

        private void DrawingSetting(EntryConfig seting)
        {
            seting.active = EditorGUILayout.Toggle("启用", seting.active);
            seting.runtime = (RuntimeMode)EditorGUILayout.EnumPopup("资源模式", seting.runtime);
            resList = BuilderConfig.instance.packages.Select(x => x.name).ToList();
            int last = resList.FindIndex(x => x == seting.module);
            int i = EditorGUILayout.Popup("默认资源", last, resList.ToArray());
            if (i >= 0 && i < resList.Count)
            {
                seting.module = resList[i];
            }

            resList = BuilderConfig.instance.ossList.Select(x => x.title).ToList();
            last = BuilderConfig.instance.ossList.FindIndex(x => x.title == seting.oss);
            i = EditorGUILayout.Popup("默认OSS", last, resList.ToArray());
            if (i >= 0 && i < resList.Count && last != i)
            {
                OSSOptions options = BuilderConfig.instance.ossList.Find(x => x.title == resList[i]);
                seting.oss = options.title;
                if (options.type == OSSType.Aliyun)
                {
                    seting.address = $"https://{options.bucket}.oss-{options.region}.aliyuncs.com/";
                }
                else
                {
                    seting.address = $"https://{options.bucket}.cos.{options.region}.myqcloud.com/";
                }
            }

            last = types.FindIndex(x => x.Assembly.GetName().Name == seting.dll);
            i = EditorGUILayout.Popup("模块入口", last, types.Select(x => x.FullName).ToArray());
            if (i >= 0 && i < types.Count && last != i)
            {
                seting.dll = types[i].Assembly.GetName().Name;
            }
        }
    }
}