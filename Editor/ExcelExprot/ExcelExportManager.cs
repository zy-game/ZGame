using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ZGame.Editor.ExcelExprot
{
    [SubPageSetting("Excel导出")]
    public class ExcelExportManager : SubPage
    {
        public override void OnGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(String.Empty, ZStyle.GUI_STYLE_ADD_BUTTON))
            {
                string path = EditorUtility.OpenFilePanel("选择文件", EditorPrefs.GetString("ExcelPath", Application.dataPath), "xlsx");
                if (path.IsNullOrEmpty())
                {
                    return;
                }

                ExcelList.instance.AddExporter(new Exporter(path));
                EditorPrefs.SetString("ExcelPath", Path.GetDirectoryName(path));
            }

            GUILayout.EndHorizontal();


            if (ExcelList.instance.exporters.Count == 0)
            {
                return;
            }

            GUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("导出列表", EditorStyles.boldLabel);


            for (int i = ExcelList.instance.exporters.Count - 1; i >= 0; i--)
            {
                Exporter exporter = ExcelList.instance.exporters[i];
                if (exporter is null)
                {
                    continue;
                }

                GUILayout.BeginHorizontal(ZStyle.ITEM_BACKGROUND_STYLE);
                GUILayout.Label(exporter.path);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.SETTING_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE))
                {
                    EditorManager.SwitchScene<ExportEditorWindow>(exporter);
                }

                if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.DELETE_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE))
                {
                    ExcelList.instance.RemoveExporter(exporter);
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
        }
    }
}