using System.Text;
using UnityEditor;
using UnityEngine;

namespace ZGame.Editor.LinkerEditor
{
    [SubPageSetting("Linker Editor", typeof(GlobalWindow), false, ".xml")]
    public class LinkerWindow : SubPage
    {
        public override void SearchRightDrawing()
        {
            if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.REFRESH_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE))
            {
                LinkerConfig.instance.Refresh();
                LinkerConfig.OnSave();
            }

            if (GUILayout.Button("Generic", ZStyle.HEADER_BUTTON_STYLE))
            {
                //生成UNITY的link文件
                LinkerConfig.instance.Generic();
                EditorUtility.DisplayDialog("Linker 生成", "Link 文件已生成", "OK");
            }
        }

        public override void OnShowHeaderRightMeun(object userData)
        {
            if (userData is Linker linker)
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("全选"), false, () =>
                {
                    linker.SelectAll();
                    LinkerConfig.OnSave();
                });
                menu.AddItem(new GUIContent("反选"), false, () =>
                {
                    linker.UnselectAll();
                    LinkerConfig.OnSave();
                });
                menu.ShowAsContext();
            }

            if (userData is LinkNameSpace linkerNameSpace)
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("全选"), false, () =>
                {
                    linkerNameSpace.SelectAll();
                    LinkerConfig.OnSave();
                });
                menu.AddItem(new GUIContent("反选"), false, () =>
                {
                    linkerNameSpace.UnselectAll();
                    LinkerConfig.OnSave();
                });
                menu.ShowAsContext();
            }
        }

        public override void OnGUI()
        {
            LinkerConfig.instance.assemblies.Sort((x, y) => x.name.CompareTo(y.name));
            for (var i = 0; i < LinkerConfig.instance.assemblies.Count; i++)
            {
                DrawingLinker(LinkerConfig.instance.assemblies[i]);
            }
        }

        private void DrawingLinker(Linker linker)
        {
            if ((linker.enable = OnBeginHeader(linker.name, linker.enable, linker)) is false)
            {
                return;
            }

            GUILayout.BeginVertical(EditorStyles.helpBox);
            foreach (LinkNameSpace linkerNameSpace in linker.nameSpaces)
            {
                if (linkerNameSpace.enable = OnBeginHeader(linkerNameSpace.name, linkerNameSpace.enable, linkerNameSpace))
                {
                    GUILayout.BeginVertical(EditorStyles.helpBox);

                    foreach (LinkClass linkClass in linkerNameSpace.classes)
                    {
                        GUILayout.BeginHorizontal(EditorStyles.helpBox);
                        EditorGUI.BeginChangeCheck();
                        linkClass.isOn = GUILayout.Toggle(linkClass.isOn, $"{linkClass.nameSpace}.{linkClass.name}");
                        if (EditorGUI.EndChangeCheck())
                        {
                            LinkerConfig.OnSave();
                        }

                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();
                    }

                    GUILayout.EndVertical();
                }
            }

            GUILayout.EndVertical();
        }
    }
}