using UnityEditor;
using UnityEngine;
using ZGame.Window;

namespace ZGame.Editor.PSD2GUI
{
    [CustomEditor(typeof(LongPresseButton))]
    public class LongPersseButtonEditor : CustomEditorWindow
    {
        private LongPresseButton template;


        public void OnEnable()
        {
            template = target as LongPresseButton;
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            SerializedProperty limitTime = serializedObject.FindProperty("limitTime");
            EditorGUILayout.PropertyField(limitTime);

            if (template.limitTime <= 0)
            {
                template.limitTime = 0.5f;
            }

            SerializedProperty onDown = serializedObject.FindProperty("_onDown");
            EditorGUILayout.PropertyField(onDown);
            SerializedProperty onUp = serializedObject.FindProperty("_onUp");
            EditorGUILayout.PropertyField(onUp);
            SerializedProperty onCancel = serializedObject.FindProperty("_onCancel");
            EditorGUILayout.PropertyField(onCancel);

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(template);
            }
        }
    }
}