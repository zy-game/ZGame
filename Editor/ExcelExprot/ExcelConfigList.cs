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
using ZGame.Editor.CodeGen;

namespace ZGame.Editor.ExcelExprot
{
    [ResourceReference("Assets/Settings/ExcelConfig.asset")]
    public class ExcelConfigList : BaseConfig<ExcelConfigList>
    {
        public List<ExcelFileObject> exporters;

        public override void OnAwake()
        {
            if (exporters is null)
            {
                exporters = new List<ExcelFileObject>();
            }
        }

        public ExcelFileObject GetExporter(string name)
        {
            return exporters.Find(x => x.name == name);
        }

        public void AddExporter(ExcelFileObject fileObject)
        {
            exporters.Add(fileObject);
            Save();
        }

        public void RemoveExporter(ExcelFileObject fileObject)
        {
            exporters.Remove(fileObject);
            Save();
        }

        public void GenericAll()
        {
            List<ExcelTable> ExportList = new List<ExcelTable>();
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

            Save();
            Generic(ExportList.ToArray());
        }

        public void Generic(params ExcelTable[] options)
        {
            if (options is null || options.Length == 0)
            {
                return;
            }

            foreach (var VARIABLE in options)
            {
                if (VARIABLE.dataTable is null)
                {
                    ExcelFileObject fileObject = GetExporter(VARIABLE.parent);
                    if (fileObject is null)
                    {
                        return;
                    }

                    VARIABLE.dataTable = fileObject.GetTable(VARIABLE.name);
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

            Save();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
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
            string templete = "new () {";
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

        private void ExportCSharpCode(ExcelTable exportSet)
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

            CodeGener codeGen = new CodeGener(exportSet.name);
            codeGen.AddReferenceNameSpace("System", "System.Linq", "System.Collections", "System.Collections.Generic", "UnityEngine", "ZGame");
            codeGen.SetInherit("IDatable");
            codeGen.SetNameSpace(exportSet.nameSpace);
            for (int i = 0; i < header.ItemArray.Length; i++)
            {
                string name = header.ItemArray[i].ToString();
                if (name.Equals("#") || name.Equals(String.Empty))
                {
                    continue;
                }

                codeGen.SetProperty(header[i].ToString(), typeRow[i].ToString(), CodeGen.ACL.Public);
            }

            codeGen.BeginMethod("Equals", false, true, "bool", ACL.Public, new ParamsList(new CodeGen.Params("object", "value")));
            codeGen.BeginCodeScope("if (value is null)");
            codeGen.WriteLine("return false;");
            codeGen.EndCodeScope();
            codeGen.BeginCodeScope($"if(value is {exportSet.name} temp)");
            codeGen.WriteLine($"return this.{header[0].ToString()}.Equals(temp.{header[0].ToString()});");
            codeGen.EndCodeScope();
            codeGen.WriteLine($"return {header[0].ToString()} == ({typeRow[0].ToString()})value;");
            codeGen.EndMethod();
            codeGen.BeginMethod("Equals", false, false, "bool", ACL.Public, new ParamsList(new CodeGen.Params("string", "field"), new CodeGen.Params("object", "value")));
            codeGen.BeginCodeScope("switch(field)");
            for (int i = 0; i < header.ItemArray.Length; i++)
            {
                string name = header.ItemArray[i].ToString();
                if (name.Equals("#") || name.Equals(String.Empty))
                {
                    continue;
                }

                codeGen.BeginCodeScope($"case \"{header[i].ToString()}\":");
                codeGen.WriteLine($"return {header[i]}.Equals(value);");
                codeGen.EndCodeScope();
            }

            codeGen.EndCodeScope();
            codeGen.WriteLine("return false;");
            codeGen.EndMethod();
            codeGen.BeginMethod("Dispose", false, false, String.Empty, ACL.Public);
            for (int i = 0; i < header.ItemArray.Length; i++)
            {
                string name = header.ItemArray[i].ToString();
                if (name.Equals("#") || name.Equals(String.Empty))
                {
                    continue;
                }

                codeGen.WriteLine($"{header[i]} = default;");
            }

            codeGen.WriteLine("GC.SuppressFinalize(this);");
            codeGen.EndMethod();
            codeGen.BeginMethod("InitConfig", true, false, $"List<{exportSet.name}>", ACL.Private);
            codeGen.BeginCodeScope("return new () ");
            for (int rowIndex = exportSet.dataRow; rowIndex < exportSet.dataTable.Rows.Count; rowIndex++)
            {
                var row = exportSet.dataTable.Rows[rowIndex];
                codeGen.WriteLine(GetStructData(row, header, typeRow, rowIndex));
            }

            codeGen.EndCodeScope(";");
            codeGen.EndMethod();
            string path = Path.Combine(AssetDatabase.GetAssetPath(exportSet.code), exportSet.name + ".cs");
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            Debug.Log("write code:" + path);
            File.WriteAllText(path, codeGen.ToString().ToString());
        }

        private void ExportJson(ExcelTable exportSet)
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