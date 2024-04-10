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
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using ZGame.Editor.CodeGen;

namespace ZGame.Editor.ExcelExprot
{
    [RefPath("Assets/Settings/ExcelConfig.asset")]
    public class ExcelConfigList : BaseConfig<ExcelConfigList>
    {
        [LabelText("输出目录")] public UnityEngine.Object output;
        [LabelText("输出类型")] public ExportType type;
        [LabelText("数据起始行")] public int dataRow = 3;
        [LabelText("字段类型行")] public int typeRow = 1;
        [LabelText("代码命名空间")] public string nameSpace;
        [LabelText("表头所在行")] public int headerRow = 0;
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
            return $"{name} = " + t switch
            {
                "int" => $"{v}",
                "float" => $"{v}",
                "bool" => $"{(v == "0" ? "false" : "true")}",
                "string" => $"@\"{v}\"",
                "int[]" => $"new int [] {{{v}}}",
                "float[]" => $"new float [] {{v}}",
                "bool[]" => $"new bool [] {{{string.Join(",", v.Split(",").Select(x => x == "0" ? "false" : "true"))}}}",
                "string[]" => $"new string [] {{{string.Join(",", v.Split(",").Select(x => $"@\"{x}\""))}}}"
            };
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
            codeGen.SetInherit(nameof(IConfigDatable));
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
            codeGen.BeginMethod("Release", false, true, String.Empty, ACL.Public);
            for (int i = 0; i < header.ItemArray.Length; i++)
            {
                string name = header.ItemArray[i].ToString();
                if (name.Equals("#") || name.Equals(String.Empty))
                {
                    continue;
                }

                codeGen.WriteLine($"{header[i]} = default;");
            }

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