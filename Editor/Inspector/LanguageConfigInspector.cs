using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using ZGame.Config;

namespace ZGame.Editor.Inspector
{
    [CustomEditor(typeof(LanguageConfig))]
    public class LanguageConfigInspector : UnityEditor.Editor
    {
        private LanguageConfig cfg;

        void OnEnable()
        {
            cfg = (LanguageConfig)target;
        }

        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("生成语言枚举"))
            {
                GenericEnum();
            }

            base.OnInspectorGUI();
        }


        private void GenericEnum()
        {
            string path = EditorUtility.OpenFolderPanel("选择文件保存路径", Application.dataPath, "");
            if (path.IsNullOrEmpty())
            {
                EditorUtility.DisplayDialog("提示", "请选择文件保存路径", "确定");
                return;
            }

            path = path + "/LanguageEnum.cs";
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"public static class LanguageEnum");
            sb.AppendLine($"{{");
            HashSet<string> ketList = new HashSet<string>();
            foreach (var VARIABLE in cfg.config)
            {
                string name = Regex.Replace(VARIABLE.en, @"[^a-zA-Z0-9\u4e00-\u9fa5\s]", "").Replace(" ", "");
                if (ketList.Contains(name))
                {
                    continue;
                }

                ketList.Add(name);
                sb.AppendLine($"\t/// {VARIABLE.zh}");
                sb.AppendLine($"\tpublic const int {name} = {VARIABLE.id};");
            }

            sb.AppendLine($"}}");

            File.WriteAllText(path, sb.ToString());
            AssetDatabase.Refresh();
        }
    }
}