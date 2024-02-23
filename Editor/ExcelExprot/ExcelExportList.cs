using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace ZGame.Editor.ExcelExprot
{
    [ResourceReference("Assets/Settings/ExcelConfig.asset")]
    public class ExcelExportList : SingletonScriptableObject<ExcelExportList>
    {
        public List<ExcelExporter> exporters;

        public override void OnAwake()
        {
            if (exporters is null)
            {
                exporters = new List<ExcelExporter>();
            }
        }

        public ExcelExporter GetExporter(string name)
        {
            return exporters.Find(x => x.name == name);
        }

        public void AddExporter(ExcelExporter exporter)
        {
            exporters.Add(exporter);
            OnSave();
        }

        public void RemoveExporter(ExcelExporter exporter)
        {
            exporters.Remove(exporter);
            OnSave();
        }

        public void GenericAll()
        {
            List<ExportOptions> ExportList = new List<ExportOptions>();
            foreach (var VARIABLE in exporters)
            {
                for (int i = 0; i < VARIABLE.options.Count; i++)
                {
                    if (VARIABLE.options[i].isExport is false)
                    {
                        continue;
                    }

                    ExportList.Add(VARIABLE.options[i]);
                }
            }

            OnSave();
            Generic(ExportList.ToArray());
        }

        public void Generic(params ExportOptions[] options)
        {
            if (options is null || options.Length == 0)
            {
                return;
            }

            foreach (var VARIABLE in options)
            {
                if (VARIABLE.dataTable is null)
                {
                    ExcelExporter exporter = GetExporter(VARIABLE.parent);
                    if (exporter is null)
                    {
                        return;
                    }

                    VARIABLE.dataTable = exporter.GetTable(VARIABLE.name);
                }

                switch (VARIABLE.type)
                {
                    case ExportType.Json:
                        ExportJson(VARIABLE);
                        break;
                    case ExportType.Csharp:
                        ExportCSharpCode(VARIABLE);
                        break;
                }
            }

            OnSave();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private string GetDataType(string t)
        {
            return t switch
            {
                "int" => "int",
                "float" => "float",
                "bool" => "bool",
                _ => "string"
            };
        }

        private string GetDefaultValue(string t, string name, string v)
        {
            return t switch
            {
                "int" => $"{name} = {v}, ",
                "float" => $"{name} = {v}, ",
                "bool" => $"{name} = {(v == "0")}, ",
                _ => $"{name} = @\"{v}\",",
            };
        }

        private string GetStructData(DataRow row, DataRow header, DataRow typeRow, int rowIndex)
        {
            string templete = "\t\t\tnew () {";
            for (int columnIndex = 0; columnIndex < row.ItemArray.Length; columnIndex++)
            {
                string data = row.ItemArray[columnIndex].ToString();
                string name = header.ItemArray[columnIndex].ToString();
                if (name.Equals("#") || name.Equals(String.Empty))
                {
                    continue;
                }

                templete += GetDefaultValue(typeRow[columnIndex].ToString(), name, data);
            }

            templete.Replace("\n", String.Empty);
            templete += "},";
            return templete;
        }

        private void ExportCSharpCode(ExportOptions exportSet)
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

            string assetTypeName = exportSet.name;
            string itemTypeName = exportSet.name + "_item";
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Linq;");
            sb.AppendLine("using System.Collections;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine("using ZGame;");
            sb.AppendLine("using ZGame.Config;");
            sb.AppendLine($"namespace {exportSet.nameSpace}");
            sb.AppendLine("{");
            sb.AppendLine($"\tpublic class {itemTypeName}");
            sb.AppendLine("\t{");
            for (int i = 0; i < header.ItemArray.Length; i++)
            {
                string name = header.ItemArray[i].ToString();
                if (name.Equals("#") || name.Equals(String.Empty))
                {
                    continue;
                }

                sb.AppendLine($"\t\tpublic {GetDataType(typeRow[i].ToString())} {header[i]} {{ get; set; }}");
            }

            sb.AppendLine("\t}");
            sb.AppendLine($"\tpublic sealed class {assetTypeName} : Singleton<{assetTypeName}>, IQuery<{itemTypeName}>");
            sb.AppendLine("\t{");
            sb.AppendLine($"\t\tpublic {itemTypeName} this[int index]");
            sb.AppendLine($"\t\t{{");
            sb.AppendLine($"\t\t\tget => cfgList[index];");
            sb.AppendLine($"\t\t}}");
            sb.AppendLine($"\t\tpublic int Count => cfgList.Count;");
            sb.AppendLine($"\t\tprivate List<{itemTypeName}> cfgList = new ()\n\t\t{{");

            for (int rowIndex = exportSet.dataRow; rowIndex < exportSet.dataTable.Rows.Count; rowIndex++)
            {
                var row = exportSet.dataTable.Rows[rowIndex];
                sb.AppendLine(GetStructData(row, header, typeRow, rowIndex));
            }

            sb.AppendLine("\t\t};");

            sb.AppendLine("");
            sb.AppendLine("\t\tpublic void Dispose()");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t\tcfgList.Clear();");
            sb.AppendLine("\t\t}");
            sb.AppendLine("");
            sb.AppendLine("\t\tobject IQuery.Query(int key)");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t\treturn Query(key);");
            sb.AppendLine("\t\t}");
            sb.AppendLine("");
            sb.AppendLine($"\t\tpublic {itemTypeName} Query({typeRow.ItemArray[0].ToString()} key)");
            sb.AppendLine("\t\t{");
            sb.AppendLine($"\t\t\treturn cfgList.Find(x => x.{header.ItemArray[0].ToString()} == key);");
            sb.AppendLine("\t\t}");
            sb.AppendLine("");
            sb.AppendLine($"\t\tpublic {itemTypeName} Query(Func<{itemTypeName}, bool> func)");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t\treturn cfgList.Find(x => func(x));");
            sb.AppendLine("\t\t}");
            sb.AppendLine("");
            sb.AppendLine("\t\tpublic object Query(Func<object, bool> whereFunc)");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t\treturn cfgList.Find(x => whereFunc(x));");
            sb.AppendLine("\t\t}");
            sb.AppendLine("");
            sb.AppendLine($"\t\tpublic List<{itemTypeName}> Wheres(Func<{itemTypeName}, bool> func)");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t\treturn cfgList.Where(x => func(x)).ToList();");
            sb.AppendLine("\t\t}");
            sb.AppendLine("");
            sb.AppendLine("\t\tpublic List<object> Wheres(Func<object, bool> whereFunc)");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t\tList<object> result = new List<object>();");
            sb.AppendLine("\t\t\tforeach (var item in cfgList)");
            sb.AppendLine("\t\t\t{");
            sb.AppendLine("\t\t\t\tif (whereFunc(item))");
            sb.AppendLine("\t\t\t\t{");
            sb.AppendLine("\t\t\t\t\tresult.Add(item);");
            sb.AppendLine("\t\t\t\t}");
            sb.AppendLine("\t\t\t}");
            sb.AppendLine("");
            sb.AppendLine("\t\t\treturn result;");
            sb.AppendLine("\t\t}");
            sb.AppendLine("\t}");
            sb.AppendLine("}");
            string path = Path.Combine(AssetDatabase.GetAssetPath(exportSet.code), assetTypeName + ".cs");
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            Debug.Log("write code:" + path);
            File.WriteAllText(path, sb.ToString());
        }

        private void ExportJson(ExportOptions exportSet)
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