using System;
using UnityEditor;
using UnityEngine;
using ZGame.Window;

namespace ZGame.Editor.PSD2GUI
{
    [CustomEditor(typeof(AutoContentSize))]
    public class AutoContentSizeEditor : CustomEditorWindow
    {
        private AutoContentSize autoContentSize;
        private Vector2 sizeDelta;
        private RectTransform source;

        public void OnEnable()
        {
            autoContentSize = target as AutoContentSize;
            source = autoContentSize.GetComponent<RectTransform>();
            sizeDelta = source.sizeDelta;
        }

        public override void OnInspectorGUI()
        {
            if (autoContentSize == null)
            {
                return;
            }

            EditorGUI.BeginChangeCheck();

            autoContentSize.target = (RectTransform)EditorGUILayout.ObjectField("Target", autoContentSize.target, typeof(RectTransform), true);
            autoContentSize.offset = EditorGUILayout.Vector2Field("Offset", autoContentSize.offset);
            autoContentSize.useMinSize = EditorGUILayout.Toggle("Use Min Size", autoContentSize.useMinSize);
            if (autoContentSize.useMinSize)
            {
                autoContentSize.minSize = EditorGUILayout.Vector2Field("Min Size", autoContentSize.minSize);
            }

            autoContentSize.useMaxSize = EditorGUILayout.Toggle("Use Max Size", autoContentSize.useMaxSize);
            if (autoContentSize.useMaxSize)
            {
                autoContentSize.maxSize = EditorGUILayout.Vector2Field("Max Size", autoContentSize.maxSize);
            }

            if (EditorGUI.EndChangeCheck() || sizeDelta != source.sizeDelta)
            {
                autoContentSize.Refresh();
                sizeDelta = source.sizeDelta;
                EditorUtility.SetDirty(autoContentSize);
            }
        }
    }
}