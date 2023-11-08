using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ZGame.Editor
{
    public abstract class PageScene : IDisposable
    {
        public abstract string name { get; }
        public Docker docker { get; }
        public List<SubPageScene> SubScenes { get; set; } = new List<SubPageScene>();
        public bool foldout;

        public PageScene(Docker window)
        {
            docker = window;
        }

        public virtual void Dispose()
        {
        }

        public virtual void OnEnable()
        {
        }

        public virtual void OnDisable()
        {
        }

        public virtual void OnGUI(Rect rect)
        {
        }

        public bool OnDrwaingMeunItem(float offset, float width)
        {
            Rect contains = EditorGUILayout.BeginVertical();
            docker.BeginColor(docker.current.Equals(this) ? Color.cyan : GUI.color);
            foldout = docker.MenuFoldout(foldout, name, SubScenes.Count > 0);
            docker.EndColor();
            GUILayout.Space(5);
            docker.BeginColor(docker.current.Equals(this) ? ZStyle.inColor : ZStyle.outColor);
            GUILayout.BeginHorizontal();
            GUILayout.Space(-offset);
            GUILayout.Box("", ZStyle.GUI_STYLE_LINE, GUILayout.MaxWidth(width), GUILayout.Height(1));
            GUILayout.EndHorizontal();
            docker.EndColor();
            bool result = false;
            if (Event.current.type == EventType.MouseDown && contains.Contains(Event.current.mousePosition) && Event.current.button == 0)
            {
                result = true;
            }

            GUILayout.Space(5);
            GUILayout.EndVertical();
            return result;
        }
    }
}