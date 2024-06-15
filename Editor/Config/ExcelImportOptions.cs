using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace ZGame.Editor.ExcelExprot
{
    public enum ExportType
    {
        Json,
        Asset
    }

    [HideInInspector]
    public class ExcelImportOptions : ScriptableObject //BaseConfig<ExcelConfigList>
    {
        public UnityEngine.Object output;
        public string path;
        public int dataRowIndex = 3;
        public int typeRowIndex = 1;
        public string nameSpace;
        public int headerRowIndex = 0;
        public int descIndex = 2;
        public ExportType exportType;
        public string selection;
    }
}