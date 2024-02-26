using System;
using UnityEditor;
using UnityEngine;
using ZGame.Editor.PSD2GUI;

namespace ZGame.Editor.PSD2GUI
{
    [PageConfig("UIBind Rule", typeof(PSD2GUIWindow), false, typeof(UIBindRulerConfig))]
    public class UIBindRulerEditor : ToolbarScene
    {
        private bool nameSpaceFoldout = true;
        private bool typeFoldout = true;

        // public override void OnEnable(params object[] args)
        // {
        //     UIBindRulerConfig.instance.OnAwake();
        //     UIBindRulerConfig.instance.rules.Sort((a, b) => a.isDefault ? -1 : 1);
        //     UIBindRulerConfig.instance.nameSpaces.Sort((a, b) => a.isDefault ? -1 : 1);
        // }

        public override void OnDrawingHeaderRight(object userData)
        {
            if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.ADD_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE))
            {
                int type = (int)userData;
                switch (type)
                {
                    case 1:
                        UIBindRulerConfig.instance.rules.Add(new UIBindRulerItem());
                        ToolsWindow.Refresh();
                        break;
                    case 2:
                        UIBindRulerConfig.instance.nameSpaces.Add(new ReferenceNameSpace());
                        ToolsWindow.Refresh();
                        break;
                }

                UIBindRulerConfig.instance.AddNameSpace(string.Empty);
                ToolsWindow.Refresh();
            }
        }

        public override void OnGUI()
        {
            nameSpaceFoldout = OnBeginHeader("Refrence NameSpace", nameSpaceFoldout, 2);
            if (nameSpaceFoldout)
            {
                for (int i = UIBindRulerConfig.instance.nameSpaces.Count - 1; i >= 0; i--)
                {
                    ReferenceNameSpace item = UIBindRulerConfig.instance.nameSpaces[i];
                    EditorGUI.BeginDisabledGroup(item.isDefault);
                    GUILayout.BeginHorizontal();
                    item.nameSpace = EditorGUILayout.TextField("Element " + i, item.nameSpace, GUILayout.Width(500));
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.DELETE_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE))
                    {
                        UIBindRulerConfig.instance.nameSpaces.Remove(item);
                        ToolsWindow.Refresh();
                    }

                    GUILayout.EndHorizontal();
                    EditorGUI.EndDisabledGroup();
                    OnDrawingSplitLine(position.width, new Color(0, 0, 0, 0.5f));
                    GUILayout.Space(2);
                }

                GUILayout.Space(3);
            }

            typeFoldout = OnBeginHeader("Rulers", typeFoldout, 1);
            if (typeFoldout)
            {
                for (int i = UIBindRulerConfig.instance.rules.Count - 1; i >= 0; i--)
                {
                    UIBindRulerItem item = UIBindRulerConfig.instance.rules[i];

                    EditorGUI.BeginDisabledGroup(item.isDefault);
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("组件路径");
                    item.fullName = EditorGUILayout.TextField(item.fullName, GUILayout.Width(200));
                    GUILayout.Label("前缀");
                    item.prefix = EditorGUILayout.TextField(item.prefix, GUILayout.Width(200));
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.DELETE_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE))
                    {
                        UIBindRulerConfig.instance.rules.Remove(item);
                        ToolsWindow.Refresh();
                    }

                    GUILayout.EndHorizontal();
                    EditorGUI.EndDisabledGroup();
                    OnDrawingSplitLine(position.width, new Color(0, 0, 0, 0.5f));
                    GUILayout.Space(2);
                }
            }

            if (Event.current.type == EventType.KeyDown && Event.current.control && Event.current.keyCode == KeyCode.S)
            {
                UIBindRulerConfig.OnSave();
                ToolsWindow.Refresh();
            }
        }
    }
}