using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using ZEngine;
using ZEngine.VFS;

namespace ZEngine.Editor.OptionsEditorWindow
{
    public class OptionsWindow : SettingsProvider
    {
        static OptionsWindow provider;
        public static OptionsWindow instance => provider;

        private bool[] foldout = new bool[2];

        // [MenuItem("工具/项目设置")]
        public static void OpenSetting()
        {
            SettingsService.OpenProjectSettings("Project/Options");
        }


        public OptionsWindow() : base("Project/Options", SettingsScope.Project)
        {
        }

        private SerializedObject hotfix;
        private SerializedObject vfs;

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            base.OnActivate(searchContext, rootElement);
            if (ZEngine.VFS.VFSOptions.instance is not null)
            {
                vfs = new SerializedObject(ZEngine.VFS.VFSOptions.instance);
            }

            if (ZEngine.HotfixOptions.instance is not null)
            {
                hotfix = new SerializedObject(ZEngine.HotfixOptions.instance);
                
            }
        }


        public override void OnGUI(string searchContext)
        {
            base.OnGUI(searchContext);
            EditorGUI.BeginChangeCheck();
            int index = 0;
            foldout[index] = EditorGUILayout.Foldout(foldout[index], "File System Options", EditorStyles.foldoutHeader);
            if (foldout[index])
            {
                GUILayout.BeginVertical("File System Options", EditorStyles.helpBox);
                EditorGUILayout.PropertyField(vfs.FindProperty("vfsState"), true);
                EditorGUILayout.PropertyField(vfs.FindProperty("layout"), true);
                EditorGUILayout.PropertyField(vfs.FindProperty("Lenght"), true);
                EditorGUILayout.PropertyField(vfs.FindProperty("Count"), true);
                EditorGUILayout.PropertyField(vfs.FindProperty("time"), true);
                GUILayout.EndVertical();
            }

            index++;
            foldout[index] = EditorGUILayout.Foldout(foldout[index], "Hotfix Options", EditorStyles.foldoutHeader);
            if (foldout[index])
            {
                GUILayout.BeginVertical("Hotfix Options", EditorStyles.helpBox);
                EditorGUILayout.PropertyField(hotfix.FindProperty("useHotfix"), true);
                EditorGUILayout.PropertyField(hotfix.FindProperty("useScript"), true);
                EditorGUILayout.PropertyField(hotfix.FindProperty("useAsset"), true);
                EditorGUILayout.PropertyField(hotfix.FindProperty("entryList"), true);
                EditorGUILayout.PropertyField(hotfix.FindProperty("cachetime"), true);
                EditorGUILayout.PropertyField(hotfix.FindProperty("address"), true);
                EditorGUILayout.PropertyField(hotfix.FindProperty("preloads"), true);
                GUILayout.EndVertical();
            }

            if (EditorGUI.EndChangeCheck())
            {
                vfs.ApplyModifiedProperties();
                hotfix.ApplyModifiedProperties();
                ZEngine.HotfixOptions.instance.Saved();
                ZEngine.VFS.VFSOptions.instance.Saved();
                // ZEngine.ReferenceOptions.instance.Saved();
            }
        }

        [SettingsProvider]
        public static SettingsProvider CreateZyGameOptions()
        {
            if (provider == null)
            {
                provider = new OptionsWindow();
            }

            return provider;
        }
    }
}