using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using ExcelDataReader;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace ZGame.Editor.ExcelExprot
{
    public enum ExportType
    {
        Json,
        Csharp
    }

    [Serializable]
    public class ExportOptions
    {
        public string parent;
        public UnityEngine.Object output;
        public UnityEngine.Object code;
        public string name;
        public ExportType type;
        public string table;
        public bool isExport;
        public int dataRow;
        public int typeRow;
        public string nameSpace;
        public int headerRow;
        [NonSerialized] public DataTable dataTable;
    }

    [Serializable]
    public class ExcelExporter
    {
        public string path;
        public string name;
        public List<ExportOptions> options;

        public ExcelExporter(string path)
        {
            FileInfo fileInfo = new FileInfo(path);
            this.path = path;
            this.name = Path.GetFileNameWithoutExtension(fileInfo.Name);
        }

        public DataTable GetTable(string name)
        {
            Debug.Log("find:" + name + "  " + string.Join(",", options.Select(x => x.name)));
            ExportOptions o = options.Find(x => x.name == name);
            if (o is not null && o.dataTable is null)
            {
                LoadFile();
            }

            return o.dataTable;
        }

        public void LoadFile()
        {
            DataTableCollection collection = default;
            if (path.EndsWith(".json"))
            {
                collection = JsonToExcel();
            }
            else
            {
                using (var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        var result = reader.AsDataSet();
                        collection = result.Tables;
                    }
                }
            }

            if (options is null)
            {
                options = new();
            }

            for (int i = 0; i < collection.Count; i++)
            {
                DataTable table = collection[i];
                if (table is null)
                {
                    continue;
                }

                ExportOptions options = this.options.Find(x => x.table == table.TableName);
                if (options is null)
                {
                    this.options.Add(options = new ExportOptions()
                    {
                        table = table.TableName,
                        name = table.TableName,
                        type = ExportType.Csharp,
                        isExport = false,
                        dataRow = 3,
                        headerRow = 0,
                    });
                }

                options.parent = name;
                options.dataTable = table;
            }

            ExcelExportList.OnSave();
        }

        private DataTableCollection JsonToExcel()
        {
            if (path.EndsWith(".json") is false)
            {
                return null;
            }

            string fileName = Path.GetFileNameWithoutExtension(path);
            DataSet dataSet = new DataSet();
            DataTable dataTable = new DataTable(fileName);
            dataSet.Tables.Add(dataTable);
            var data = JsonConvert.DeserializeObject<ArrayList>(File.ReadAllText(path));
            foreach (var v in data)
            {
                var obj = v as JObject;
                List<string> title = new List<string>();
                foreach (var d in obj)
                {
                    title.Add(d.Key);
                    dataTable.Columns.Add(d.Key);
                }

                dataTable.Rows.Add(title.ToArray());
                break;
            }

            foreach (var v in data)
            {
                var obj = v as JObject;
                List<string> title = new List<string>();
                foreach (var d in obj)
                {
                    title.Add(d.Value.ToString());
                }

                dataTable.Rows.Add(title.ToArray());
            }

            return dataSet.Tables;
        }
    }
}