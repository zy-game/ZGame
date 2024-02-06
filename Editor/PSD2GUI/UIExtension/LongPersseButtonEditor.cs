using UnityEngine;
using UnityEditor;
using ZGame.UI;

namespace ZGame.Editor.PSD2GUI
{
    [CustomEditor(typeof(LongPresseButton))]
    public class LongPersseButtonEditor : BasicWindow
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

            SerializedProperty onDown = serializedObject.FindProperty("onDown");
            if (onDown != null)
            {
                EditorGUILayout.PropertyField(onDown);
            }

            SerializedProperty onUp = serializedObject.FindProperty("onUp");
            if (onUp != null)
            {
                EditorGUILayout.PropertyField(onUp);
            }

            SerializedProperty onCancel = serializedObject.FindProperty("onCancel");
            if (onCancel != null)
            {
                EditorGUILayout.PropertyField(onCancel);
            }

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(template);
            }
        }
    }
}