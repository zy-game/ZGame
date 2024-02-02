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
        public List<ExportOptions> ExportList;

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
            if (ExportList is null)
            {
                ExportList = new List<ExportOptions>();
            }

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
            ExportAll();
        }

        public void Generic(params ExportOptions[] options)
        {
            if (ExportList is null)
            {
                ExportList = new List<ExportOptions>();
            }

            for (int i = 0; i < options.Length; i++)
            {
                if (options[i].isExport is false)
                {
                    continue;
                }

                ExportList.Add(options[i]);
            }

            OnSave();
            ExportAll();
        }

        private void ExportAll()
        {
            if (ExportList is null || ExportList.Count == 0)
            {
                return;
            }

            for (int i = ExportList.Count - 1; i >= 0; i--)
            {
                ExportOptions options = ExportList[i];
                if (options.dataTable is null)
                {
                    ExcelExporter exporter = GetExporter(options.parent);
                    if (exporter is null)
                    {
                        return;
                    }

                    options.dataTable = exporter.GetTable(options.name);
                }

                switch (options.type)
                {
                    case ExportType.Json:
                        ExportJson(options);
                        ExportList.Remove(options);
                        break;
                    case ExportType.Csharp:
                        ExportCSharpCode(options);
                        ExportList.Remove(options);
                        break;
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }


        // [UnityEditor.Callbacks.DidReloadScripts]
        // static void BuildCompletion()
        // {
        //     if (ExcelExportList.instance.ExportList is null || ExcelExportList.instance.ExportList.Count == 0)
        //     {
        //         return;
        //     }
        //
        //     for (int i = ExcelExportList.instance.ExportList.Count - 1; i >= 0; i--)
        //     {
        //         ExportOptions options = ExcelExportList.instance.ExportList[i];
        //         if (options.type == ExportType.Assets)
        //         {
        //             if (options.dataTable is null)
        //             {
        //                 ExcelExporter exporter = ExcelExportList.instance.GetExporter(options.parent);
        //                 if (exporter is null)
        //                 {
        //                     return;
        //                 }
        //
        //                 options.dataTable = exporter.GetTable(options.name);
        //             }
        //
        //             EditorUtility.DisplayProgressBar("正在导出数据", options.name, (float)i / ExcelExportList.instance.ExportList.Count);
        //             ExcelExportList.instance.ExportCsharpData(options);
        //             ExcelExportList.instance.ExportList.Remove(options);
        //         }
        //     }
        //
        //     ExcelExportList.OnSave();
        //     AssetDatabase.SaveAssets();
        //     AssetDatabase.Refresh();
        //     EditorUtility.ClearProgressBar();
        // }


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


            sb.AppendLine($"\tpublic sealed class {assetTypeName} : Singleton<{assetTypeName}>, IQuery<{itemTypeName}>");
            sb.AppendLine("\t{");
            sb.AppendLine($"\t\tpublic List<{itemTypeName}> cfgList = new ()\n\t\t{{");
            for (int rowIndex = exportSet.dataRow; rowIndex < exportSet.dataTable.Rows.Count; rowIndex++)
            {
                var row = exportSet.dataTable.Rows[rowIndex];
                string templete = "\t\t\tnew () {";
                for (int columnIndex = 0; columnIndex < row.ItemArray.Length; columnIndex++)
                {
                    string data = row.ItemArray[columnIndex].ToString();
                    string name = header.ItemArray[columnIndex].ToString();
                    if (name.Equals("#") || name.Equals(String.Empty))
                    {
                        continue;
                    }

                    string t = typeRow[columnIndex].ToString();
                    switch (t)
                    {
                        case "int":
                        case "float":
                            templete += $"{name} = {data}, ";
                            break;
                        case "bool":
                            templete += $"{name} = {(data == "0")}, ";
                            break;
                        default:
                            templete += $"{name} = @\"{data}\",";
                            break;
                    }
                }

                templete.Replace("\n", String.Empty);
                templete += "},";
                sb.AppendLine(templete);
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
            sb.AppendLine($"\t\tpublic {itemTypeName} Query(int key)");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t\treturn cfgList.Find(x => x.id_num == key);");
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

        void ExportCsharpData(ExportOptions options)
        {
            if (options is null || options.isExport is false)
            {
                return;
            }

            DataRow header = options.dataTable.Rows[options.headerRow];
            if (header is null)
            {
                return;
            }

            DataRow typeRow = options.dataTable.Rows[options.typeRow];
            if (typeRow is null)
            {
                return;
            }

            string assetTypeName = options.nameSpace + "." + options.name;
            string itemTypeName = options.nameSpace + "." + options.name + "_item";

            Debug.Log("creating");
            Type type = AppDomain.CurrentDomain.GetType(assetTypeName);
            Type itemType = AppDomain.CurrentDomain.GetType(itemTypeName);
            if (type is null)
            {
                return;
            }

            var obj = ScriptableObject.CreateInstance(type);
            AssetDatabase.CreateAsset(obj, Path.Combine(AssetDatabase.GetAssetPath(options.output), options.name + ".asset"));

            IList list = type.GetField("cfgList") as IList;
            if (list is null)
            {
                object temp = Activator.CreateInstance(typeof(List<>).MakeGenericType(itemType));
                list = (IList)temp;
                type.GetField("cfgList").SetValue(obj, temp);
            }

            for (int rowIndex = options.dataRow; rowIndex < options.dataTable.Rows.Count; rowIndex++)
            {
                var row = options.dataTable.Rows[rowIndex];
                var cfgData = Activator.CreateInstance(itemType);
                for (int columnIndex = 0; columnIndex < row.ItemArray.Length; columnIndex++)
                {
                    string data = row.ItemArray[columnIndex].ToString();
                    string name = header.ItemArray[columnIndex].ToString();
                    if (name.Equals("#") || name.Equals(String.Empty))
                    {
                        continue;
                    }

                    FieldInfo field = itemType.GetField(name);
                    if (field is null)
                    {
                        continue;
                    }

                    if (data.IsNullOrEmpty() is false)
                    {
                        field.SetValue(cfgData, Convert.ChangeType(data, field.FieldType));
                    }
                }

                list.Add(cfgData);
            }

            EditorUtility.SetDirty(obj);
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