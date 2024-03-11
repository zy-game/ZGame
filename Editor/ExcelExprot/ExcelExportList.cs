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
                "string" => "string",
                "int[]" => "int[]",
                "float[]" => "float[]",
                "bool[]" => "bool[]",
                "string[]" => "string[]",
                _ => "string"
            };
        }

        private string GetDefaultValue(string t, string name, string v)
        {
            string temp = $"{name} = ";
            switch (t)
            {
                case "int":
                case "float":
                    temp += $"{v}, ";
                    break;
                case "bool":
                    temp += $"{(v == "0" ? "false" : "true")}, ";
                    break;
                case "string":
                    temp += $"@\"{v}\", ";
                    break;
                case "int[]":
                    temp += $"new int [] {{{v}}}";
                    break;
                case "float[]":
                    temp += $"new float [] {{v}}";
                    break;
                case "bool[]":
                    string[] m = v.Split(",");
                    temp += $"new bool [] {{";
                    for (int i = 0; i < m.Length; i++)
                    {
                        temp += m[i] == "0" ? "false" : "true";
                        if (i != m.Length - 1)
                        {
                            temp += ", ";
                        }
                    }

                    temp += "}, ";
                    break;
                case "string[]":
                    temp += $"new string [] {{";
                    string[] m2 = v.Split(",");
                    for (int i = 0; i < m2.Length; i++)
                    {
                        temp += $"@\"{m2[i]}\"";
                        if (i != m2.Length - 1)
                        {
                            temp += ", ";
                        }
                    }

                    temp += "}, ";
                    break;
                default:
                    break;
            }

            return temp;
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


            // ExcelOutputTemplete templete = new ExcelOutputTemplete();
            // templete.TransformText();
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

            // CodeGen codeGen = new CodeGen(exportSet.name);
            // codeGen.AddReferenceNameSpace("System", "System.Linq", "System.Collections", "System.Collections.Generic", "UnityEngine", "ZGame");
            // codeGen.SetNameSpace(exportSet.nameSpace);

            // string assetTypeName = exportSet.name;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Linq;");
            sb.AppendLine("using System.Collections;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine("using ZGame;");
            sb.AppendLine($"namespace {exportSet.nameSpace}");
            sb.AppendLine("{");
            sb.AppendLine($"\tpublic sealed class {exportSet.name} : IDatableTemplete<{exportSet.name}>");
            sb.AppendLine("\t{");

            for (int i = 0; i < header.ItemArray.Length; i++)
            {
                string name = header.ItemArray[i].ToString();
                if (name.Equals("#") || name.Equals(String.Empty))
                {
                    continue;
                }

                // codeGen.SetProperty(header[i].ToString(), typeRow[i].ToString(), CodeGen.ACL.PUBLIC);

                sb.AppendLine($"\t\t\tpublic {GetDataType(typeRow[i].ToString())} {header[i]} {{ get; private set; }}");
            }

            // codeGen.BeginOverrideMethod("Equals", string.Empty, CodeGen.ACL.PUBLIC, new CodeGen.ParamsList(new CodeGen.Params("object", "value")));
            // codeGen.BeginConditional("if (value is null)");
            // codeGen.Return("false");
            // codeGen.EndConditional();
            // codeGen.Return("id == (int)value");

            sb.AppendLine($"\t\tpublic override bool Equals(object obj)");
            sb.AppendLine($"\t\t{{");
            sb.AppendLine($"\t\t\tif (obj is null)");
            sb.AppendLine($"\t\t\t{{");
            sb.AppendLine($"\t\t\t\treturn false;");
            sb.AppendLine($"\t\t\t}}");
            sb.AppendLine("");
            sb.AppendLine($"\t\t\tif (obj is  {exportSet.name} temp)");
            sb.AppendLine("\t\t\t{");
            sb.AppendLine($"\t\t\t\treturn this.{header.ItemArray[0].ToString()}.Equals(temp.{header.ItemArray[0].ToString()});");
            sb.AppendLine("\t\t\t}");
            sb.AppendLine("");
            sb.AppendLine($"\t\t\treturn {header.ItemArray[0].ToString()}.Equals(obj);");
            sb.AppendLine($"\t\t}}");
            // codeGen.BeginOverrideMethod("Equals", String.Empty, CodeGen.ACL.PUBLIC, new CodeGen.ParamsList(new CodeGen.Params("string", "field"), new CodeGen.Params("object", "value")));
            // codeGen.BeginSwitch("field");

            sb.AppendLine($"\t\tpublic bool Equals(string field, object value)");
            sb.AppendLine($"\t\t{{");
            sb.AppendLine($"\t\t\tswitch (field)");
            sb.AppendLine($"\t\t\t{{");
            for (int i = 0; i < header.ItemArray.Length; i++)
            {
                string name = header.ItemArray[i].ToString();
                if (name.Equals("#") || name.Equals(String.Empty))
                {
                    continue;
                }

                // codeGen.Case(header[i].ToString());
                // codeGen.Return($"{header[i]}.Equals(value)");
                sb.AppendLine($"\t\t\t\tcase \"{header[i]}\":");
                sb.AppendLine($"\t\t\t\t\treturn {header[i]}.Equals(value);");
            }

            sb.AppendLine($"\t\t\t}}");
            // codeGen.Return("false");
            sb.AppendLine($"\t\t\treturn false;");
            sb.AppendLine($"\t\t}}");
            // codeGen.BeginMethod("Dispose", String.Empty, CodeGen.ACL.PUBLIC);
            sb.AppendLine($"\t\tpublic void Dispose()");
            sb.AppendLine($"\t\t{{");
            for (int i = 0; i < header.ItemArray.Length; i++)
            {
                string name = header.ItemArray[i].ToString();
                if (name.Equals("#") || name.Equals(String.Empty))
                {
                    continue;
                }

                // codeGen.Code($"{header[i]} = default");
                sb.AppendLine($"\t\t\t{header[i]} = default;");
            }

            // codeGen.Code("GC.SuppressFinalize(this)");
            // codeGen.EndMethod();
            sb.AppendLine($"\t\t\tGC.SuppressFinalize(this);");
            sb.AppendLine($"\t\t}}");
            // codeGen.BeginStaticMethod("InitConfig", $"List<{exportSet.name}>", CodeGen.ACL.PRIVATE);
            sb.AppendLine($"\t\tprivate static List<{exportSet.name}> InitConfig()");
            sb.AppendLine("\t\t{");
            // codeGen.BeginListInstance(exportSet.name, "temp");
            sb.AppendLine($"\t\t\t return new () {{");
            for (int rowIndex = exportSet.dataRow; rowIndex < exportSet.dataTable.Rows.Count; rowIndex++)
            {
                var row = exportSet.dataTable.Rows[rowIndex];
                sb.AppendLine(GetStructData(row, header, typeRow, rowIndex));
                // codeGen.Code(GetStructData(row, header, typeRow, rowIndex));
            }

            // codeGen.EndListInstance();
            // codeGen.Return(codeGen.GetLastValueName());
            // codeGen.EndStaticMethod();
            sb.AppendLine("\t\t\t};");
            sb.AppendLine("\t\t}");
            sb.AppendLine("\t}");
            sb.AppendLine("}");

            string path = Path.Combine(AssetDatabase.GetAssetPath(exportSet.code), exportSet.name + ".cs");
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