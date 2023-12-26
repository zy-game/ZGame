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
    [SubPageSetting("Excel Editor", typeof(ExcelExportManager))]
    public class ExportEditorWindow : SubPage
    {
        private Exporter exporter;
        private ExportSet options;
        private int index = 0;
        private GUIContent[] tableNames;

        public override void OnEnable(params object[] args)
        {
            if (args == null || args.Length == 0)
            {
                return;
            }

            exporter = args[0] as Exporter;
            exporter?.LoadFile();
            options = exporter?.options?.First();
            tableNames = exporter?.options?.Select(x => new GUIContent(x.table)).ToArray();
        }

        public override void OnGUI()
        {
            if (exporter is null || exporter.options is null || exporter.options.Count == 0 || options is null)
            {
                return;
            }

            // GUILayout.BeginHorizontal();
            // GUILayout.Label("表名");
            // if (EditorGUILayout.DropdownButton(new GUIContent(options.table), FocusType.Passive, GUILayout.Width(200)))
            // {
            //     GenericMenu menu = new GenericMenu();
            //     for (int i = 0; i < exporter.options.Count; i++)
            //     {
            //         ExportSet set = exporter.options[i];
            //         if (set is null)
            //         {
            //             continue;
            //         }
            //
            //         menu.AddItem(new GUIContent(set.table), false, () => { this.options = set; });
            //     }
            //
            //     menu.ShowAsContext();
            // }
            //
            // GUILayout.FlexibleSpace();
            // GUILayout.EndHorizontal();

            index = GUILayout.Toolbar(index, tableNames, EditorStyles.toolbarButton);
            if (index < 0 || index >= exporter.options.Count)
            {
                return;
            }

            options = exporter.options[index];
            GUILayout.BeginVertical();
            options.nameSpace = EditorGUILayout.TextField("命名空间", options.nameSpace);
            options.headerRow = EditorGUILayout.IntField("表头行", options.headerRow);
            options.dataRow = EditorGUILayout.IntField("数据起始行", options.dataRow);
            options.typeRow = EditorGUILayout.IntField("字段类型起始行", options.typeRow);
            options.isExport = EditorGUILayout.Toggle("Export", options.isExport);
            options.name = EditorGUILayout.TextField("文件名", options.name);
            options.type = (ExportType)EditorGUILayout.EnumPopup("导出类型", options.type);
            options.output = (UnityEngine.Object)EditorGUILayout.ObjectField("输出路径", options.output, typeof(UnityEngine.Object), false);
            if (GUILayout.Button("Generic"))
            {
                exporter.Export(options);
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
    }
}