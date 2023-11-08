using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
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

        public void OnEnable()
        {
            _startup = (Startup)target;
        }

        public override void OnInspectorGUI()
        {
            // GUILayout.BeginVertical(EditorStyles.helpBox);
            // GUILayout.Label("Global Setting", EditorStyles.boldLabel);
            // EditorGUILayout.PropertyField(this.serializedObject.FindProperty("useHotfix"));
            // GUILayout.EndVertical();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(this.serializedObject.FindProperty("GameSettings"), true);
            if (EditorGUI.EndChangeCheck())
            {
                this.serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(_startup);
            }
        }
    }
}