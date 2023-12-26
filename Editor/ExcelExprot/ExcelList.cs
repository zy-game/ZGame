using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using ExcelDataReader;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace ZGame.Editor.ExcelExprot
{
    [ResourceReference("Assets/Settings/ExcelConfig.asset")]
    public class ExcelList : SingletonScriptableObject<ExcelList>
    {
        public List<Exporter> exporters;

        public override void OnAwake()
        {
            if (exporters is null)
            {
                exporters = new List<Exporter>();
            }
        }

        public void AddExporter(Exporter exporter)
        {
            exporters.Add(exporter);
            OnSave();
        }

        public void RemoveExporter(Exporter exporter)
        {
            exporters.Remove(exporter);
            OnSave();
        }
    }

    public enum ExportType
    {
        Json,
        Asset
    }

    [Serializable]
    public class ExportSet
    {
        public UnityEngine.Object output;
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
    public class Exporter
    {
        public string path;
        public string name;
        public List<ExportSet> options;

        public Exporter(string path)
        {
            FileInfo fileInfo = new FileInfo(path);
            this.path = path;
            this.name = Path.GetFileNameWithoutExtension(fileInfo.Name);
        }

        public DataTableCollection LoadFile()
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
                if (table is null || table.TableName.StartsWith("#") || table.TableName.StartsWith("@"))
                {
                    continue;
                }

                ExportSet set = options.Find(x => x.table == table.TableName);
                if (set is not null)
                {
                    set.dataTable = table;
                    continue;
                }

                options.Add(new ExportSet()
                {
                    table = table.TableName,
                    name = table.TableName,
                    type = ExportType.Asset,
                    isExport = false,
                    dataRow = 3,
                    headerRow = 0,
                    dataTable = table
                });
            }

            return collection;
        }

        private DataTableCollection JsonToExcel()
        {
            if (path.EndsWith(".json"))
            {
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

            return null;
        }

        public void Export(ExportSet exportSet)
        {
            if (exportSet.type == ExportType.Json)
            {
                ExportJson(exportSet);
            }
            else
            {
                ExportCSharp(exportSet);
            }
        }

        private void ExportCSharp(ExportSet exportSet)
        {
            if (exportSet.isExport is false)
            {
                return;
            }

            DataRow header = exportSet.dataTable.Rows[exportSet.headerRow];
            if (header is null)
            {
                return;
            }

            DataRow typeRow = exportSet.dataTable.Rows[exportSet.typeRow];
            if (typeRow is null)
            {
                return;
            }


            StringBuilder sb = new StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine("using ZGame;");

            sb.AppendLine($"namespace {exportSet.nameSpace}");
            sb.AppendLine("{");
            sb.AppendLine("\t[Serializable]");
            sb.AppendLine($"\tpublic class {exportSet.name}");
            sb.AppendLine("\t{");
            for (int i = 0; i < header.ItemArray.Length; i++)
            {
                string name = header.ItemArray[i].ToString();
                if (name.Equals("#") || name.Equals(String.Empty))
                {
                    continue;
                }
                string t = typeRow[i].ToString();
                switch (t)
                {
                    case "int":
                        sb.AppendLine($"\t\tpublic int {header[i]};");
                        break;
                    case "float":
                        sb.AppendLine($"\t\tpublic float {header[i]};");
                        break;
                    case "bool":
                        sb.AppendLine($"\t\tpublic bool {header[i]};");
                        break;
                    default:
                        sb.AppendLine($"\t\tpublic string {header[i]};");
                        break;
                }
            }

            sb.AppendLine("\t}");
            string temp = AssetDatabase.GetAssetPath(exportSet.output);
            if (temp.IsNullOrEmpty() is false)
            {
                if (temp.Contains("Resources"))
                {
                    temp = temp.Substring(0, temp.LastIndexOf("."));
                    temp = temp.Substring(temp.IndexOf("Resources"));
                }

                sb.AppendLine($"\t[ResourceReference(\"{temp}\")]");
            }

            sb.AppendLine($"\tpublic sealed class {exportSet.name}List : SingletonScriptableObject<{exportSet.name}List>");
            sb.AppendLine("\t{");
            sb.AppendLine($"\t\tpublic List<{exportSet.name}> list;");
            sb.AppendLine("\t\tpublic override void OnAwake()");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t\tif (list is null)");
            sb.AppendLine("\t\t\t{");
            sb.AppendLine("\t\t\t\tlist = new List<" + exportSet.name + ">();");
            sb.AppendLine("\t\t\t}");
            sb.AppendLine("\t\t}");

            sb.AppendLine("\t}");
            sb.AppendLine("}");
            string path = Path.Combine(AssetDatabase.GetAssetPath(exportSet.output), exportSet.name + "List.cs");
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            Debug.Log("write code:" + path);
            File.WriteAllText(path, sb.ToString());

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Type type = Type.GetType(exportSet.nameSpace + "." + exportSet.name + "List");
            if (type is null)
            {
                return;
            }

            var obj = ScriptableObject.CreateInstance(type);
            AssetDatabase.CreateAsset(obj, Path.Combine(AssetDatabase.GetAssetPath(exportSet.output), exportSet.name + "List.asset"));

            IList list = type.GetField("list") as IList;

            Type itemType = Type.GetType(exportSet.nameSpace + "." + exportSet.name);
            for (int i = exportSet.dataRow; i < exportSet.dataTable.Rows.Count; i++)
            {
                var row = exportSet.dataTable.Rows[i];
                if (row.ToString().Equals("#") || row.ToString().Equals(String.Empty))
                {
                    continue;
                }
                var obj2 = Activator.CreateInstance(itemType);
                for (int j = 0; j < row.ItemArray.Length; j++)
                {
                    itemType.GetField(header[j].ToString()).SetValue(obj2, row[j]);
                }

                list.Add(obj2);
            }

            EditorUtility.SetDirty(obj);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void ExportJson(ExportSet exportSet)
        {
            if (exportSet.isExport is false)
            {
                return;
            }

            DataRow header = exportSet.dataTable.Rows[exportSet.headerRow];
            if (header is null)
            {
                return;
            }

            DataRow typeRow = exportSet.dataTable.Rows[exportSet.typeRow];
            if (typeRow is null)
            {
                return;
            }

            List<JObject> list = new List<JObject>();
            for (int rowIndex = exportSet.dataRow; rowIndex < exportSet.dataTable.Rows.Count; rowIndex++)
            {
                var row = exportSet.dataTable.Rows[rowIndex];
                JObject item = new JObject();
                for (int columnIndex = 0; columnIndex < row.ItemArray.Length; columnIndex++)
                {
                    string data = row.ItemArray[columnIndex].ToString();
                    string name = header.ItemArray[columnIndex].ToString();
                    if (name.Equals("#") || name.Equals(String.Empty))
                    {
                        continue;
                    }

                    if (int.TryParse(data, out int value))
                    {
                        item.Add(name, value);
                        continue;
                    }

                    if (float.TryParse(data, out float value2))
                    {
                        item.Add(name, value2);
                        continue;
                    }

                    if (bool.TryParse(data, out bool value3))
                    {
                        item.Add(name, value3);
                        continue;
                    }

                    item.Add(name, data);
                }

                list.Add(item);
            }

            string path = Path.Combine(AssetDatabase.GetAssetPath(exportSet.output), exportSet.name + ".json");
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            File.AppendAllText(path, JsonConvert.SerializeObject(list));
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}