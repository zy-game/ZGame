using System.IO;
using UnityEditor;
using UnityEngine;
using ZGame.Editor.ExcelExprot;
using ZGame.Resource;

namespace ZGame.Editor.Excel
{
    public class ExcelHomeEditorWindow : HomeEditorSceneWindow
    {
        public override string name => "Excel Export";

        public override void OnDrawToolbar()
        {
            if (GUILayout.Button(EditorGUIUtility.FindTexture(ZStyle.ADD_BUTTON_ICON), EditorStyles.toolbarButton))
            {
                string excelPath = EditorUtility.OpenFilePanel("Open Excel File", Application.dataPath, "xlsx");
                if (excelPath.IsNullOrEmpty())
                {
                    return;
                }

                ExcelImportOptions options = ScriptableObject.CreateInstance<ExcelImportOptions>();
                options.path = excelPath;
                string folder = EditorUtility.OpenFolderPanel("Save Folder", Application.dataPath, "").Replace(Application.dataPath, "Assets");
                AssetDatabase.CreateAsset(options, $"{folder}/{Path.GetFileNameWithoutExtension(excelPath)}.asset");
                options.output = options;
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                EditorWindow.GetWindow<GameEditorWindow>().OnEnable();
                EditorWindow.GetWindow<GameEditorWindow>().Repaint();
            }
        }
    }
}