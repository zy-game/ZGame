using UnityEditor;
using UnityEngine;

namespace ZGame.Editor
{
    [GameSubEditorWindowOptions("多语言设置", typeof(RuntimeEditorWindow))]
    public class LanguageEditorWindow : GameSubEditorWindow
    {
        public override void OnEnable(params object[] args)
        {
            LanguageConfig.instance.OnAwake();
        }

        public override void OnGUI()
        {
            for (int i = 0; i < LanguageConfig.instance.lanList.Count; i++)
            {
                LanguageOptions options = LanguageConfig.instance.lanList[i];
                GUILayout.BeginHorizontal(EditorStyles.helpBox);
                options.name = EditorGUILayout.TextField("名称", options.name);
                options.filter = EditorGUILayout.TextField("语言简写", options.filter, GUILayout.Width(300));
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.DELETE_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE))
                {
                    LanguageConfig.instance.lanList.RemoveAt(i);
                    LanguageConfig.Save();
                }

                GUILayout.EndHorizontal();
            }
        }

        public override void SaveChanges()
        {
            LanguageConfig.Save();
        }

        public override void SearchRightDrawing()
        {
            if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.ADD_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE))
            {
                LanguageConfig.instance.lanList.Add(new LanguageOptions());
                LanguageConfig.Save();
            }
        }
    }
}