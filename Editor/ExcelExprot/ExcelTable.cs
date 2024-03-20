using System;
using System.Data;

namespace ZGame.Editor.ExcelExprot
{
    [Serializable]
    public class ExcelTable
    {
        public string parent;
        public UnityEngine.Object output;
        public UnityEngine.Object code;
        public string name;
        public ExportType type;
        public string table;
        public bool isExport;
        public int dataRow = 3;
        public int typeRow = 1;
        public string nameSpace;
        public int headerRow = 0;
        [NonSerialized] public DataTable dataTable;
    }
}