using System;
using UnityEditor;
using UnityEngine;

namespace ZGame.Editor
{
    public class GUILayoutTools
    {
        public static void DrawLine(Color dDolor, float leftSpace = 0)
        {
            Color color = GUI.color;
            GUI.color = dDolor;
            if (leftSpace != 0)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(leftSpace);
            }

            GUILayout.Box("", ZStyle.GUI_STYLE_LINE, GUILayout.Height(1));
            GUI.color = color;
            if (leftSpace != 0)
            {
                GUILayout.EndHorizontal();
            }
        }

        public static void DrawBackground(Color dDolor, float height, float leftSpace = 0)
        {
            Color color = GUI.color;
            GUI.color = dDolor;
            if (leftSpace != 0)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(leftSpace);
            }

            GUILayout.Box("", ZStyle.GUI_STYLE_LINE, GUILayout.Height(height));
            if (leftSpace != 0)
            {
                GUILayout.EndHorizontal();
            }

            GUI.color = color;
            GUILayout.Space(-height);
        }


        private static bool isEdit = false;
        private static string newText = String.Empty;

        public static string DrawEditLabel(string text, GUIStyle style)
        {
            Rect rect = EditorGUILayout.BeginHorizontal();
            if (isEdit)
            {
                newText = EditorGUILayout.TextField("", newText);
                if (((Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.Escape) && Event.current.type == EventType.KeyDown)
                    || (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition) is false))
                {
                    isEdit = false;
                    text = newText;
                    Event.current.Use();
                    EditorWindow.focusedWindow.Repaint();
                }
            }
            else
            {
                EditorGUILayout.LabelField(text, style);
                if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition) && Event.current.button == 0 && Event.current.clickCount == 2)
                {
                    newText = text;
                    isEdit = true;
                    Event.current.Use();
                    EditorWindow.focusedWindow.Repaint();
                }
            }

            EditorGUILayout.EndHorizontal();

            return text;
        }
    }
}