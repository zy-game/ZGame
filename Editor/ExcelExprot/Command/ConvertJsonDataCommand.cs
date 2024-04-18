using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ZGame.Editor.Command;

namespace ZGame.Editor.ExcelExprot
{
    class ConvertJsonDataCommand : ICommandHandler<List<DataTable>>
    {
        public void Release()
        {
        }


        public List<DataTable> OnExecute(params object[] args)
        {
            if (args is null || args.Length == 0)
            {
                return default;
            }

            string jsonFilePath = args[0].ToString();
            if (jsonFilePath.EndsWith(".json") is false)
            {
                return null;
            }

            string fileName = Path.GetFileNameWithoutExtension(jsonFilePath);
            DataSet dataSet = new DataSet();
            DataTable dataTable = new DataTable(fileName);
            dataSet.Tables.Add(dataTable);
            var data = JsonConvert.DeserializeObject<ArrayList>(File.ReadAllText(jsonFilePath));
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

            List<DataTable> _tables = new List<DataTable>();
            for (int i = 0; i < dataSet.Tables.Count; i++)
            {
                DataTable table = dataSet.Tables[i];
                if (table is null)
                {
                    continue;
                }

                _tables.Add(table);
            }

            return _tables;
        }
    }
}