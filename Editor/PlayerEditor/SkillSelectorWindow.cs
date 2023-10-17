using System;
using UnityEditor;
using UnityEngine;
using ZEngine.Editor.SkillEditor;

namespace ZEngine.Editor.PlayerEditor
{
    class SkillSelectorWindow : PopupWindowContent
    {
        public Vector2 size;
        public PlayerOptions options;
        public PlayerEditorWindow playerEditorWindow;

        public override Vector2 GetWindowSize()
        {
            return size;
        }

        public override void OnGUI(Rect rect)
        {
            GUILayout.BeginVertical();
            foreach (var VARIABLE in SkillDataList.instance.optionsList)
            {
                GUILayout.BeginHorizontal(EditorStyles.helpBox);
                GUILayout.Label(AssetDatabase.LoadAssetAtPath<Texture2D>(VARIABLE.icon), GUILayout.Width(50), GUILayout.Height(50));
                GUILayout.BeginVertical();
                GUILayout.Label(VARIABLE.name);
                GUILayout.EndVertical();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(String.Empty, CustomWindowStyle.GUI_STYLE_ADD_BUTTON))
                {
                    var item = options.skills.Find(x => x == VARIABLE.id);
                    if (item > 0)
                    {
                        EditorUtility.DisplayDialog("错误", "该角色已添加过相同技能", "确定");
                        this.editorWindow.Close();
                        return;
                    }

                    options.skills.Add(VARIABLE.id);
                    playerEditorWindow.SaveChanges();
                    this.editorWindow.Close();
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
        }
    }
}