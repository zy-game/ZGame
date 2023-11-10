using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ZGame.Editor
{
    public partial class Docker : EditorWindow
    {
        private List<PageScene> _scenes = new List<PageScene>();
        public PageScene current { get; private set; }
        private float leftWidth = 300;
        private float leftSpaceWidth = 5;
        private float rightSpace = 5;
        private float leftSpace => leftWidth + leftSpaceWidth + rightSpace;
        private Vector2 menuRoll;
        private Vector2 pageRoll;
        private string scarch;

        private void OnEnable()
        {
            current?.OnEnable();
        }

        private void OnDestroy()
        {
            Debug.Log("窗口关闭");
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
            GUILayout.Space(leftSpaceWidth);
            DrawingPageScene();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawingPageScene()
        {
            GUILayout.BeginVertical(ZStyle.GUI_STYLE_BOX_BACKGROUND, GUILayout.Width(position.width - 305), GUILayout.Height(position.height));
            
            GUILayout.BeginHorizontal();
            int size = EditorStyles.boldLabel.fontSize;
            EditorStyles.boldLabel.fontSize = 20;
            GUILayout.Label(current.name, EditorStyles.boldLabel);
            EditorStyles.boldLabel.fontSize = size;
            GUILayout.FlexibleSpace();
            scarch = EditorGUILayout.TextField(scarch, EditorStyles.toolbarSearchField, GUILayout.Width(300));
            GUILayout.EndHorizontal();
            
            this.BeginColor(ZStyle.inColor);
            GUILayout.Box("", ZStyle.GUI_STYLE_LINE, GUILayout.Height(1), GUILayout.Width(position.width - 305));
            this.EndColor();
            GUILayout.Space(5);
            
            pageRoll = GUILayout.BeginScrollView(pageRoll);
            current.OnGUI(scarch, new Rect(position.x + leftSpace, position.y, position.width - 325, position.height));
            GUILayout.EndScrollView();
            
            GUILayout.EndVertical();
        }

        private void DrawingMenuList()
        {
            GUILayout.BeginVertical(ZStyle.GUI_STYLE_BOX_BACKGROUND, GUILayout.MaxWidth(leftWidth), GUILayout.Height(position.height));
            menuRoll = GUILayout.BeginScrollView(menuRoll);
            foreach (var VARIABLE in _scenes)
            {
                if (VARIABLE.OnDrwaingMeunItem(0, leftWidth))
                {
                    SwitchPageScene(VARIABLE);
                }

                if (VARIABLE.SubScenes.Count == 0 || VARIABLE.foldout is false)
                {
                    continue;
                }

                GUILayout.BeginHorizontal();
                GUILayout.Space(20);
                GUILayout.BeginVertical();
                foreach (var subScene in VARIABLE.SubScenes)
                {
                    if (subScene.OnDrwaingMeunItem(20, leftWidth))
                    {
                        SwitchPageScene(subScene);
                    }
                }

                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        public void SwitchPageScene(PageScene pageScene)
        {
            current?.OnDisable();
            current = pageScene;
            current.OnEnable();
            this.Repaint();
        }
    }
}