using UnityEditor;
using UnityEngine;
using ZGame.Config;
using ZGame.Editor.LinkerEditor;

namespace ZGame.Editor.Linker
{
    public class LinkerHomeEditorWindow : HomeEditorSceneWindow
    {
        public override string name => "Linker";


        public override void OnDrawToolbar()
        {
            if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.ADD_BUTTON_ICON), EditorStyles.toolbarButton))
            {
                string path = EditorUtility.OpenFolderPanel("Select Save Folder", Application.dataPath, "");
                if (string.IsNullOrEmpty(path))
                {
                    return;
                }

                LinkerConfig options = new LinkerConfig();
                AssetDatabase.CreateAsset(options, $"Assets/{path.Replace(Application.dataPath)}/new LinkerConfig.asset");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }
    }
}