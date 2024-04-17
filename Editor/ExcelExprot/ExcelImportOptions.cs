using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace ZGame.Editor.ExcelExprot
{
    public enum ExportType
    {
        Json,
        Csharp,
    }

    [Serializable]
    public class ExcelImportOptions
    {
        [Sirenix.OdinInspector.FilePath()] public string path;

        [Selector("LoadTableList"), LabelText("$GetSelectionTableList")]
        public List<string> selection;

        private List<DataTable> _tables;

        private List<string> LoadTableList()
        {
            if (_tables is null)
            {
                LoadTable();
            }

            return _tables.Select(x => x.TableName).ToList();
        }

        private string GetSelectionTableList()
        {
            if (selection is null || selection.Count == 0)
            {
                return "None";
            }

            return string.Join(",", selection);
        }

        public DataTable GetTable(string name)
        {
            if (_tables is null)
            {
                LoadTable();
            }

            return _tables.Find(x => x.TableName == name);
        }

        private void LoadTable()
        {
            if (path.EndsWith(".json"))
            {
                using (ConvertJsonDataCommand convert = new ConvertJsonDataCommand())
                {
                    _tables = convert.OnExecute(path);
                }
            }
            else
            {
                using (ConvertExcelFileDataCommand convert = new ConvertExcelFileDataCommand())
                {
                    _tables = convert.OnExecute(path);
                }
            }
        }
    }
}