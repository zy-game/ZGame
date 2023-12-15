using UnityEditor;
using UnityEngine;

namespace ZGame.Editor
{
    public class CustomEditorWindow : UnityEditor.Editor
    {
        public bool OnShowFoldoutHeader(string name, bool show2)
        {
            Rect rect = EditorGUILayout.BeginHorizontal(ZStyle.GUI_STYLE_BOX_BACKGROUND);
            show2 = EditorGUILayout.Foldout(show2, "");
            GUILayout.Space(-40);
            EditorGUILayout.LabelField(name, EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                show2 = !show2;
                Event.current.Use();
            }

            GUILayout.EndHorizontal();
            return show2;
        }
    }
}