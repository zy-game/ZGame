using System;
using UnityEditor;
using UnityEngine;

namespace ZGame.Editor
{
    public static class Extension
    {
        private static Color _color;

        public static void BeginColor(this EditorWindow window, Color color)
        {
            _color = GUI.color;
            GUI.color = color;
        }

        public static void EndColor(this EditorWindow window)
        {
            GUI.color = _color;
        }

        public static void BeginColor(this UnityEditor.Editor window, Color color)
        {
            _color = GUI.color;
            GUI.color = color;
        }

        public static void EndColor(this UnityEditor.Editor window)
        {
            GUI.color = _color;
        }

        public static void OnMouseLeftButtonDown(this EditorWindow window, Rect contains, Action action)
        {
            if (Rect.zero.Equals(contains))
            {
                contains = window.position;
            }

            if (Event.current.type == EventType.MouseDown && contains.Contains(Event.current.mousePosition) && Event.current.button == 0)
            {
                action?.Invoke();
            }
        }

        public static void OnMouseRightButtomDown(this EditorWindow window, Rect contains, Action action)
        {
            if (Rect.zero.Equals(contains))
            {
                contains = window.position;
            }

            if (Event.current.type == EventType.MouseDown && contains.Contains(Event.current.mousePosition) && Event.current.button == 1)
            {
                action?.Invoke();
            }
        }

        public static void OnKeyboardButtomDown(this EditorWindow window, KeyCode keyCode, Action action)
        {
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode.Equals(keyCode))
            {
                action?.Invoke();
            }
        }

        public static bool MenuItem(this EditorWindow window, float width, bool showfoldout, bool selection, string name, ref bool foldout)
        {
            Rect contains = EditorGUILayout.BeginVertical();
            window.BeginColor(selection ? Color.cyan : GUI.color);
            GUILayout.BeginHorizontal();
            {
                if (showfoldout)
                {
                    GUILayout.BeginVertical(GUILayout.Width(20));
                    GUILayout.Space(3);
                    foldout = EditorGUILayout.Foldout(foldout, "");
                    GUILayout.EndVertical();
                    GUILayout.Space(-40);
                }

                GUILayout.Label(name, ZStyle.GUI_STYLE_TITLE_LABLE);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
            window.EndColor();
            GUILayout.Space(5);
            window.BeginColor(selection ? ZStyle.inColor : ZStyle.outColor);
            GUILayout.Box("", ZStyle.GUI_STYLE_LINE, GUILayout.MaxWidth(width), GUILayout.Height(1));
            window.EndColor();
            bool result = false;
            if (Event.current.type == EventType.MouseDown && contains.Contains(Event.current.mousePosition) && Event.current.button == 0)
            {
                result = true;
            }

            GUILayout.Space(5);
            GUILayout.EndVertical();
            return result;
        }

        public static bool MenuFoldout(this EditorWindow window,bool value, string name, bool showFoldout)
        {
            GUILayout.BeginHorizontal();
            if (showFoldout)
            {
                GUILayout.BeginVertical(GUILayout.Width(20));
                GUILayout.Space(3);
                value = EditorGUILayout.Foldout(value, "");
                GUILayout.EndVertical();
                GUILayout.Space(-40);
            }

            GUILayout.Label(name, ZStyle.GUI_STYLE_TITLE_LABLE);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            return value;
        }
    }
}