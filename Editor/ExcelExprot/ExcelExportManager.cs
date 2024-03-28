using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ZGame.Editor.ExcelExprot
{
    [GameSubEditorWindowOptions("Excel导出", null, false, typeof(ExcelConfigList))]
    public class ExcelExportManager : GameSubEditorWindow
    {
        public override void SearchRightDrawing()
        {
            if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.PLAY_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE))
            {
                List<ExcelTable> list = new List<ExcelTable>();
                for (int i = 0; i < ExcelConfigList.instance.exporters.Count; i++)
                {
                    list.AddRange(ExcelConfigList.instance.exporters[i].options);
                }

                ExcelConfigList.instance.Generic(list.ToArray());
            }
        }

        public override void OnGUI()
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("导出列表", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.ADD_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE))
            {
                string path = EditorUtility.OpenFilePanel("选择文件", EditorPrefs.GetString("ExcelPath", Application.dataPath), "xlsx");
                if (path.IsNullOrEmpty())
                {
                    return;
                }

                ExcelConfigList.instance.AddExporter(new ExcelFileObject(path));
                EditorPrefs.SetString("ExcelPath", Path.GetDirectoryName(path));
            }

            GUILayout.EndHorizontal();


            GUILayout.Space(5);
            for (int i = ExcelConfigList.instance.exporters.Count - 1; i >= 0; i--)
            {
                ExcelFileObject fileObject = ExcelConfigList.instance.exporters[i];
                if (fileObject is null)
                {
                    continue;
                }

                GUILayout.BeginHorizontal(ZStyle.ITEM_BACKGROUND_STYLE);
                GUILayout.Label(fileObject.path);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.SETTING_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE))
                {
                    GameBaseEditorWindow.SwitchScene<ExportEditorWindow>(fileObject);
                }

                if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.DELETE_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE))
                {
                    ExcelConfigList.instance.RemoveExporter(fileObject);
                }

                if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.PLAY_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE))
                {
                    ExcelConfigList.instance.Generic(fileObject.options.ToArray());
                }

                GUILayout.EndHorizontal();
                GUILayout.Space(2);
            }

            GUILayout.EndVertical();
        }

        public override void SaveChanges()
        {
            ExcelConfigList.Save();
        }
    }
}