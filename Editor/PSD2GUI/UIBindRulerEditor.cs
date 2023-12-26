using System;
using UnityEditor;
using UnityEngine;
using ZGame.Editor.PSD2GUI;

namespace ZGame.Editor.PSD2GUI
{
    [ReferenceScriptableObject(typeof(UIBindRulerConfig))]
    [SubPageSetting("UIBind Rule", typeof(PSD2GUIWindow))]
    public class UIBindRulerEditor : SubPage
    {
        private bool nameSpaceFoldout = true;
        private bool typeFoldout = true;

        // public override void OnEnable(params object[] args)
        // {
        //     UIBindRulerConfig.instance.OnAwake();
        //     UIBindRulerConfig.instance.rules.Sort((a, b) => a.isDefault ? -1 : 1);
        //     UIBindRulerConfig.instance.nameSpaces.Sort((a, b) => a.isDefault ? -1 : 1);
        // }

        public override void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            nameSpaceFoldout = OnShowFoldoutHeader("Refrence NameSpace", nameSpaceFoldout, () =>
            {
                UIBindRulerConfig.instance.AddNameSpace(string.Empty);
                EditorManager.Refresh();
            });
            if (nameSpaceFoldout)
            {
                bool isEnd = false;
                EditorGUI.BeginDisabledGroup(true);
                for (int i = UIBindRulerConfig.instance.nameSpaces.Count - 1; i >= 0; i--)
                {
                    ReferenceNameSpace item = UIBindRulerConfig.instance.nameSpaces[i];
                    if (item.isDefault is false)
                    {
                        if (isEnd is false)
                        {
                            EditorGUI.EndDisabledGroup();
                            isEnd = true;
                        }

                        continue;
                    }

                    GUILayout.BeginHorizontal();
                    item.nameSpace = EditorGUILayout.TextField("Element " + i, item.nameSpace, GUILayout.Width(500));
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.DELETE_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE))
                    {
                        UIBindRulerConfig.instance.nameSpaces.Remove(item);
                        EditorManager.Refresh();
                    }

                    GUILayout.EndHorizontal();
                }

                if (isEnd is false)
                {
                    EditorGUI.EndDisabledGroup();
                }

                GUILayout.Space(3);
            }

            typeFoldout = OnShowFoldoutHeader("Rulers", typeFoldout, () =>
            {
                UIBindRulerConfig.instance.rules.Add(new UIBindRulerItem());
                EditorManager.Refresh();
            });

            if (typeFoldout)
            {
                EditorGUI.BeginDisabledGroup(true);
                bool isEnd = false;

                for (int i = UIBindRulerConfig.instance.rules.Count - 1; i >= 0; i--)
                {
                    UIBindRulerItem item = UIBindRulerConfig.instance.rules[i];
                    if (item.isDefault is false)
                    {
                        if (isEnd is false)
                        {
                            EditorGUI.EndDisabledGroup();
                            isEnd = true;
                        }

                        continue;
                    }

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("组件类型");
                    item.type = (UIBindRulerType)EditorGUILayout.EnumPopup(item.type, GUILayout.Width(200));
                    GUILayout.Label("组件路径");
                    item.fullName = EditorGUILayout.TextField(item.fullName, GUILayout.Width(200));
                    GUILayout.Label("前缀");
                    item.prefix = EditorGUILayout.TextField(item.prefix, GUILayout.Width(200));
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.DELETE_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE))
                    {
                        UIBindRulerConfig.instance.rules.Remove(item);
                        EditorManager.Refresh();
                    }

                    GUILayout.EndHorizontal();
                }

                if (isEnd is false)
                {
                    EditorGUI.EndDisabledGroup();
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                UIBindRulerConfig.OnSave();
                EditorManager.Refresh();
            }
        }
    }
}