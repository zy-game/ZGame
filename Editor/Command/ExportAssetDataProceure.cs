using System;
using System.Collections;
using System.Data;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using ZGame.Config;
using Object = UnityEngine.Object;

namespace ZGame.Editor
{
    public class ExportAssetDataProceure : IProcedure
    {
        public object Execute(params object[] args)
        {
            var table = args[0] as DataTable;
            int headerRowIndex = (int)args[1];
            int dataRowIndex = (int)args[2];
            int typeRowIndex = (int)args[3];
            ConfigBase output = (ConfigBase)args[4];

            //todo 通过反射关系把数据写入到asset中
            DataRow header = table.Rows[headerRowIndex];
            if (header is null)
            {
                return default;
            }

            DataRow typeRow = table.Rows[typeRowIndex];
            if (typeRow is null)
            {
                return default;
            }

            output.Config.Clear();
            Type valueType = output.Config.GetType().GenericTypeArguments.FirstOrDefault();
            FieldInfo[] fieldList = valueType.GetFields(BindingFlags.Instance | BindingFlags.Public);
            for (int rowIndex = dataRowIndex; rowIndex < table.Rows.Count; rowIndex++)
            {
                output.Config.Add(GetValue(table.Rows[rowIndex], header, typeRow, fieldList, valueType));
            }

            EditorUtility.SetDirty(output);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"{args[4].GetType().Name} 导出{output.Config.Count}条数据");
            return default;
        }

        private object GetValue(DataRow row, DataRow header, DataRow typeRow, FieldInfo[] fieldList, Type type)
        {
            object cfg = Activator.CreateInstance(type);
            for (int i = 0; i < header.ItemArray.Length; i++)
            {
                string name = header.ItemArray[i].ToString();
                if (name.Equals("#") || name.Equals(String.Empty))
                {
                    continue;
                }

                FieldInfo field = fieldList.FirstOrDefault(x => x.Name == name);
                if (field is null)
                {
                    Debug.Log($"没有找到字段{name}");
                    continue;
                }

                switch (typeRow[i].ToString())
                {
                    case "int":
                        field.SetValue(cfg, int.Parse(row[i].ToString()));
                        break;
                    case "float":
                        field.SetValue(cfg, float.Parse(row[i].ToString()));
                        break;
                    case "bool":
                        field.SetValue(cfg, row[i].ToString() == "1");
                        break;
                    case "string":
                        field.SetValue(cfg, row[i].ToString());
                        break;
                    case "int[]":
                        field.SetValue(cfg, row[i].ToString().Split(",").Select(x => int.Parse(x)).ToArray());
                        break;
                    case "float[]":
                        field.SetValue(cfg, row[i].ToString().Split(",").Select(x => float.Parse(x)).ToArray());
                        break;
                    case "bool[]":
                        field.SetValue(cfg, row[i].ToString().Split(",").Select(x => x == "1").ToArray());
                        break;
                    case "string[]":
                        field.SetValue(cfg, row[i].ToString().Split(",").ToArray());
                        break;
                    default:
                        Debug.Log($"没有找到类型{field.FieldType.Name}");
                        break;
                }
            }

            return cfg;
        }

        public void Release()
        {
        }
    }
}