using System;
using System.Data;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using ZGame.Editor.CodeGen;
using ZGame.Editor.Command;
using Object = UnityEngine.Object;

namespace ZGame.Editor.ExcelExprot
{
    class ExportCsharpCodeCommand : ICommandHandler
    {
        public void Dispose()
        {
        }

        public void OnExecute(params object[] args)
        {
            if (args is null || args.Length == 0)
            {
                return;
            }

            var table = args[0] as DataTable;
            int headerRowIndex = (int)args[1];
            int typeRowIndex = (int)args[2];
            int dataRowIndex = (int)args[3];
            Object output = (Object)args[4];
            string nameSpace = args[5].ToString();

            DataRow header = table.Rows[headerRowIndex];
            if (header is null)
            {
                return;
            }

            DataRow typeRow = table.Rows[typeRowIndex];
            if (typeRow is null)
            {
                return;
            }

            CodeGener codeGen = new CodeGener(table.TableName);
            codeGen.AddReferenceNameSpace("System", "System.Linq", "System.Collections", "System.Collections.Generic", "UnityEngine", "ZGame");
            codeGen.SetInherit(nameof(IConfigDatable));
            codeGen.SetNameSpace(nameSpace);
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
            codeGen.BeginCodeScope($"if(value is {table.TableName} temp)");
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
            codeGen.BeginMethod("InitConfig", true, false, $"List<{table.TableName}>", ACL.Private);
            codeGen.BeginCodeScope("return new () ");
            for (int rowIndex = dataRowIndex; rowIndex < table.Rows.Count; rowIndex++)
            {
                var row = table.Rows[rowIndex];
                codeGen.WriteLine(GetStructData(row, header, typeRow, rowIndex));
            }

            codeGen.EndCodeScope(";");
            codeGen.EndMethod();
            string path = Path.Combine(AssetDatabase.GetAssetPath(output), table.TableName + ".cs");
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            Debug.Log("write code:" + path);
            File.WriteAllText(path, codeGen.ToString().ToString());
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
    }
}