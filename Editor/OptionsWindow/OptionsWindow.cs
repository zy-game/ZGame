﻿using System;
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
            reference = new SerializedObject(ZEngine.ReferenceOptions.instance);
            vfs = new SerializedObject(ZEngine.VFS.VFSOptions.instance);
            hotfix = new SerializedObject(ZEngine.Resource.HotfixOptions.instance);
            audio = new SerializedObject(ZEngine.Sound.SoundPlayOptions.instance);
        }


        public override void OnGUI(string searchContext)
        {
            base.OnGUI(searchContext);
            GUILayout.BeginVertical("Reference Options", EditorStyles.helpBox);
            GUILayout.Space(10);
            EditorGUILayout.PropertyField(reference.FindProperty("DefaultCount"), true);
            EditorGUILayout.PropertyField(reference.FindProperty("MaxCount"), true);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("File System Options", EditorStyles.helpBox);
            GUILayout.Space(10);
            EditorGUILayout.PropertyField(vfs.FindProperty("vfsState"), true);
            EditorGUILayout.PropertyField(vfs.FindProperty("layout"), true);
            EditorGUILayout.PropertyField(vfs.FindProperty("Lenght"), true);
            EditorGUILayout.PropertyField(vfs.FindProperty("Count"), true);
            EditorGUILayout.PropertyField(vfs.FindProperty("time"), true);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("Hotfix Options", EditorStyles.helpBox);
            GUILayout.Space(10);
            EditorGUILayout.PropertyField(hotfix.FindProperty("useHotfix"), true);
            EditorGUILayout.PropertyField(hotfix.FindProperty("useScript"), true);
            EditorGUILayout.PropertyField(hotfix.FindProperty("useAsset"), true);
            EditorGUILayout.PropertyField(hotfix.FindProperty("cachetime"), true);
            EditorGUILayout.PropertyField(hotfix.FindProperty("autoLoad"), true);
            EditorGUILayout.PropertyField(hotfix.FindProperty("address"), true);
            EditorGUILayout.PropertyField(hotfix.FindProperty("preloads"), true);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("Audio Options", EditorStyles.helpBox);
            GUILayout.Space(10);
            EditorGUILayout.PropertyField(audio.FindProperty("optionsList"), true);
            GUILayout.EndVertical();
            if (EditorGUI.EndChangeCheck())
            {
                reference.ApplyModifiedProperties();
                vfs.ApplyModifiedProperties();
                hotfix.ApplyModifiedProperties();
                audio.ApplyModifiedProperties();
                ZEngine.Resource.HotfixOptions.instance.Saved();
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