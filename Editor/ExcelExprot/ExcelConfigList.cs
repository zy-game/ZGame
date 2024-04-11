using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace ZGame.Editor.ExcelExprot
{
    [CreateAssetMenu(menuName = "ZGame/Create Excel Generic Config", fileName = "ExcelConfig.asset", order = 2)]
    public class ExcelConfigList : BaseConfig<ExcelConfigList>
    {
        [LabelText("���Ŀ¼")] public UnityEngine.Object output;
        [LabelText("������ʼ��")] public int dataRowIndex = 3;
        [LabelText("�ֶ�������")] public int typeRowIndex = 1;
        [LabelText("���������ռ�")] public string nameSpace;
        [LabelText("��ͷ������")] public int headerRowIndex = 0;
        [LabelText("�����ļ�����")] public ExportType exportType;

        [TableList, LabelText("Excel File List")]
        public List<ExcelImportOptions> exporters;

        public override void OnAwake()
        {
            if (exporters is null)
            {
                exporters = new List<ExcelImportOptions>();
            }
        }

        public void GenericAll()
        {
            EditorUtility.SetDirty(this);
            Generic(exporters.ToArray());
        }

        public void Generic(params ExcelImportOptions[] options)
        {
            if (options is null || options.Length == 0)
            {
                return;
            }

            foreach (var VARIABLE in options)
            {
                if (VARIABLE.selection is null || VARIABLE.selection.Count == 0)
                {
                    continue;
                }

                switch (exportType)
                {
                    case ExportType.Json:
                        foreach (var select in VARIABLE.selection)
                        {
                            ExportJson(VARIABLE.GetTable(select));
                        }

                        break;
                    case ExportType.Csharp:
                        foreach (var select in VARIABLE.selection)
                        {
                            ExportCSharpCode(VARIABLE.GetTable(select));
                        }

                        break;
                }
            }

            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }


        private void ExportCSharpCode(DataTable table)
        {
            using (ExportCsharpCodeCommand csharpCodeCommand = new ExportCsharpCodeCommand())
            {
                csharpCodeCommand.OnExecute(table, headerRowIndex, dataRowIndex, typeRowIndex, output, nameSpace);
            }
        }

        private void ExportJson(DataTable table)
        {
            using (ExportJsonFileCommand command = new ExportJsonFileCommand())
            {
                command.OnExecute(table, headerRowIndex, dataRowIndex, typeRowIndex, output);
            }
        }
    }
}