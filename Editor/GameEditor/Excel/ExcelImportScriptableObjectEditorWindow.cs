using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using ZGame.Config;
using ZGame.Editor.ExcelExprot;
using Object = UnityEngine.Object;

namespace ZGame.Editor.Excel
{
    public class ExcelImportScriptableObjectEditorWindow : ScriptableObjectEditorWindow<ExcelImportOptions>
    {
        private List<DataTable> _tables;
        public override Type owner => typeof(ExcelHomeEditorWindow);

        public ExcelImportScriptableObjectEditorWindow(ExcelImportOptions data) : base(data)
        {
        }

        public override void OnDrawToolbar()
        {
            if (GUILayout.Button(EditorGUIUtility.FindTexture(ZStyle.DELETE_BUTTON_ICON), EditorStyles.toolbarButton))
            {
                this.OnDelete();
            }
        }

        public override void OnEnable()
        {
            if (_tables is not null)
            {
                return;
            }

            LoadTable();
        }

        private DataTable GetTable(string name)
        {
            if (_tables is null)
            {
                LoadTable();
            }

            return _tables.Find(x => x.TableName == name);
        }

        private void LoadTable()
        {
            if (this.target.path.IsNullOrEmpty())
            {
                return;
            }

            if (File.Exists(this.target.path) is false)
            {
                return;
            }

            if (this.target.path.EndsWith(".json"))
            {
                using (ConvertJsonDataProceure convert = new ConvertJsonDataProceure())
                {
                    _tables = convert.Execute(this.target.path);
                }
            }
            else
            {
                using (ConvertExcelFileDataProceure convert = new ConvertExcelFileDataProceure())
                {
                    _tables = convert.Execute(this.target.path);
                }
            }
        }

        public override void OnGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Excel文件路径", GUILayout.Width(150));
            if (GUILayout.Button(this.target.path, EditorStyles.textField))
            {
                this.target.path = EditorUtility.OpenFilePanel("选择Excel文件", "", "xlsx");
            }

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("输出文件", GUILayout.Width(150));
            this.target.output = EditorGUILayout.ObjectField(this.target.output, typeof(UnityEngine.ScriptableObject), true);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("导出类型", GUILayout.Width(150));
            this.target.exportType = (ExportType)EditorGUILayout.EnumPopup(this.target.exportType);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("数据行", GUILayout.Width(150));
            this.target.dataRowIndex = EditorGUILayout.IntField(this.target.dataRowIndex);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("类型行", GUILayout.Width(150));
            this.target.typeRowIndex = EditorGUILayout.IntField(this.target.typeRowIndex);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("命名空间", GUILayout.Width(150));
            this.target.nameSpace = EditorGUILayout.TextField(this.target.nameSpace);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("描述行", GUILayout.Width(150));
            this.target.descIndex = EditorGUILayout.IntField(this.target.descIndex);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("数据表", GUILayout.Width(150));
            if (EditorGUILayout.DropdownButton(new GUIContent(this.target.selection), FocusType.Passive))
            {
                var menu = new GenericMenu();
                foreach (var item in _tables)
                {
                    menu.AddItem(new GUIContent(item.TableName), item.TableName == this.target.selection, () => { this.target.selection = item.TableName; });
                }

                menu.ShowAsContext();
            }

            GUILayout.EndHorizontal();

            if (GUILayout.Button("导出"))
            {
                Export();
            }

            if (GUILayout.Button("创建配置表对象"))
            {
                GenericScriptableCode();
            }
        }

        public void GenericScriptableCode()
        {
            string scriptCodePath = EditorUtility.OpenFolderPanel("导出脚本", Application.dataPath, "");
            if (scriptCodePath.IsNullOrEmpty())
            {
                return;
            }

            DataTable table = GetTable(this.target.selection);
            if (table is null)
            {
                return;
            }

            var fieldRow = table.Rows[this.target.headerRowIndex];
            var fieldTypeRow = table.Rows[this.target.typeRowIndex];
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Linq;");
            sb.AppendLine("using System.Collections;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine("using ZGame;");
            sb.AppendLine("using ZGame.Config;");
            sb.AppendLine("namespace " + (this.target.nameSpace.IsNullOrEmpty() ? "Shared" : this.target.nameSpace));
            sb.AppendLine("{");
            sb.AppendLine("\tpublic class " + table.TableName + " : " + nameof(ConfigBase));
            sb.AppendLine("\t{");

            sb.AppendLine("\t\tpublic override IList Config => config;");
            sb.AppendLine("\t\tpublic List<Datable> config = new List<Datable>();");
            sb.AppendLine("\t\t[Serializable]");
            sb.AppendLine("\t\tpublic class Datable");
            sb.AppendLine("\t\t{");
            for (int i = 0; i < fieldRow.ItemArray.Length; i++)
            {
                string name = fieldRow.ItemArray[i].ToString();
                if (name.Equals("#") || name.Equals(String.Empty))
                {
                    continue;
                }

                sb.AppendLine($"\t\t\tpublic {fieldTypeRow.ItemArray[i].ToString()} {fieldRow.ItemArray[i].ToString().ToLower()};");
            }

            sb.AppendLine("\t\t}");
            sb.AppendLine("\t}");
            sb.AppendLine("}");

            EditorPrefs.SetString("GeneratorExcelScriptableObject", table.TableName);
            File.WriteAllText(scriptCodePath + "/" + table.TableName + ".cs", sb.ToString());
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public override void OnScriptCompiled()
        {
            if (EditorPrefs.HasKey("GeneratorExcelScriptableObject") is false)
            {
                return;
            }

            string tableName = EditorPrefs.GetString("GeneratorExcelScriptableObject");
            EditorPrefs.DeleteKey("GeneratorExcelScriptableObject");
            string output = EditorUtility.OpenFolderPanel("导出配置", Application.dataPath, "").Replace(Application.dataPath, "Assets");
            if (output.IsNullOrEmpty())
            {
                return;
            }

            Type cfgType = AppDomain.CurrentDomain.GetType(tableName);
            if (cfgType == null)
            {
                Debug.LogError($"{tableName} not found");
                return;
            }

            Object obj = ScriptableObject.CreateInstance(cfgType);
            if (obj == null)
            {
                Debug.LogError($"{tableName} create failed");
                return;
            }

            this.target.output = obj;
            AssetDatabase.DeleteAsset($"{output}/{tableName}.asset");
            AssetDatabase.CreateAsset(obj, $"{output}/{tableName}.asset");
            OnSave();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void Export()
        {
            if (this.target.selection.IsNullOrEmpty())
            {
                return;
            }

            switch (this.target.exportType)
            {
                case ExportType.Json:
                    using (ExportJsonFileProceure proceure = new ExportJsonFileProceure())
                    {
                        proceure.Execute(GetTable(this.target.selection), this.target.headerRowIndex, this.target.dataRowIndex, this.target.typeRowIndex, this.target.output);
                    }

                    break;
                case ExportType.Asset:
                    using (ExportAssetDataProceure proceure = new ExportAssetDataProceure())
                    {
                        proceure.Execute(GetTable(this.target.selection), this.target.headerRowIndex, this.target.dataRowIndex, this.target.typeRowIndex, this.target.output);
                    }

                    break;
            }
        }
    }
}