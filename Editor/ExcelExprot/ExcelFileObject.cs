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
        Csharp,
    }

    [Serializable]
    public class ExcelFileObject
    {
        public string path;
        public string name;
        public List<ExcelTable> options;

        public ExcelFileObject(string path)
        {
            FileInfo fileInfo = new FileInfo(path);
            this.path = path;
            this.name = Path.GetFileNameWithoutExtension(fileInfo.Name);
        }

        public DataTable GetTable(string name)
        {
            Debug.Log("find:" + name + "  " + string.Join(",", options.Select(x => x.name)));
            ExcelTable o = options.Find(x => x.name == name);
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

                ExcelTable options = this.options.Find(x => x.table == table.TableName);
                if (options is null)
                {
                    this.options.Add(options = new ExcelTable()
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

            ExcelConfigList.Save();
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