using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using ExcelDataReader;
using UnityEngine;
using ZGame.Editor.Command;

namespace ZGame.Editor.ExcelExprot
{
    class ConvertExcelFileDataCommand : ICommandHandler<List<DataTable>>
    {
        public List<DataTable> OnExecute(params object[] args)
        {
            if (args is null || args.Length == 0)
            {
                return default;
            }

            string jsonFilePath = args[0].ToString();
            if (jsonFilePath.EndsWith(".json"))
            {
                return null;
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

        public void Dispose()
        {
        }

        void ICommandHandler.OnExecute(params object[] args)
        {
            throw new NotImplementedException();
        }
    }
}