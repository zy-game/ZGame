using System;
using System.Data;

namespace ZGame.Editor.ExcelExprot
{
    [Serializable]
    public class ExcelTable
    {
        public string parent;
        public string name;
        public UnityEngine.Object output;
        public UnityEngine.Object code;
        public ExportType type;
        public int dataRow = 3;
        public int typeRow = 1;
        public string nameSpace;
        public int headerRow = 0;
        public string table;
        public bool isExport;
        [NonSerialized] public DataTable dataTable;
    }
}