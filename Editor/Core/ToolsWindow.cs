using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ZGame.Editor
{
    public partial class ToolsWindow : EditorWindow
    {
        private Vector2 menuRoll;
        private Vector2 pageRoll;
        private float leftWidth = 300;
        public ToolbarScene current { get; private set; }
        public string search { get; private set; }

        private void OnEnable()
        {
            current?.OnEnable();
        }

        private void OnDestroy()
        {
            _docker = null;
        }

        private void OnGUI()
        {
            if (current == null)
            {
                return;
            }

            EditorGUILayout.BeginHorizontal();
            DrawingMenuList();
            GUILayout.Space(5);
            DrawingPageScene();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawingPageScene()
        {
            GUILayout.BeginVertical(ZStyle.BOX_BACKGROUND, GUILayout.Width(position.width - 305), GUILayout.Height(position.height));
            GUILayout.BeginHorizontal();
            if (current is not null && current.parent is not null)
            {
                GUILayout.Space(5);
                if (GUILayout.Button(EditorGUIUtility.IconContent("d_tab_prev"), ZStyle.HEADER_BUTTON_STYLE))
                {
                    ToolsWindow.SwitchScene(current.parent);
                }
            }


            int size = EditorStyles.boldLabel.fontSize;
            EditorStyles.boldLabel.fontSize = 20;
            GUILayout.Label(current.name, EditorStyles.boldLabel);
            EditorStyles.boldLabel.fontSize = size;

            GUILayout.FlexibleSpace();
            search = EditorGUILayout.TextField(search, EditorStyles.toolbarSearchField, GUILayout.Width(300));
            if (Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.Return && search.IsNullOrEmpty() is false)
            {
                current.search = search;
                search = String.Empty;
                Event.current.Use();
            }

            current?.SearchRightDrawing();
            GUILayout.EndHorizontal();

            this.BeginColor(ZStyle.inColor);
            GUILayout.Box("", ZStyle.GUI_STYLE_LINE, GUILayout.Height(1), GUILayout.Width(position.width - 305));
            this.EndColor();
            GUILayout.Space(5);

            pageRoll = GUILayout.BeginScrollView(pageRoll);
            current.position = new Rect(position.x + leftWidth + 5, position.y, position.width - 325, position.height);
            current.search = search;

            current.OnGUI();
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        private void DrawingMenuList()
        {
            GUILayout.BeginVertical(ZStyle.BOX_BACKGROUND, GUILayout.MaxWidth(300), GUILayout.Height(position.height));
            menuRoll = GUILayout.BeginScrollView(menuRoll);
            foreach (var VARIABLE in sceneMaps)
            {
                VARIABLE.OnDrawingMeunItem(current, 0);
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }
    }
}