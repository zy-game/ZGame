using UnityEditor;
using UnityEngine;

namespace ZGame.Editor
{
    [PageConfig("多语言设置", typeof(RuntimeEditorWindow))]
    public class LanguageEditorWindow : ToolbarScene
    {
        public override void OnEnable(params object[] args)
        {
            LanguageConfig.instance.OnAwake();
        }

        public override void OnGUI()
        {
            for (int i = 0; i < LanguageConfig.instance.languageTempletes.Count; i++)
            {
                LanguageTemplete templete = LanguageConfig.instance.languageTempletes[i];
                GUILayout.BeginHorizontal(EditorStyles.helpBox);
                templete.name = EditorGUILayout.TextField("名称", templete.name);
                templete.filter = EditorGUILayout.TextField("语言简写", templete.filter, GUILayout.Width(300));
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.DELETE_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE))
                {
                    LanguageConfig.instance.languageTempletes.RemoveAt(i);
                    LanguageConfig.OnSave();
                }

                GUILayout.EndHorizontal();
            }
        }

        public override void SearchRightDrawing()
        {
            if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.ADD_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE))
            {
                LanguageConfig.instance.languageTempletes.Add(new LanguageTemplete());
                LanguageConfig.OnSave();
            }
        }
    }
}