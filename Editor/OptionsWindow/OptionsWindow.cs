using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using ZEngine;
using ZEngine.Sound;
using ZEngine.VFS;

namespace ZEngine.Editor.OptionsEditorWindow
{
    public class OptionsWindow : SettingsProvider
    {
        static OptionsWindow provider;
        public static OptionsWindow instance => provider;

        private bool[] foldout = new bool[4];

        [MenuItem("Game/Options")]
        public static void OpenSetting()
        {
            SettingsService.OpenProjectSettings("Project/Options");
        }

        public OptionsWindow() : base("Project/Options", SettingsScope.Project)
        {
        }

        private SerializedObject hotfix;
        private SerializedObject audio;
        private SerializedObject vfs;
        private SerializedObject reference;

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            base.OnActivate(searchContext, rootElement);
            if (ZEngine.ReferenceOptions.instance is not null)
            {
                reference = new SerializedObject(ZEngine.ReferenceOptions.instance);
            }

            if (ZEngine.VFS.VFSOptions.instance is not null)
            {
                vfs = new SerializedObject(ZEngine.VFS.VFSOptions.instance);
            }

            if (ZEngine.HotfixOptions.instance is not null)
            {
                hotfix = new SerializedObject(ZEngine.HotfixOptions.instance);
            }

            if (ZEngine.Sound.SoundPlayOptions.instance is not null)
            {
                audio = new SerializedObject(ZEngine.Sound.SoundPlayOptions.instance);
            }
        }


        public override void OnGUI(string searchContext)
        {
            base.OnGUI(searchContext);
            foldout[0] = EditorGUILayout.Foldout(foldout[0], "Reference Options", EditorStyles.foldoutHeader);
            if (foldout[0])
            {
                GUILayout.BeginVertical("Reference Options", EditorStyles.helpBox);
                EditorGUILayout.PropertyField(reference.FindProperty("DefaultCount"), true);
                EditorGUILayout.PropertyField(reference.FindProperty("MaxCount"), true);
                GUILayout.EndVertical();
            }


            foldout[1] = EditorGUILayout.Foldout(foldout[1], "File System Options", EditorStyles.foldoutHeader);
            if (foldout[1])
            {
                GUILayout.BeginVertical("File System Options", EditorStyles.helpBox);
                EditorGUILayout.PropertyField(vfs.FindProperty("vfsState"), true);
                EditorGUILayout.PropertyField(vfs.FindProperty("layout"), true);
                EditorGUILayout.PropertyField(vfs.FindProperty("Lenght"), true);
                EditorGUILayout.PropertyField(vfs.FindProperty("Count"), true);
                EditorGUILayout.PropertyField(vfs.FindProperty("time"), true);
                GUILayout.EndVertical();
            }


            foldout[2] = EditorGUILayout.Foldout(foldout[2], "Hotfix Options", EditorStyles.foldoutHeader);
            if (foldout[2])
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


            foldout[3] = EditorGUILayout.Foldout(foldout[3], "Audio Options", EditorStyles.foldoutHeader);
            if (foldout[3])
            {
                GUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.PropertyField(audio.FindProperty("optionsList"), true);
                GUILayout.EndVertical();
            }

            if (EditorGUI.EndChangeCheck())
            {
                reference.ApplyModifiedProperties();
                vfs.ApplyModifiedProperties();
                hotfix.ApplyModifiedProperties();
                audio.ApplyModifiedProperties();
                ZEngine.HotfixOptions.instance.Saved();
                ZEngine.VFS.VFSOptions.instance.Saved();
                ZEngine.ReferenceOptions.instance.Saved();
                ZEngine.Sound.SoundPlayOptions.instance.Saved();
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