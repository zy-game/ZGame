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
        private Dictionary<Type, Action<Object>> map = new Dictionary<Type, Action<Object>>();
        private float leftWidth = 200;
        private float leftSpaceWidth = 5;
        private float rightSpace = 5;
        private float leftSpace => leftWidth + leftSpaceWidth + rightSpace;

        public void SetupOpenAssetCallback(Type t, Action<Object> callback)
        {
            if (map.ContainsKey(t) is false)
            {
                map.Add(t, args => { });
            }

            map[t] += callback;
        }

        private void OnEnable()
        {
            current?.OnEnable();
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
            GUILayout.BeginVertical(ZStyle.GUI_STYLE_BOX_BACKGROUND, GUILayout.Width(position.width - leftSpace), GUILayout.Height(position.height));

            int size = EditorStyles.boldLabel.fontSize;
            EditorStyles.boldLabel.fontSize = 20;
            GUILayout.Label(current.name, EditorStyles.boldLabel);
            EditorStyles.boldLabel.fontSize = size;
            this.BeginColor(ZStyle.inColor);
            GUILayout.Box("", ZStyle.GUI_STYLE_LINE, GUILayout.Height(1), GUILayout.Width(position.width - leftSpace));
            this.EndColor();
            current.OnGUI(new Rect(position.x + leftSpace, position.y, position.width - leftSpace, position.height));
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawingMenuList()
        {
            GUILayout.BeginVertical(ZStyle.GUI_STYLE_BOX_BACKGROUND, GUILayout.MaxWidth(leftWidth), GUILayout.Height(position.height));
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