using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ZGame.Editor.ExcelExprot
{
    [PageConfig("Excel导出", null, false, typeof(ExcelExportList))]
    public class ExcelExportManager : ToolbarScene
    {
        public override void OnGUI()
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("导出列表", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.MORE_BUTTON_ICON), "FoldoutHeaderIcon"))
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("导入配置文件"), false, () =>
                {
                    string path = EditorUtility.OpenFilePanel("选择文件", EditorPrefs.GetString("ExcelPath", Application.dataPath), "xlsx");
                    if (path.IsNullOrEmpty())
                    {
                        return;
                    }

                    ExcelExportList.instance.AddExporter(new ExcelExporter(path));
                    EditorPrefs.SetString("ExcelPath", Path.GetDirectoryName(path));
                });
                menu.AddItem(new GUIContent("导出所有配置"), false, () =>
                {
                    List<ExportOptions> list = new List<ExportOptions>();
                    for (int i = 0; i < ExcelExportList.instance.exporters.Count; i++)
                    {
                        list.AddRange(ExcelExportList.instance.exporters[i].options);
                    }

                    ExcelExportList.instance.Generic(list.ToArray());
                });
                menu.ShowAsContext();
            }

            GUILayout.EndHorizontal();


            GUILayout.Space(5);
            for (int i = ExcelExportList.instance.exporters.Count - 1; i >= 0; i--)
            {
                ExcelExporter exporter = ExcelExportList.instance.exporters[i];
                if (exporter is null)
                {
                    continue;
                }

                GUILayout.BeginHorizontal(ZStyle.ITEM_BACKGROUND_STYLE);
                GUILayout.Label(exporter.path);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.SETTING_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE))
                {
                    ToolsWindow.SwitchScene<ExportEditorWindow>(exporter);
                }

                if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.DELETE_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE))
                {
                    ExcelExportList.instance.RemoveExporter(exporter);
                }

                if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.PLAY_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE))
                {
                    ExcelExportList.instance.Generic(exporter.options.ToArray());
                }

                GUILayout.EndHorizontal();
                GUILayout.Space(2);
            }

            GUILayout.EndVertical();
        }
    }
}