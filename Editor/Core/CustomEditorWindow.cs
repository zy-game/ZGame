using UnityEditor;
using UnityEngine;

namespace ZGame.Editor
{
    public class CustomEditorWindow : UnityEditor.Editor
    {
        public bool OnBeginShowHeader(string name, bool show2)
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            Rect rect = EditorGUILayout.BeginHorizontal();
            GUILayout.Space(10);
            show2 = EditorGUILayout.Foldout(show2, "");
            GUILayout.Space(-50);
            EditorGUILayout.LabelField(name, EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                show2 = !show2;
                Event.current.Use();
            }

            GUILayout.EndHorizontal();
            if (show2)
            {
                GUILayout.Space(5);
            }

            return show2;
        }

  
        
    }
}