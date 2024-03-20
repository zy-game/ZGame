using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using ExcelDataReader;
using UnityEditor;
using UnityEngine;

namespace ZGame.Editor.ExcelExprot
{
    [GameSubEditorWindowOptions("Excel Editor", typeof(ExcelExportManager), true)]
    public class ExportEditorWindow : GameSubEditorWindow
    {
        private ExcelFileObject _fileObject;
        private ExcelTable options;
        private int index = 0;
        private GUIContent[] tableNames;

        public override void OnEnable(params object[] args)
        {
            if (args == null || args.Length == 0)
            {
                return;
            }

            _fileObject = args[0] as ExcelFileObject;
            if (_fileObject is null)
            {
                return;
            }

            _fileObject.LoadFile();
            options = _fileObject.options.First();
            tableNames = _fileObject.options.Select(x => new GUIContent(x.table)).ToArray();
        }

        public override void OnGUI()
        {
            if (_fileObject is null || _fileObject.options is null || _fileObject.options.Count == 0 || options is null)
            {
                return;
            }

            index = GUILayout.Toolbar(index, tableNames, EditorStyles.toolbarButton);
            if (index < 0 || index >= _fileObject.options.Count)
            {
                return;
            }

            options = _fileObject.options[index];
            GUILayout.BeginVertical();
            
            options.nameSpace = EditorGUILayout.TextField("命名空间", options.nameSpace);
            options.headerRow = EditorGUILayout.IntField("表头行", options.headerRow);
            options.typeRow = EditorGUILayout.IntField("字段类型行", options.typeRow);
            options.dataRow = EditorGUILayout.IntField("数据起始行", options.dataRow);
            options.isExport = EditorGUILayout.Toggle("是否导出", options.isExport);
            options.name = EditorGUILayout.TextField("导出文件名", options.name);
            options.type = (ExportType)EditorGUILayout.EnumPopup("导出类型", options.type);
            if (options.type == ExportType.Csharp)
            {
                options.code = (UnityEngine.Object)EditorGUILayout.ObjectField("代码保存路径", options.code, typeof(UnityEngine.Object), false);
            }

            if (GUILayout.Button("Generic"))
            {
                ExcelConfigList.instance.Generic(options);
            }

            GUILayout.EndVertical();

            GUILayout.BeginHorizontal(EditorStyles.helpBox);
            DataRow tableRow = options.dataTable.Rows[options.headerRow];
            int maxColumn = 0;
            if (tableRow is not null)
            {
                foreach (var VARIABLE in tableRow.ItemArray)
                {
                    if (VARIABLE.ToString().Equals("#") || VARIABLE.ToString().Equals(String.Empty))
                    {
                        continue;
                    }

                    EditorGUILayout.LabelField(new GUIContent(VARIABLE.ToString()), EditorStyles.boldLabel, GUILayout.Width(60));
                    GUILayout.Box("", GUILayout.Width(2));
                    maxColumn++;
                }
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            tableRow = options.dataTable.Rows[options.typeRow];

            GUILayout.BeginHorizontal(EditorStyles.helpBox);
            if (tableRow is not null)
            {
                foreach (var VARIABLE in tableRow.ItemArray)
                {
                    if (VARIABLE.ToString().Equals("#") || VARIABLE.ToString().Equals(String.Empty))
                    {
                        continue;
                    }

                    EditorGUILayout.LabelField(new GUIContent(VARIABLE.ToString()), EditorStyles.boldLabel, GUILayout.Width(60));
                    GUILayout.Box("", GUILayout.Width(2));
                }
            }


            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();


            GUILayout.BeginVertical(EditorStyles.helpBox);
            for (int i = options.dataRow; i < options.dataTable.Rows.Count; i++)
            {
                if (i > 30)
                {
                    continue;
                }

                tableRow = options.dataTable.Rows[i];
                if (tableRow is null)
                {
                    continue;
                }

                GUILayout.BeginHorizontal();
                for (int j = 0; j < maxColumn; j++)
                {
                    GUILayout.Label(new GUIContent(tableRow[j].ToString()), GUILayout.Width(60));
                    GUILayout.Box("", GUILayout.Width(2));
                }

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
        }

        public override void SaveChanges()
        {
            ExcelConfigList.Save();
        }
    }
}