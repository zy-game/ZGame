using UnityEditor;
using UnityEngine;

namespace ZEngine.Editor
{
    static public class EditorWindowExtension
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
    }
}