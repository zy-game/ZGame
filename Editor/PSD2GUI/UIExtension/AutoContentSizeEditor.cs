using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using ZGame.UI;

namespace ZGame.Editor.PSD2GUI
{
    [CustomEditor(typeof(AutoContentSize))]
    public class AutoContentSizeEditor : BasicWindow
    {
        private AutoContentSize _content;
        private RectTransform source;
        private ContentSizeFitter fitter;

        public void OnEnable()
        {
            _content = target as AutoContentSize;
            source = _content.GetComponent<RectTransform>();
            fitter = _content.GetComponent<ContentSizeFitter>();
            _content.Refresh();
        }

        public override void OnInspectorGUI()
        {
            if (_content == null)
            {
                return;
            }

            if (_content.targets == null)
            {
                _content.targets = new();
            }

            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Target List", EditorStyles.boldLabel);

                GUILayout.FlexibleSpace();
                if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.DELETE_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE))
                {
                    _content.targets.Clear();
                }

                if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.ADD_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE))
                {
                    _content.targets.Add(new AutoContentOptions());
                }

                GUILayout.EndHorizontal();
            }
            this.BeginColor(ZStyle.inColor);
            GUILayout.Box("", ZStyle.GUI_STYLE_LINE, GUILayout.Height(1));
            this.EndColor();


            for (int i = _content.targets.Count - 1; i >= 0; i--)
            {
                AutoContentOptions options = _content.targets[i];

                GUILayout.BeginVertical(EditorStyles.helpBox);
                options.HorizontalMode = (ContentSizeFitter.FitMode)EditorGUILayout.EnumPopup("Horizontal Mode", options.HorizontalMode);
                options.VerticalMode = (ContentSizeFitter.FitMode)EditorGUILayout.EnumPopup("Vertical Mode", options.VerticalMode);
                options.target = EditorGUILayout.ObjectField(new GUIContent("Target"), options.target, typeof(RectTransform), true) as RectTransform;
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.DELETE_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE))
                {
                    _content.targets.RemoveAt(i);
                }

                GUILayout.EndVertical();
            }

            GUILayout.EndVertical();

            _content.layout = (LayoutAxis)EditorGUILayout.EnumPopup("Layout", _content.layout);
            _content.offset = EditorGUILayout.Vector2Field("Offset", _content.offset);
            _content.useMinSize = (bool)EditorGUILayout.Toggle("Use Min Size", _content.useMinSize);
            EditorGUI.BeginDisabledGroup(!_content.useMinSize);
            _content.minSize = EditorGUILayout.Vector2Field("Min Size", _content.minSize);
            EditorGUI.EndDisabledGroup();

            _content.useMaxSize = (bool)EditorGUILayout.Toggle("Use Max Size", _content.useMaxSize);
            EditorGUI.BeginDisabledGroup(!_content.useMaxSize);
            _content.maxSize = EditorGUILayout.Vector2Field("Max Size", _content.maxSize);
            EditorGUI.EndDisabledGroup();
            _content.Refresh();
            if (Event.current.type == EventType.KeyDown && Event.current.control && Event.current.keyCode == KeyCode.S)
            {
                EditorUtility.SetDirty(_content);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }
    }
}