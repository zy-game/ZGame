using System;
using UnityEditor;
using UnityEngine;

namespace ZGame.Editor.UIBindEditor
{
    [SubPageSetting("UIBind Rule")]
    public class UIBindRuler : SubPage
    {
        private bool nameSpaceFoldout = true;
        private bool typeFoldout = true;

        public override void OnGUI()
        {
            nameSpaceFoldout = OnShowFoldoutHeader("Refrence NameSpace", nameSpaceFoldout);
            if (nameSpaceFoldout)
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("", ZStyle.GUI_STYLE_ADD_BUTTON))
                {
                    UIBindRulerConfig.instance.nameSpaces.Add(String.Empty);
                }

                GUILayout.EndHorizontal();
                for (int i = UIBindRulerConfig.instance.nameSpaces.Count - 1; i >= 0; i--)
                {
                    GUILayout.BeginHorizontal();
                    UIBindRulerConfig.instance.nameSpaces[i] = EditorGUILayout.TextField("Element " + i, UIBindRulerConfig.instance.nameSpaces[i], GUILayout.Width(500));
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("", ZStyle.GUI_STYLE_MINUS))
                    {
                        UIBindRulerConfig.instance.nameSpaces.Remove(UIBindRulerConfig.instance.nameSpaces[i]);
                        EditorManager.Refresh();
                    }

                    GUILayout.EndHorizontal();
                }

                GUILayout.Space(10);
            }

            typeFoldout = OnShowFoldoutHeader("Rulers", typeFoldout);

            if (typeFoldout)
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("", ZStyle.GUI_STYLE_ADD_BUTTON))
                {
                    UIBindRulerConfig.instance.rules.Add(new UIBindRulerItem());
                    EditorManager.Refresh();
                }

                GUILayout.EndHorizontal();
                for (int i = UIBindRulerConfig.instance.rules.Count - 1; i >= 0; i--)
                {
                    UIBindRulerItem item = UIBindRulerConfig.instance.rules[i];

                    GUILayout.BeginHorizontal();
                    item.fullName = EditorGUILayout.TextField("Path", item.fullName);
                    item.prefix = EditorGUILayout.TextField("Prefix", item.prefix);
                    GUILayout.EndHorizontal();
                }
            }
        }
    }
}