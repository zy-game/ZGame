using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using ZGame.Editor.Command;
using Object = UnityEngine.Object;

namespace ZGame.Editor.ExcelExprot
{
    class ExportJsonFileCommand : ICommandHandler
    {
        public void Release()
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

            List<JObject> list = new List<JObject>();
            for (int rowIndex = dataRowIndex; rowIndex < table.Rows.Count; rowIndex++)
            {
                var row = table.Rows[rowIndex];
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

            string path = Path.Combine(AssetDatabase.GetAssetPath(output), table.TableName + ".json");
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