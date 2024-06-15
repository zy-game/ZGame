using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using ExcelDataReader;
using UnityEngine;

namespace ZGame.Editor
{
    class ConvertExcelFileDataProceure : IProcedure<List<DataTable>>
    {
        public List<DataTable> Execute(params object[] args)
        {
            if (args is null || args.Length == 0)
            {
                return default;
            }

            string jsonFilePath = args[0].ToString();
            if (jsonFilePath.EndsWith(".json") || !File.Exists(jsonFilePath))
            {
                return default;
            }

            using (var stream = File.Open(jsonFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var result = reader.AsDataSet();
                    List<DataTable> _tables = new List<DataTable>();
                    for (int i = 0; i < result.Tables.Count; i++)
                    {
                        DataTable table = result.Tables[i];
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

        public void Release()
        {
        }
    }
}