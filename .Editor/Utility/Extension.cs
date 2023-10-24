using System;
using UnityEditor;
using UnityEngine;

namespace ZEngine.Editor
{
    static public class Extension
    {
        private static Color _color;

        public static void BeginColor(this EditorWindow window, Color color, Action action)
        {
            _color = GUI.color;
            GUI.color = color;
            action?.Invoke();
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
    }
}