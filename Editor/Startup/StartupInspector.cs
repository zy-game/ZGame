using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using ZGame.Editor.ResBuild.Config;
using ZGame.FileSystem;
using ZGame.Game;
using ZGame.Localization;
using ZGame.Networking;
using ZGame.Resource;
using ZGame.Window;

namespace ZGame.Editor
{
    [CustomEditor(typeof(Startup))]
    public class StartupInspector : UnityEditor.Editor
    {
        private Startup _startup;
        private List<Type> types;
        public void OnEnable()
        {
            _startup = (Startup)target;
            types = AppDomain.CurrentDomain.GetCustomAttributesWithoutType<Entry>();
        }

        public override void OnInspectorGUI()
        {
            // GUILayout.BeginVertical(EditorStyles.helpBox);
            // GUILayout.Label("Global Setting", EditorStyles.boldLabel);
            // EditorGUILayout.PropertyField(this.serializedObject.FindProperty("useHotfix"));
            // GUILayout.EndVertical();
            EditorGUI.BeginChangeCheck();
            if (_startup.GameSettings == null)
            {
                _startup.GameSettings = new List<GameSeting>();
            }

            for (int i = 0; i < _startup.GameSettings.Count; i++)
            {
                GUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("", ZStyle.GUI_STYLE_MINUS))
                {
                    _startup.GameSettings.Remove(_startup.GameSettings[i]);
                }

                GUILayout.EndHorizontal();
                DrawingSetting(_startup.GameSettings[i]);
                GUILayout.EndVertical();
            }

            if (GUILayout.Button("Add Entry"))
            {
                _startup.GameSettings.Add(new GameSeting());
            }

            //EditorGUILayout.PropertyField(this.serializedObject.FindProperty("GameSettings"), true);
            if (EditorGUI.EndChangeCheck())
            {
                this.serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(_startup);
            }
        }

        private void DrawingSetting(GameSeting seting)
        {
            seting.active = EditorGUILayout.Toggle("启用", seting.active);
            seting.useHotfix = EditorGUILayout.Toggle(new GUIContent("启用热更", "该字段只在编辑器中有效"), seting.useHotfix);
            seting.Language = (Language)EditorGUILayout.EnumPopup("默认语言", seting.Language);
            List<string> resList = BuilderConfig.instance.packages?.Select(x => x.name).ToList();
            if (seting.module.IsNullOrEmpty() && resList.Count > 0)
            {
                seting.module = BuilderConfig.instance.packages[0].name;
            }

            int i = EditorGUILayout.Popup("默认资源", resList.IndexOf(seting.module), resList.ToArray());
            if (i >= 0 && i < resList.Count)
            {
                seting.module = resList[i];
            }

            resList = BuilderConfig.instance.ossList.Select(x => x.title).ToList();

            if (seting.address.IsNullOrEmpty() && resList.Count > 0)
            {
                seting.address = BuilderConfig.instance.ossList[0].address;
            }

            i = BuilderConfig.instance.ossList.FindIndex(x => x.address == seting.address);
            i = EditorGUILayout.Popup("默认OSS", i, resList.ToArray());
            if (i >= 0 && i < resList.Count)
            {
                seting.address = BuilderConfig.instance.ossList.Find(x => x.title == resList[i])?.address;
            }

            i = types.FindIndex(x => x.Assembly.GetName().Name == seting.dll);
            i = EditorGUILayout.Popup("模块入口", i, types.Select(x => x.FullName).ToArray());
            if (i >= 0 && i < types.Count)
            {
                seting.dll = types[i].Assembly.GetName().Name;
            }
        }
    }
}